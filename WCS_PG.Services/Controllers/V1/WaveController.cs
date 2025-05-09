using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WCS_PG.Data;
using WCS_PG.Services.Models.request.V1;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using WCS_PG.Data.Models;
using WCS_PG.Services.Services;

namespace WCS_PG.Services.Controllers.V1
{
    [ApiController]
    [Route("api/[controller]")]
    public class WaveController : ControllerBase
    {
        private readonly ILogger<WaveController> _logger;
        private readonly WCSContext _context;
        private readonly IConfiguration _configuration;
        private readonly ClpCommunicationService _clpService; // Adicionar este campo

        public WaveController(
            ILogger<WaveController> logger,
            WCSContext context,
            IConfiguration configuration,
            ClpCommunicationService clpService) // Adicionar este parâmetro
        {
            _logger = logger;
            _context = context;
            _configuration = configuration;
            _clpService = clpService; // Inicializar o campo
        }

        [HttpGet("pickrequests")]
        [ProducesResponseType(typeof(List<WavePickRequestDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<List<WavePickRequestDto>>> GetPickRequests([FromQuery] string groupingField = "STOP_NAM")
        {
            try
            {
                // Buscar pick requests pendentes (não associados a waves)
                var pickRequestsDb = await _context.PickRequests
                    .Where(pr => pr.Status == "Pendente")
                    .ToListAsync();

                // Verificar se já estão associados a alguma wave
                var pickRequestsNaWave = await _context.WavePickRequests
                    .Select(wpr => wpr.PickRequestId)
                    .ToListAsync();

                // Filtrar apenas os que não estão em nenhuma wave
                var pickRequestsDisponiveis = pickRequestsDb
                    .Where(pr => !pickRequestsNaWave.Contains(pr.Id))
                    .ToList();

                var requests = new List<WavePickRequestDto>();

                if (pickRequestsDisponiveis.Any())
                {
                    foreach (var pr in pickRequestsDisponiveis)
                    {
                        // Obter rampa se já estiver alocada
                        var rampaAtual = await _context.RampAllocations
                            .Where(ra => ra.PickRequestId == pr.Id && ra.ReleasedAt == null)
                            .Select(ra => (int?)ra.RampId)
                            .FirstOrDefaultAsync();

                        requests.Add(new WavePickRequestDto
                        {
                            Embarque = pr.OrderNumber,
                            StopId = pr.StopId,
                            CarMoveId = pr.CarMoveId, // Adicionando o CAR_MOVE_ID
                            Delivery = pr.DeliveryType,
                            Cliente = pr.ClientName,
                            Particularidade = pr.Customization,
                            Quantidade = pr.TotalQuantity,
                            Skus = pr.TotalSkus,
                            Status = pr.Status,
                            Rampa = rampaAtual,
                            GroupingKey = groupingField == "CAR_MOVE_ID" ? pr.CarMoveId : pr.StopId // Chave de agrupamento dinâmica
                        });
                    }
                }

                return Ok(requests);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar pick requests disponíveis");
                return StatusCode(500, "Erro interno ao processar a requisição");
            }
        }
        [HttpPost("alocar-rampa")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> AlocarRampa([FromBody] AlocacaoRampaDto alocacao, [FromQuery] string groupingField = "STOP_NAM")
        {
            try
            {
                _logger.LogInformation($"Alocando rampa {alocacao.Rampa} para embarque {alocacao.Embarque}");

                // Verificar se o pick request existe
                var pickRequest = await _context.PickRequests
                    .FirstOrDefaultAsync(pr => pr.OrderNumber == alocacao.Embarque);

                if (pickRequest == null)
                {
                    return NotFound($"Pick request com número de embarque {alocacao.Embarque} não encontrado");
                }

                // Verificar se a rampa existe
                var rampa = await _context.Ramps.FindAsync(alocacao.Rampa);
                if (rampa == null)
                {
                    return NotFound($"Rampa {alocacao.Rampa} não encontrada");
                }

                // Verificar se a rampa já está em uso
                if (rampa.CurrentPickRequestId != null)
                {
                    return BadRequest($"Rampa {alocacao.Rampa} já está em uso pelo pick request {rampa.CurrentPickRequestId}");
                }

                // Obter a chave de agrupamento do pick request atual
                string groupingKey = groupingField == "CAR_MOVE_ID" ? pickRequest.CarMoveId : pickRequest.StopId;

                // Buscar todos os pick requests com a mesma chave de agrupamento
                var relatedPickRequests = await _context.PickRequests
                    .Where(pr =>
                        pr.Status == "Pendente" &&
                        ((groupingField == "CAR_MOVE_ID" && pr.CarMoveId == groupingKey) ||
                        (groupingField == "STOP_NAM" && pr.StopId == groupingKey)))
                    .ToListAsync();

                // Para cada pick request relacionado, verificar e liberar alocações existentes
                foreach (var relatedPR in relatedPickRequests)
                {
                    var alocacaoExistente = await _context.RampAllocations
                        .Where(ra => ra.PickRequestId == relatedPR.Id && ra.ReleasedAt == null)
                        .FirstOrDefaultAsync();

                    if (alocacaoExistente != null)
                    {
                        // Liberar a alocação existente
                        alocacaoExistente.ReleasedAt = DateTime.Now;
                        alocacaoExistente.ReleasedByUserId = 1; // Ajustar para usuário atual

                        // Atualizar a rampa anterior
                        var rampaAnterior = await _context.Ramps.FindAsync(alocacaoExistente.RampId);
                        if (rampaAnterior != null)
                        {
                            rampaAnterior.CurrentPickRequestId = null;
                            rampaAnterior.Status = "Disponível";
                        }
                    }

                    // Criar nova alocação para cada pick request relacionado
                    var novaAlocacao = new RampAllocation
                    {
                        RampId = alocacao.Rampa,
                        PickRequestId = relatedPR.Id,
                        AllocatedAt = DateTime.Now,
                        AllocatedByUserId = 1004 // Ajustar para usuário atual
                    };

                    await _context.RampAllocations.AddAsync(novaAlocacao);

                    // Para o primeiro pick request, atualizar também a rampa
                    if (relatedPR.Id == pickRequest.Id)
                    {
                        rampa.CurrentPickRequestId = pickRequest.Id;
                        rampa.Status = "Em Uso";
                    }
                }

                await _context.SaveChangesAsync();

                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Erro ao alocar rampa {alocacao.Rampa} para embarque {alocacao.Embarque}");
                return StatusCode(500, "Erro interno ao processar a requisição");
            }
        }

        [HttpPost("gerar")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult> GerarWave([FromBody] List<string> embarques)
        {
            try
            {
                _logger.LogInformation($"Gerando wave para {embarques.Count} embarques");

                if (embarques == null || !embarques.Any())
                {
                    return BadRequest("É necessário informar pelo menos um embarque");
                }

                // Verificar se todos os embarques existem
                var pickRequests = await _context.PickRequests
                    .Where(pr => embarques.Contains(pr.OrderNumber))
                    .ToListAsync();

                if (pickRequests.Count != embarques.Count)
                {
                    return BadRequest("Um ou mais embarques não foram encontrados");
                }

                // Verificar se todos os embarques têm rampa alocada
                var pickRequestIds = pickRequests.Select(pr => pr.Id).ToList();
                var alocacoes = await _context.RampAllocations
                    .Where(ra => pickRequestIds.Contains(ra.PickRequestId) && ra.ReleasedAt == null)
                    .ToListAsync();

                // Criar mapeamento de pickRequestId para rampId
                Dictionary<string, int> rampMappings = alocacoes
                    .ToDictionary(ra => ra.PickRequestId, ra => ra.RampId);

                if (rampMappings.Count != pickRequestIds.Count)
                {
                    return BadRequest("Um ou mais embarques não têm rampa alocada");
                }

                // Gerar número de wave
                string waveNumber = $"W{DateTime.Now:yyyyMMddHHmmss}";

                // Criar a wave
                var wave = new Wave
                {
                    WaveNumber = waveNumber,
                    Status = "Em Execução",
                    CreatedAt = DateTime.Now,
                    CreatedByUserId = 1004 // Ajustar para usuário atual
                };

                await _context.Waves.AddAsync(wave);
                await _context.SaveChangesAsync(); // Salvar para obter o ID

                // Associar pick requests à wave
                foreach (var pr in pickRequests)
                {
                    // Atualizar status do pick request
                    pr.Status = "Em Execução";
                    pr.StartedAt = DateTime.Now;

                    // Criar associação com a wave
                    var wavePickRequest = new WavePickRequest
                    {
                        WaveId = wave.Id,
                        PickRequestId = pr.Id,
                        AddedAt = DateTime.Now,
                        AddedByUserId = 1004 // Ajustar para usuário atual
                    };

                    await _context.WavePickRequests.AddAsync(wavePickRequest);
                }

                await _context.SaveChangesAsync();

                // Obter todos os itens de todos os pick requests para sincronizar com o PLC
                var pickRequestItems = await _context.PickRequestItems
                    .Where(pri => pickRequestIds.Contains(pri.PickRequestId))
                    .ToListAsync();

                // Sincronizar com o PLC
                bool plcSyncSuccess = _clpService.SyncWaveWithPlc(pickRequestItems, rampMappings);
                if (!plcSyncSuccess)
                {
                    _logger.LogWarning($"Não foi possível sincronizar a wave {waveNumber} com o PLC, mas a wave foi criada com sucesso");
                }
                else
                {
                    _logger.LogInformation($"Wave {waveNumber} sincronizada com o PLC com sucesso");
                }

                return Ok(new { WaveNumber = waveNumber, PlcSyncSuccess = plcSyncSuccess });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao gerar wave");
                return StatusCode(500, "Erro interno ao processar a requisição");
            }
        }

        [HttpPost("desassociar")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult> DesassociarRampas([FromBody] List<string> embarques)
        {
            try
            {
                _logger.LogInformation($"Desassociando rampas de {embarques.Count} embarques");

                if (embarques == null || !embarques.Any())
                {
                    return BadRequest("É necessário informar pelo menos um embarque");
                }

                // Buscar pick requests
                var pickRequests = await _context.PickRequests
                    .Where(pr => embarques.Contains(pr.OrderNumber))
                    .ToListAsync();

                if (!pickRequests.Any())
                {
                    return BadRequest("Nenhum embarque encontrado");
                }

                var pickRequestIds = pickRequests.Select(pr => pr.Id).ToList();

                // Buscar alocações ativas
                var alocacoes = await _context.RampAllocations
                    .Where(ra => pickRequestIds.Contains(ra.PickRequestId) && ra.ReleasedAt == null)
                    .ToListAsync();

                if (!alocacoes.Any())
                {
                    return BadRequest("Nenhuma alocação encontrada para os embarques informados");
                }

                // Liberar alocações
                foreach (var alocacao in alocacoes)
                {
                    alocacao.ReleasedAt = DateTime.Now;
                    alocacao.ReleasedByUserId = 1004; // Ajustar para usuário atual

                    // Liberar rampa
                    var rampa = await _context.Ramps.FindAsync(alocacao.RampId);
                    if (rampa != null)
                    {
                        rampa.CurrentPickRequestId = null;
                        rampa.Status = "Disponível";
                    }
                }

                await _context.SaveChangesAsync();

                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao desassociar rampas");
                return StatusCode(500, "Erro interno ao processar a requisição");
            }
        }

        [HttpGet("active")]
        [ProducesResponseType(typeof(WaveInfoDto), StatusCodes.Status200OK)]
        public async Task<ActionResult<WaveInfoDto>> GetActiveWave()
        {
            try
            {
                // Buscar a wave ativa
                var wave = await _context.Waves
                    .Where(w => w.Status == "Em Execução")
                    .OrderByDescending(w => w.CreatedAt)
                    .FirstOrDefaultAsync();

                if (wave == null)
                {
                    return Ok(new WaveInfoDto
                    {
                        WaveNumber = "",
                        Status = "Sem wave ativa",
                        CreatedAt = "",
                        PickRequests = new List<WavePickRequestDto>()
                    });
                }

                // Buscar pick requests da wave
                var wavePickRequests = await _context.WavePickRequests
                    .Where(wpr => wpr.WaveId == wave.Id)
                    .Include(wpr => wpr.PickRequest)
                    .ToListAsync();

                var pickRequests = new List<WavePickRequestDto>();

                foreach (var wpr in wavePickRequests)
                {
                    // Buscar rampa alocada
                    var alocacao = await _context.RampAllocations
                        .Where(ra => ra.PickRequestId == wpr.PickRequestId && ra.ReleasedAt == null)
                        .Select(ra => ra.RampId)
                        .FirstOrDefaultAsync();

                    pickRequests.Add(new WavePickRequestDto
                    {
                        Embarque = wpr.PickRequest.OrderNumber,
                        StopId = wpr.PickRequest.StopId,
                        Delivery = wpr.PickRequest.DeliveryType,
                        Cliente = wpr.PickRequest.ClientName,
                        Particularidade = wpr.PickRequest.Customization,
                        Quantidade = wpr.PickRequest.TotalQuantity,
                        Skus = wpr.PickRequest.TotalSkus,
                        Status = wpr.PickRequest.Status,
                        Rampa = alocacao != 0 ? (int?)alocacao : null
                    });
                }

                var waveInfo = new WaveInfoDto
                {
                    WaveNumber = wave.WaveNumber,
                    Status = wave.Status,
                    CreatedAt = wave.CreatedAt.ToString("o"),
                    PickRequests = pickRequests
                };

                return Ok(waveInfo);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar wave ativa");
                return StatusCode(500, "Erro interno ao processar a requisição");
            }
        }
    }
    
}