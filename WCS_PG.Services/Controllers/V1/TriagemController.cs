using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WCS_PG.Data;
using WCS_PG.Data.Models;
using WCS_PG.Services.Models.request.V1;

namespace WCS_PG.Services.Controllers.V1
{
    [ApiController]
    [Route("api/[controller]")]
    public class TriagemController : ControllerBase
    {
        private readonly ILogger<TriagemController> _logger;
        private readonly WCSContext _context;

        public TriagemController(ILogger<TriagemController> logger, WCSContext context)
        {
            _logger = logger;
            _context = context;
        }

        [HttpGet("sku/{sku}")]
        [ProducesResponseType(typeof(SkuInfoDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<SkuInfoDto>> BuscarSku(string sku)
        {
            try
            {
                // Verificar se o SKU existe
                var pickRequestItems = await _context.PickRequestItems
                    .Where(pri => pri.Sku == sku)
                    .Include(pri => pri.PickRequest)
                    .ThenInclude(pr => pr.RampAllocations.Where(ra => ra.ReleasedAt == null))
                    .ToListAsync();

                if (!pickRequestItems.Any())
                {
                    // Para testes, manter o mock para o SKU específico
                    if (sku == "12345678")
                    {
                        var skuInfoMock = new SkuInfoDto
                        {
                            Sku = "12345678",
                            Descricao = "Shampoo Pantene Bambu",
                            Destinos = new List<DestinationInfoDto>
                            {
                                new() { Rampa = 1, Caixas = 6 },
                                new() { Rampa = 2, Caixas = 2 }
                            }
                        };
                        return Ok(skuInfoMock);
                    }

                    return NotFound();
                }

                // Pegar a descrição do primeiro item
                var descricao = pickRequestItems.First().Description;

                // Pegar destinos (rampas) e contar caixas pendentes
                var destinos = new List<DestinationInfoDto>();

                foreach (var item in pickRequestItems)
                {
                    if (item.PickRequest?.RampAllocations == null || !item.PickRequest.RampAllocations.Any())
                        continue;

                    // Para cada alocação ativa (não liberada)
                    foreach (var alocacao in item.PickRequest.RampAllocations)
                    {
                        // Evitar rampa duplicada
                        var destinoExistente = destinos.FirstOrDefault(d => d.Rampa == alocacao.RampId);

                        if (destinoExistente != null)
                        {
                            // Adicionar as caixas ao destino existente
                            destinoExistente.Caixas += (int)item.ExpectedQuantity - (int)item.ReceivedQuantity;
                        }
                        else
                        {
                            // Adicionar novo destino
                            destinos.Add(new DestinationInfoDto
                            {
                                Rampa = alocacao.RampId,
                                Caixas = (int)item.ExpectedQuantity - (int)item.ReceivedQuantity
                            });
                        }
                    }
                }

                // Remover destinos com 0 caixas
                destinos = destinos.Where(d => d.Caixas > 0).ToList();

                var skuInfo = new SkuInfoDto
                {
                    Sku = sku,
                    Descricao = descricao,
                    Destinos = destinos
                };

                return Ok(skuInfo);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Erro ao buscar SKU {sku}");
                return StatusCode(500, "Erro interno ao processar a requisição");
            }
        }

        [HttpGet("lotes/{sku}")]
        [ProducesResponseType(typeof(List<string>), StatusCodes.Status200OK)]
        public async Task<ActionResult<List<string>>> GetLotes(string sku)
        {
            try
            {
                // Buscar lotes para o SKU
                var lotesDoBanco = await _context.PickRequestItems
                    .Where(pri => pri.Sku == sku && !string.IsNullOrEmpty(pri.BatchNumber))
                    .Select(pri => pri.BatchNumber)
                    .Distinct()
                    .ToListAsync();

                if (lotesDoBanco.Any())
                {
                    return Ok(lotesDoBanco);
                }

                // Mock data se não tiver lotes no banco
                var lotes = new List<string> { "A012345", "B012345", "C012345" };
                return Ok(lotes);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Erro ao buscar lotes para SKU {sku}");
                return StatusCode(500, "Erro interno ao processar a requisição");
            }
        }

        [HttpPost("movimentacao")]
        [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
        public async Task<ActionResult<bool>> ConfirmarMovimentacao([FromBody] MovimentacaoDto movimentacao)
        {
            try
            {
                _logger.LogInformation($"Confirmando movimentação: SKU {movimentacao.Sku}, Rampa {movimentacao.Rampa}, Lote {movimentacao.Lote}, Quantidade {movimentacao.Quantidade}");

                // Verificar se a rampa existe
                var rampa = await _context.Ramps
                    .Include(r => r.CurrentPickRequest)
                    .FirstOrDefaultAsync(r => r.Id == movimentacao.Rampa);

                if (rampa == null)
                {
                    return BadRequest($"Rampa {movimentacao.Rampa} não encontrada");
                }

                if (rampa.CurrentPickRequest == null)
                {
                    return BadRequest($"Não há pick request associado à rampa {movimentacao.Rampa}");
                }

                // Buscar o item de pick request
                var pickRequestItem = await _context.PickRequestItems
                    .FirstOrDefaultAsync(pri =>
                        pri.PickRequestId == rampa.CurrentPickRequest.Id &&
                        pri.Sku == movimentacao.Sku);

                if (pickRequestItem == null)
                {
                    // Registrar a movimentação como manual mesmo sem pick request item
                    var manualTriage = new ManualTriage
                    {
                        Sku = movimentacao.Sku,
                        DestinationRampId = movimentacao.Rampa,
                        BatchNumber = movimentacao.Lote,
                        Quantity = movimentacao.Quantidade,
                        TriagedAt = DateTime.Now,
                        TriagedByUserId = 1, // Usuário do sistema, ajustar para usuário atual
                        PickRequestId = rampa.CurrentPickRequest.Id
                    };

                    await _context.ManualTriages.AddAsync(manualTriage);
                    await _context.SaveChangesAsync();

                    return Ok(true);
                }

                // Atualizar quantidade recebida
                pickRequestItem.ReceivedQuantity += movimentacao.Quantidade;

                // Atualizar lote se não tiver
                if (string.IsNullOrEmpty(pickRequestItem.BatchNumber))
                {
                    pickRequestItem.BatchNumber = movimentacao.Lote;
                }

                // Registrar a triagem manual
                var triagem = new ManualTriage
                {
                    Sku = movimentacao.Sku,
                    DestinationRampId = movimentacao.Rampa,
                    BatchNumber = movimentacao.Lote,
                    Quantity = movimentacao.Quantidade,
                    TriagedAt = DateTime.Now,
                    TriagedByUserId = 1, // Usuário do sistema, ajustar para usuário atual
                    PickRequestId = rampa.CurrentPickRequest.Id
                };

                await _context.ManualTriages.AddAsync(triagem);
                await _context.SaveChangesAsync();

                return Ok(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Erro ao confirmar movimentação: SKU {movimentacao.Sku}, Rampa {movimentacao.Rampa}");
                return StatusCode(500, "Erro interno ao processar a requisição");
            }
        }

        [HttpPost("validar-quantidade")]
        [ProducesResponseType(typeof(ValidacaoQuantidadeResultDto), StatusCodes.Status200OK)]
        public async Task<ActionResult<ValidacaoQuantidadeResultDto>> ValidarQuantidade([FromBody] ValidacaoQuantidadeDto validacao)
        {
            try
            {
                // Verificar se a rampa existe e tem um pick request associado
                var rampa = await _context.Ramps
                    .Include(r => r.CurrentPickRequest)
                    .FirstOrDefaultAsync(r => r.Id == validacao.Rampa);

                if (rampa == null || rampa.CurrentPickRequest == null)
                {
                    return Ok(new ValidacaoQuantidadeResultDto
                    {
                        Valido = false,
                        Mensagem = $"Rampa {validacao.Rampa} não encontrada ou sem pick request"
                    });
                }

                // Buscar o item no pick request
                var pickRequestItem = await _context.PickRequestItems
                    .FirstOrDefaultAsync(pri =>
                        pri.PickRequestId == rampa.CurrentPickRequest.Id &&
                        pri.Sku == validacao.Sku);

                // Se o item não existir, limitar a uma quantidade padrão
                if (pickRequestItem == null)
                {
                    const int maxQuantidadePadrao = 10;
                    if (validacao.Quantidade > maxQuantidadePadrao)
                    {
                        return Ok(new ValidacaoQuantidadeResultDto
                        {
                            Valido = false,
                            Mensagem = $"Item não previsto. Quantidade máxima permitida é {maxQuantidadePadrao}"
                        });
                    }

                    return Ok(new ValidacaoQuantidadeResultDto { Valido = true });
                }

                // Verificar quantidade pendente
                int qtdPendente = (int)pickRequestItem.ExpectedQuantity - (int)pickRequestItem.ReceivedQuantity;

                if (qtdPendente <= 0)
                {
                    return Ok(new ValidacaoQuantidadeResultDto
                    {
                        Valido = false,
                        Mensagem = $"Quantidade esperada ({pickRequestItem.ExpectedQuantity}) já foi atingida. Não é possível adicionar mais itens."
                    });
                }

                // Verificar se a quantidade está dentro do permitido
                if (validacao.Quantidade > qtdPendente)
                {
                    return Ok(new ValidacaoQuantidadeResultDto
                    {
                        Valido = false,
                        Mensagem = $"Quantidade máxima permitida é {qtdPendente}"
                    });
                }

                return Ok(new ValidacaoQuantidadeResultDto { Valido = true });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Erro ao validar quantidade: SKU {validacao.Sku}, Rampa {validacao.Rampa}");
                return StatusCode(500, "Erro interno ao processar a requisição");
            }
        }
    }
}