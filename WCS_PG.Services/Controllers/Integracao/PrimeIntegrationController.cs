using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using WCS_PG.Data.Models;
using WCS_PG.Data;
using WCS_PG.Services.Models.request.Integracao;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using WCS_PG.Services.Models.request.V1;
using Microsoft.AspNetCore.Authorization;
using System.Text.Json;
using System.Net.Http.Json;

namespace WCS_PG.Services.Controllers.Integracao
{
    /// <summary>
    /// Controller para integração com o PrIME
    /// </summary>
    [ApiController]
    [ApiVersion("1.1")]
    [Route("api/v{version:apiVersion}/prime")]
    public class PrimeIntegrationController : ControllerBase
    {
        private readonly ILogger<PrimeIntegrationController> _logger;
        private readonly WCSContext _context;
        private readonly IConfiguration _configuration;
        private readonly IHttpClientFactory _httpClientFactory;

        public PrimeIntegrationController(
            ILogger<PrimeIntegrationController> logger,
            WCSContext context,
            IConfiguration configuration,
            IHttpClientFactory httpClientFactory)
        {
            _logger = logger;
            _context = context;
            _configuration = configuration;
            _httpClientFactory = httpClientFactory;
        }

        /// <summary>
        /// Recebe solicitação de pick request do PrIME
        /// </summary>
        /// <param name="request">Dados do pick request</param>
        /// <returns>Status 200 se processado com sucesso</returns>
        [HttpPost("var-list-pick-request")]
        [MapToApiVersion("1.1")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [Authorize]
        public async Task<IActionResult> ReceivePickRequest([FromBody] VarListPickRequestDto request)
        {
            try
            {
                _logger.LogInformation($"Recebendo VAR_LIST_PICK_REQUEST - TranId: {request.VAR_LIST_PICK_REQUEST.CTRL_SEG.TRANID}");

                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                // Processamento direto aqui
                var pickRequest = new PickRequest
                {
                    Id = request.VAR_LIST_PICK_REQUEST.CTRL_SEG.TRANID.ToString(),
                    OrderNumber = request.VAR_LIST_PICK_REQUEST.CTRL_SEG.PALLET_SEG.ORDNUM,
                    StopId = request.VAR_LIST_PICK_REQUEST.CTRL_SEG.PALLET_SEG.STOP_NAM,
                    CarMoveId = request.VAR_LIST_PICK_REQUEST.CTRL_SEG.PALLET_SEG.CAR_MOVE_ID, // Adicionando o CAR_MOVE_ID
                    DeliveryType = "Multiplos", // Verificar de onde vem esta informação
                    ClientName = request.VAR_LIST_PICK_REQUEST.CTRL_SEG.PALLET_SEG.RTCUST,
                    Status = "Pendente",
                    CreatedAt = DateTime.UtcNow,
                    Customization = ""
                };

                await _context.PickRequests.AddAsync(pickRequest);

                foreach (var pickSeg in request.VAR_LIST_PICK_REQUEST.CTRL_SEG.PALLET_SEG.PICK_SEG)
                {
                    var pickRequestItem = new PickRequestItem
                    {
                        PickRequestId = pickRequest.Id,
                        Sku = pickSeg.PRTNUM,
                        ExpectedQuantity = pickSeg.PCKQTY,
                        InducedQuantity = 0,
                        ReceivedQuantity = 0,
                        BatchNumber = "", // Será preenchido depois
                        NoReadRejectionCount = 0,
                        FinalRejectionCount = 0,
                        PendingCount = pickSeg.PCKQTY,
                        WorkReference = pickSeg.WRKREF,
                        PackageUnit = pickSeg.PCK_UOM
                    };

                    await _context.PickRequestItems.AddAsync(pickRequestItem);
                }

                await _context.SaveChangesAsync();

                return Ok($"Sucesso - VAR_LIST_PICK_REQUEST - TranId: {request.VAR_LIST_PICK_REQUEST.CTRL_SEG.TRANID} at {DateTime.Now}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao processar VAR_LIST_PICK_REQUEST");
                return StatusCode(500, "Erro interno ao processar requisição");
            }
        }




        /// <summary>
        /// Recebe confirmação de indução do PrIME
        /// </summary>
        /// <param name="request">Dados da indução</param>
        /// <returns>Status 200 se processado com sucesso</returns>
        [HttpPost("induction")]
        [MapToApiVersion("1.1")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [Authorize]
        public async Task<IActionResult> ReceiveInduction([FromBody] InductionDto request)
        {
            try
            {
                _logger.LogInformation($"Recebendo INDUCTION - TranId: {request.INDUCTION.CTRL_SEG.TRANID}");

                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                // Extrair SKU e quantidade induzida e parametros
                var skuInduzido = request.INDUCTION.CTRL_SEG.MOVE_REQ_SEG.INV_SEG?.PRTNUM;
                var loteSkuInduzido = request.INDUCTION.CTRL_SEG.MOVE_REQ_SEG.INV_SEG?.LOTNUM;
                var qtdInduzida = 0;
                var assetType = request.INDUCTION.CTRL_SEG.MOVE_REQ_SEG?.ASSET_TYP;
                var sourcePallet = request.INDUCTION.CTRL_SEG.MOVE_REQ_SEG?.LODNUM;

                if (request.INDUCTION.CTRL_SEG.MOVE_REQ_SEG.INV_SEG != null)
                {
                    int.TryParse(request.INDUCTION.CTRL_SEG.MOVE_REQ_SEG.INV_SEG.UNTQTY, out qtdInduzida);
                }

                if (string.IsNullOrEmpty(skuInduzido) || qtdInduzida <= 0)
                {
                    return BadRequest("SKU ou quantidade inválida");
                }

                // Buscar todos os pick requests ativos em waves ativas
                var wavesAtivas = await _context.Waves
                    .Where(w => w.Status == "Em Execução")
                    .ToListAsync();

                if (!wavesAtivas.Any())
                {
                    return BadRequest("Não há waves ativas para receber indução");
                }

                var waveIds = wavesAtivas.Select(w => w.Id).ToList();

                // Buscar pick requests associados às waves ativas
                var wavePickRequests = await _context.WavePickRequests
                    .Where(wpr => waveIds.Contains(wpr.WaveId))
                    .Select(wpr => wpr.PickRequestId)
                    .ToListAsync();

                // Buscar todos os itens do SKU induzido nos pick requests ativos
                var itens = await _context.PickRequestItems
                    .Include(pri => pri.PickRequest)
                    .Where(pri =>
                        wavePickRequests.Contains(pri.PickRequestId) &&
                        pri.Sku == skuInduzido &&
                        pri.PickRequest.Status == "Em Execução" &&
                        pri.InducedQuantity < pri.ExpectedQuantity)
                    .ToListAsync();

                if (!itens.Any())
                {
                    _logger.LogWarning($"Não foram encontrados itens pendentes para o SKU {skuInduzido}");
                    return Ok(); // Aceita a indução mesmo sem itens correspondentes
                }

                // Distribuir a quantidade induzida entre os itens pendentes
                var qtdRestante = qtdInduzida;

                foreach (var item in itens)
                {
                    item.AssetType = assetType;
                    item.SourcePallet = sourcePallet;

                    int qtdPendente = (int)(item.ExpectedQuantity! - item.InducedQuantity!);

                    if (qtdPendente <= 0)
                        continue;

                    var qtdAtribuir = Math.Min(qtdRestante, qtdPendente);

                    if (qtdAtribuir <= 0)
                        break;

                    item.InducedQuantity += qtdAtribuir;

                    // Atualizar lote se ainda não definido
                    if (string.IsNullOrEmpty(item.BatchNumber) && !string.IsNullOrEmpty(loteSkuInduzido))
                    {
                        item.BatchNumber = loteSkuInduzido;
                    }

                    qtdRestante -= qtdAtribuir;

                    // Atualizar status do pick request se necessário
                    if (item.PickRequest.Status != "Em Execução")
                    {
                        item.PickRequest.Status = "Em Execução";
                        item.PickRequest.StartedAt ??= DateTime.UtcNow;
                    }

                    if (qtdRestante <= 0)
                        break;
                }

                await _context.SaveChangesAsync();
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao processar INDUCTION");
                return StatusCode(500, "Erro interno ao processar requisição");
            }
        }


        /// <summary>
        /// Recebe solicitação de remoção de associação rampa x pick request
        /// </summary>
        /// <param name="request">Dados da remoção</param>
        /// <returns>Status 200 se processado com sucesso</returns>
        [HttpPost("removal")]
        [MapToApiVersion("1.1")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [Authorize]
        public async Task<IActionResult> ReceiveRemoval([FromBody] RemovalDto request)
        {
            try
            {
                _logger.LogInformation($"Recebendo REMOVAL - TranId: {request.REMOVAL.CTRL_SEG.TRANID}");

                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                // Buscar a rampa baseada na localização
                var ramp = await _context.Ramps
                    .Include(r => r.CurrentPickRequest)
                    .FirstOrDefaultAsync(r => r.RampNumber == request.REMOVAL.CTRL_SEG.REMOVAL_SEG.STOLOC);

                if (ramp == null)
                {
                    return BadRequest($"Rampa não encontrada: {request.REMOVAL.CTRL_SEG.REMOVAL_SEG.STOLOC}");
                }

                if (ramp.CurrentPickRequest != null)
                {
                    // Criar registro de desalocação
                    var rampAllocation = new RampAllocation
                    {
                        RampId = ramp.Id,
                        PickRequestId = ramp.CurrentPickRequest.Id,
                        ReleasedAt = DateTime.UtcNow,
                        ReleasedByUserId = null // Sistema
                    };

                    await _context.RampAllocations.AddAsync(rampAllocation);

                    // Limpar referência atual
                    ramp.CurrentPickRequestId = null;
                    ramp.Status = "Disponível";
                }

                await _context.SaveChangesAsync();

                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao processar REMOVAL");
                return StatusCode(500, "Erro interno ao processar requisição");
            }
        }

        /// <summary>
        /// Recebe solicitação de cancelamento de pick request
        /// </summary>
        /// <param name="request">Dados da cancelamento</param>
        /// <returns>Status 200 se processado com sucesso</returns>
        [HttpPost("pick-cancel")]
        [MapToApiVersion("1.1")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [Authorize]
        public async Task<IActionResult> ReceivePickCancel([FromBody] PickCancelDto request)
        {
            try
            {
                _logger.LogInformation($"Recebendo REMOVAL - TranId: {request.PICK_CANCEL.CTRL_SEG.TRANID}");

                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                // Buscar a pick request
                var _pickitem = await _context.PickRequestItems.FirstOrDefaultAsync(_ => _.Description == request.PICK_CANCEL.CTRL_SEG.PCK_CAN_SEG.WRKREF);

                if (_pickitem == null)
                {
                    return BadRequest($"Item não encontrado: {request.PICK_CANCEL.CTRL_SEG.PCK_CAN_SEG.WRKREF}");
                }

                _pickitem.ExpectedQuantity = 0;

                await _context.SaveChangesAsync();

                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao processar REMOVAL");
                return StatusCode(500, "Erro interno ao processar requisição");
            }
        }

        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto LoginModel)
        {
            try
            {
                var databaseName = _context.Database.GetDbConnection().Database;
                User? _user = await _context.Users.Where(srch => srch.Username == LoginModel.UserName).FirstOrDefaultAsync();
                if (_user == null)
                {
                    return BadRequest("Usuário inválido");
                }
                if (_user.PasswordHash != LoginModel.Password)
                {
                    return BadRequest("Senha inválida");
                }

                var key = Encoding.ASCII.GetBytes(_configuration.GetSection("Encryption:Secret").Value!);
                var tokenDescriptor = new SecurityTokenDescriptor
                {
                    Subject = new ClaimsIdentity(new[]
                    {
                new Claim(JwtRegisteredClaimNames.Sub, _user.Name),
                new Claim(JwtRegisteredClaimNames.Name, _user.Username),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString("N")),
                new Claim("ID", _user.Id.ToString()),
                new Claim("IP", HttpContext.Connection.RemoteIpAddress.ToString())
            }),
                    Expires = DateTime.UtcNow.AddDays(1),
                    SigningCredentials = new SigningCredentials
                    (new SymmetricSecurityKey(key),
                    SecurityAlgorithms.HmacSha512Signature)
                };
                var tokenHandler = new JwtSecurityTokenHandler();
                var token = tokenHandler.CreateToken(tokenDescriptor);
                var stringToken = tokenHandler.WriteToken(token);

                // Determinar se a requisição é do PrIME
                bool isPrimeRequest = HttpContext.Request.Path.ToString().Contains("prime", StringComparison.OrdinalIgnoreCase);

                if (isPrimeRequest)
                {
                    // Configurar o cookie para o PrIME
                    var cookieOptions = new CookieOptions
                    {
                        HttpOnly = false,
                        Secure = HttpContext.Request.IsHttps,
                        SameSite = SameSiteMode.Lax,
                        Expires = DateTime.UtcNow.AddDays(1)
                    };
                    Response.Cookies.Append("PrIME_Auth", stringToken, cookieOptions);

                    return Ok(new { message = "Autenticação bem-sucedida", username = _user.Username });
                }
                else
                {
                    // Retornar o token JWT para outros clientes
                    var authProperties = new AuthenticationProperties
                    {
                        IsPersistent = true
                    };
                    return Ok(new { token = stringToken });
                }
            }
            catch (Exception _ex)
            {
                return StatusCode(500, _ex.Message);
            }
        }

        /// <summary>
        /// Envia  solicitação de heartbeat do PrIME
        /// </summary>
        /// <param name="request">Dados do heartbeat</param>
        /// <returns>Status 200 se processado com sucesso</returns>
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [HttpPost("heartbeat-initiate-test")]
        public async Task<IActionResult> ReceiveHeartbeatInitiateTest([FromBody] HeartbeatInitiateDto request)
        {
            try
            {
                _logger.LogInformation($"Eviando HEARTBEAT_INITIATE - TranId: {request.HEARTBEAT_INITIATE.CTRL_SEG.TRANID}");

                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                request.HEARTBEAT_INITIATE.CTRL_SEG.TRANID = long.Parse(Guid.NewGuid().ToString("N").Substring(0, 20));
                request.HEARTBEAT_INITIATE.CTRL_SEG.TRANDT = long.Parse(DateTime.Now.ToString("yyyyMMddHHmmss"));
                request.HEARTBEAT_INITIATE.CTRL_SEG.WCS_ID = _configuration["PrimeApi:WcsId"];
                request.HEARTBEAT_INITIATE.CTRL_SEG.WH_ID = _configuration["PrimeApi:WhId"];

                // Criar o cliente HTTP para comunicação com o PrIME
                var client = _httpClientFactory.CreateClient("PrimeApi");
                client.BaseAddress = new Uri(_configuration["PrimeApi:BaseUrl"]);

                // 1. Primeiro fazer login
                string loginEndpoint = _configuration["PrimeApi:Endpoints:Login"];

                // Obter credenciais do appsettings
                var loginCredentials = new
                {
                    usr_id = _configuration["PrimeApi:Credentials:Username"],
                    password = _configuration["PrimeApi:Credentials:Password"]
                };

                // Fazer login no PrIME
                var loginResponse = await client.PostAsJsonAsync(loginEndpoint, loginCredentials);

                if (!loginResponse.IsSuccessStatusCode)
                {
                    string loginErrorContent = await loginResponse.Content.ReadAsStringAsync();
                    _logger.LogError($"Erro ao fazer login no PrIME - Status: {loginResponse.StatusCode}, Erro: {loginErrorContent}");
                    return StatusCode(500, "Erro ao autenticar com o PrIME");
                }



                // 4. Enviar a resposta para o PrIME
                string heartbeatEndpoint = _configuration["PrimeApi:Endpoints:HeartbeatConfirm"];
                var heartbeatRequest = new HttpRequestMessage(HttpMethod.Post, heartbeatEndpoint);
                heartbeatRequest.Content = new StringContent(Newtonsoft.Json.JsonConvert.SerializeObject(request), Encoding.UTF8, "application/json");
                heartbeatRequest.Headers.TransferEncodingChunked = true;

                var heartbeatResponse = await client.SendAsync(heartbeatRequest);

                // Verificar o resultado
                if (heartbeatResponse.IsSuccessStatusCode)
                {
                    _logger.LogInformation($"HEARTBEAT_INITIATE enviado com sucesso - TranId: {request.HEARTBEAT_INITIATE.CTRL_SEG.TRANID}");
                }
                else
                {
                    string errorContent = await heartbeatResponse.Content.ReadAsStringAsync();
                    _logger.LogError($"Erro na requisição - Status: {heartbeatResponse.StatusCode}");
                    _logger.LogError($"Headers da resposta: {string.Join(", ", heartbeatResponse.Headers.Select(h => $"{h.Key}: {string.Join(", ", h.Value)}"))}");
                    _logger.LogError($"Erro: {errorContent}");
                    return StatusCode((int)heartbeatResponse.StatusCode,
                        $"Erro ao enviar requisição: Status={heartbeatResponse.StatusCode}, Erro={errorContent}");
                }

                return Ok(await heartbeatResponse.Content.ReadAsStringAsync());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao processar HEARTBEAT_INITIATE");
                return StatusCode(500, "Erro interno ao processar requisição: " );
            }
        }

        /// <summary>
        /// Envia  solicitação de heartbeat do PrIME
        /// </summary>
        /// <param name="request">Dados do heartbeat</param>
        /// <returns>Status 200 se processado com sucesso</returns>
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [HttpPost("heartbeat-initiate")]
        public async Task<IActionResult> ReceiveHeartbeatInitiate([FromBody] HeartbeatInitiateDto request)
        {
            try
            {
                _logger.LogInformation($"Eviando HEARTBEAT_INITIATE - TranId: {request.HEARTBEAT_INITIATE.CTRL_SEG.TRANID}");

                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                // Criar o cliente HTTP para comunicação com o PrIME
                var client = _httpClientFactory.CreateClient("PrimeApi");
                client.BaseAddress = new Uri(_configuration["PrimeApi:BaseUrl"]);

                // 1. Primeiro fazer login
                string loginEndpoint = _configuration["PrimeApi:Endpoints:Login"];

                // Obter credenciais do appsettings
                var loginCredentials = new
                {
                    usr_id = _configuration["PrimeApi:Credentials:Username"],
                    password = _configuration["PrimeApi:Credentials:Password"]
                };

                // Fazer login no PrIME
                var loginResponse = await client.PostAsJsonAsync(loginEndpoint, loginCredentials);

                if (!loginResponse.IsSuccessStatusCode)
                {
                    string loginErrorContent = await loginResponse.Content.ReadAsStringAsync();
                    _logger.LogError($"Erro ao fazer login no PrIME - Status: {loginResponse.StatusCode}, Erro: {loginErrorContent}");
                    return StatusCode(500, "Erro ao autenticar com o PrIME");
                }

                var response = new HeartbeatConfirmDto
                {
                    HEARTBEAT_CONFIRM = new HeartbeatControlDto
                    {
                        CTRL_SEG = new HeartbeatControlSegmentDto
                        {
                            TRANID = request.HEARTBEAT_INITIATE.CTRL_SEG.TRANID,
                            TRANDT = Int64.Parse(DateTime.Now.ToString("yyyyMMddHHmmss")),
                            WCS_ID = request.HEARTBEAT_INITIATE.CTRL_SEG.WCS_ID,
                            WH_ID = request.HEARTBEAT_INITIATE.CTRL_SEG.WH_ID,
                            HEARTBEAT_SEG = new HeartbeatSegmentDto
                            {
                                TEXT = request.HEARTBEAT_INITIATE.CTRL_SEG.HEARTBEAT_SEG.TEXT
                            }
                        }
                    }
                };


                // 4. Enviar a resposta para o PrIME
                string heartbeatEndpoint = _configuration["PrimeApi:Endpoints:HeartbeatConfirm"];
                var heartbeatRequest = new HttpRequestMessage(HttpMethod.Post, heartbeatEndpoint);
                heartbeatRequest.Content = new StringContent(Newtonsoft.Json.JsonConvert.SerializeObject(response), Encoding.UTF8, "application/json");
                heartbeatRequest.Headers.TransferEncodingChunked = true;

                var heartbeatResponse = await client.SendAsync(heartbeatRequest);

                // Verificar o resultado
                if (heartbeatResponse.IsSuccessStatusCode)
                {
                    _logger.LogInformation($"HEARTBEAT_INITIATE recebido com sucesso - TranId: {request.HEARTBEAT_INITIATE.CTRL_SEG.TRANID}");
                }
                else
                {
                    string errorContent = await heartbeatResponse.Content.ReadAsStringAsync();
                    _logger.LogError($"Erro ao enviar HEARTBEAT_INITIATE - Status: {heartbeatResponse.StatusCode}, Erro: {errorContent}");
                    return StatusCode(500, "Erro ao enviar HEARTBEAT_INITIATE para o PrIME:");
                }

                return Ok(await heartbeatResponse.Content.ReadAsStringAsync());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao processar HEARTBEAT_INITIATE");
                return StatusCode(500, "Erro interno ao processar requisição: ");
            }
        }

        /// <summary>
        /// Recebe solicitação de heartbeat do PrIME e confirma
        /// </summary>
        /// <param name="request">Dados do heartbeat</param>
        /// <returns>Status 200 se processado com sucesso</returns>
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [HttpPost("heartbeat-confirm")]
        public async Task<IActionResult> ReceiveHeartbeatConfirm([FromBody] HeartbeatConfirmDto request)
        {
            try
            {
                _logger.LogInformation($"Recebendo HEARTBEAT_INITIATE - TranId: {request.HEARTBEAT_CONFIRM.CTRL_SEG.TRANID}");

                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                // Criar resposta para o heartbeat confirm
                var response = new HeartbeatConfirmDto
                {
                    HEARTBEAT_CONFIRM = new HeartbeatControlDto
                    {
                        CTRL_SEG = new HeartbeatControlSegmentDto
                        {
                            TRANID = request.HEARTBEAT_CONFIRM.CTRL_SEG.TRANID,
                            TRANDT = Int64.Parse(DateTime.Now.ToString("yyyyMMddHHmmss")),
                            WCS_ID = request.HEARTBEAT_CONFIRM.CTRL_SEG.WCS_ID,
                            WH_ID = request.HEARTBEAT_CONFIRM.CTRL_SEG.WH_ID,
                            HEARTBEAT_SEG = new HeartbeatSegmentDto
                            {
                                TEXT = request.HEARTBEAT_CONFIRM.CTRL_SEG.HEARTBEAT_SEG.TEXT
                            }
                        }
                    }
                };

                return Ok(new { message = "Confirm recebido em: " + DateTime.Now.ToString("yyyyMMddHHmmss") + " TRANID: " + request.HEARTBEAT_CONFIRM.CTRL_SEG.TRANID });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao processar HEARTBEAT_INITIATE");
                return StatusCode(500, "Erro interno ao processar requisição");
            }
        }


        /// <summary>
        /// Testar pick Confirm do PrIME
        /// </summary>
        /// <param name="request">Dados do request</param>
        /// <returns>Status 200 se processado com sucesso</returns>
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [HttpPost("pick-confirm-test")]
        public async Task<IActionResult> PickConfirmTest([FromBody] VarListPickConfirmDto request)
        {
            try
            {
                _logger.LogInformation($"Eviando PICK_CONFIRM - TranId: {request.VAR_LIST_PICK_CONFIRM.CTRL_SEG.TRANID}");

                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                request.VAR_LIST_PICK_CONFIRM.CTRL_SEG.TRANDT = long.Parse(DateTime.Now.ToString("yyyyMMddHHmmss"));
                request.VAR_LIST_PICK_CONFIRM.CTRL_SEG.WCS_ID = _configuration["PrimeApi:WcsId"];
                request.VAR_LIST_PICK_CONFIRM.CTRL_SEG.WH_ID = _configuration["PrimeApi:WhId"];

                // Criar o cliente HTTP para comunicação com o PrIME
                var client = _httpClientFactory.CreateClient("PrimeApi");
                client.BaseAddress = new Uri(_configuration["PrimeApi:BaseUrl"]);

                // 1. Primeiro fazer login
                string loginEndpoint = _configuration["PrimeApi:Endpoints:Login"];

                // Obter credenciais do appsettings
                var loginCredentials = new
                {
                    usr_id = _configuration["PrimeApi:Credentials:Username"],
                    password = _configuration["PrimeApi:Credentials:Password"]
                };

                // Fazer login no PrIME
                var loginResponse = await client.PostAsJsonAsync(loginEndpoint, loginCredentials);

                if (!loginResponse.IsSuccessStatusCode)
                {
                    string loginErrorContent = await loginResponse.Content.ReadAsStringAsync();
                    _logger.LogError($"Erro ao fazer login no PrIME - Status: {loginResponse.StatusCode}, Erro: {loginErrorContent}");
                    return StatusCode(500, "Erro ao autenticar com o PrIME");
                }



                // 4. Enviar a resposta para o PrIME
                string Endpoint = _configuration["PrimeApi:Endpoints:PickConfirm"];
                var _request = new HttpRequestMessage(HttpMethod.Post, Endpoint);
                _request.Content = new StringContent(Newtonsoft.Json.JsonConvert.SerializeObject(request), Encoding.UTF8, "application/json");
                _request.Headers.TransferEncodingChunked = true;

                var _response = await client.SendAsync(_request);

                // Verificar o resultado
                if (_response.IsSuccessStatusCode)
                {
                    _logger.LogInformation($"Pick Confirm enviado com sucesso - TranId: {request.VAR_LIST_PICK_CONFIRM.CTRL_SEG.TRANID}");
                }
                else
                {
                    string errorContent = await _response.Content.ReadAsStringAsync();
                    _logger.LogError($"Erro na requisição - Status: {_response.StatusCode}");
                    _logger.LogError($"Headers da resposta: {string.Join(", ", _response.Headers.Select(h => $"{h.Key}: {string.Join(", ", h.Value)}"))}");
                    _logger.LogError($"Erro: {errorContent}");
                    return StatusCode((int)_response.StatusCode,
                        $"Erro ao enviar requisição: Status={_response.StatusCode}, Erro={errorContent}");
                }

                return Ok(await _response.Content.ReadAsStringAsync());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao processar HEARTBEAT_INITIATE");
                return StatusCode(500, "Erro interno ao processar requisição: ");
            }
        }


        /// <summary>
        /// Testar pick Error do PrIME
        /// </summary>
        /// <param name="request">Dados do request</param>
        /// <returns>Status 200 se processado com sucesso</returns>
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [HttpPost("pick-error-test")]
        public async Task<IActionResult> PickErrorTest([FromBody] PickErrorDto request)
        {
            try
            {
                _logger.LogInformation($"Eviando PICK_CONFIRM - TranId: {request.PICK_ERROR.CTRL_SEG.TRANID}");

                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                // Criar o cliente HTTP para comunicação com o PrIME
                var client = _httpClientFactory.CreateClient("PrimeApi");
                client.BaseAddress = new Uri(_configuration["PrimeApi:BaseUrl"]);

                // 1. Primeiro fazer login
                string loginEndpoint = _configuration["PrimeApi:Endpoints:Login"];

                // Obter credenciais do appsettings
                var loginCredentials = new
                {
                    usr_id = _configuration["PrimeApi:Credentials:Username"],
                    password = _configuration["PrimeApi:Credentials:Password"]
                };

                // Fazer login no PrIME
                var loginResponse = await client.PostAsJsonAsync(loginEndpoint, loginCredentials);

                if (!loginResponse.IsSuccessStatusCode)
                {
                    string loginErrorContent = await loginResponse.Content.ReadAsStringAsync();
                    _logger.LogError($"Erro ao fazer login no PrIME - Status: {loginResponse.StatusCode}, Erro: {loginErrorContent}");
                    return StatusCode(500, "Erro ao autenticar com o PrIME");
                }



                // 4. Enviar a resposta para o PrIME
                string Endpoint = _configuration["PrimeApi:Endpoints:PickError"];
                var _request = new HttpRequestMessage(HttpMethod.Post, Endpoint);
                _request.Content = new StringContent(Newtonsoft.Json.JsonConvert.SerializeObject(request), Encoding.UTF8, "application/json");
                _request.Headers.TransferEncodingChunked = true;

                var _response = await client.SendAsync(_request);

                // Verificar o resultado
                if (_response.IsSuccessStatusCode)
                {
                    _logger.LogInformation($"Pick Confirm enviado com sucesso - TranId: {request.PICK_ERROR.CTRL_SEG.TRANID}");
                }
                else
                {
                    string errorContent = await _response.Content.ReadAsStringAsync();
                    _logger.LogError($"Erro na requisição - Status: {_response.StatusCode}");
                    _logger.LogError($"Headers da resposta: {string.Join(", ", _response.Headers.Select(h => $"{h.Key}: {string.Join(", ", h.Value)}"))}");
                    _logger.LogError($"Erro: {errorContent}");
                    return StatusCode((int)_response.StatusCode,
                        $"Erro ao enviar requisição: Status={_response.StatusCode}, Erro={errorContent}");
                }

                return Ok(await _response.Content.ReadAsStringAsync());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao processar HEARTBEAT_INITIATE");
                return StatusCode(500, "Erro interno ao processar requisição: ");
            }
        }
    }



}
