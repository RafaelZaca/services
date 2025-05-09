using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WCS_PG.Data;
using WCS_PG.Data.Models;
using WCS_PG.Services.Models.request.V1;

namespace WCS_PG.Services.Controllers.V1
{
    [ApiController]
    [Route("api/[controller]")]
    public class SeparacaoController : ControllerBase
    {
        private readonly ILogger<SeparacaoController> _logger;
        private readonly WCSContext _context;

        public SeparacaoController(ILogger<SeparacaoController> logger, WCSContext context)
        {
            _logger = logger;
            _context = context;
        }

        [HttpGet]
        [ProducesResponseType(typeof(SeparacaoInfoDto), StatusCodes.Status200OK)]
        public async Task<ActionResult<SeparacaoInfoDto>> GetSeparacaoData([FromQuery] string? waveId = null)
        {
            try
            {
                // Consulta base para obter a wave atual ou a especificada
                Wave? wave = null;
                List<string> pickRequestIds = new List<string>();

                if (!string.IsNullOrEmpty(waveId))
                {
                    // Buscar wave específica
                    wave = await _context.Waves
                        .FirstOrDefaultAsync(w => w.WaveNumber == waveId);
                }
                else
                {
                    // Buscar wave ativa (em execução)
                    wave = await _context.Waves
                        .FirstOrDefaultAsync(w => w.Status == "Em Execução");
                }

                // Se não encontrou wave, busca a mais recente
                if (wave == null)
                {
                    wave = await _context.Waves
                        .OrderByDescending(w => w.CreatedAt)
                        .FirstOrDefaultAsync();
                }

                // Buscar pick requests associados à wave através da tabela de junção
                if (wave != null)
                {
                    var wavePickRequests = await _context.WavePickRequests
                        .Where(wp => wp.WaveId == wave.Id)
                        .ToListAsync();

                    pickRequestIds = wavePickRequests.Select(wp => wp.PickRequestId).ToList();
                }

                // Se ainda assim não encontrou, usa mock data
                if (wave == null || !pickRequestIds.Any())
                {
                    return Ok();
                    return Ok(GetMockSeparacaoData());
                }

                // Buscar todos os pick request items associados
                var items = await _context.PickRequestItems
                    .Where(pri => pickRequestIds.Contains(pri.PickRequestId))
                    .ToListAsync();

                // Buscar rejeições
                var rejeicoes = await _context.Rejections
                    .Where(r => pickRequestIds.Contains(r.PickRequestId))
                    .ToListAsync();

                // Agrupar items por SKU
                var skuGroups = items.GroupBy(i => i.Sku).ToList();

                var separacaoItems = new List<SeparacaoItemDto>();

                foreach (var group in skuGroups)
                {
                    var sku = group.Key;

                    // Calcular totais
                    int qtdEsperada = (int)(group.Sum(i => i.ExpectedQuantity) == null ? 0 : group.Sum(i => i.ExpectedQuantity)!);
                    int qtdInduzida = (int)(group.Sum(i => i.InducedQuantity) == null ? 0 : group.Sum(i => i.InducedQuantity)!);
                    int qtdRecebida = (int)(group.Sum(i => i.ReceivedQuantity) == null ? 0 : group.Sum(i => i.ReceivedQuantity)!);
                    int percentualInd = qtdEsperada > 0 ? (int)(qtdInduzida * 100 / qtdEsperada) : 0;
                    int percentualRec = qtdEsperada > 0 ? (int)(qtdRecebida * 100 / qtdEsperada) : 0;

                    // Calcular rejeições
                    int rejeitosNoRead = rejeicoes.Count(r => r.Sku == sku && r.RejectionType == "NoRead");
                    int rejeitosFinal = rejeicoes.Count(r => r.Sku == sku && r.RejectionType != "NoRead");

                    // Calcular pendentes
                    int pendentes = qtdEsperada - qtdRecebida;

                    // Calcular tempo desde última caixa
                    int tempoUltimaCaixa = 0;
                    var ultimoItem = group.OrderByDescending(i => i.LastBoxReceivedAt).FirstOrDefault();
                    if (ultimoItem?.LastBoxReceivedAt != null)
                    {
                        tempoUltimaCaixa = (int)Math.Floor((DateTime.Now - ultimoItem.LastBoxReceivedAt.Value).TotalMinutes);
                    }

                    // Obter descrição
                    string descricao = ultimoItem?.Description ?? sku;

                    separacaoItems.Add(new SeparacaoItemDto
                    {
                        Sku = sku,
                        QuantidadeEsperada = qtdEsperada,
                        QuantidadeInduzida = qtdInduzida,
                        PercentualInd = percentualInd,
                        QuantidadeRecebida = qtdRecebida,
                        PercentualRec = percentualRec,
                        TempoUltimaCaixa = tempoUltimaCaixa,
                        Descricao = descricao,
                        QuantidadeRejeitoFinal = rejeitosFinal,
                        QuantidadeRejeitoNoRead = rejeitosNoRead,
                        Pendente = pendentes > 0 ? pendentes : 0
                    });
                }

                // Calcular totais gerais
                int totalRejeitoNoRead = rejeicoes.Count(r => r.RejectionType == "NoRead");
                int totalPendente = separacaoItems.Sum(item => item.Pendente);

                var resposta = new SeparacaoInfoDto
                {
                    RejeitoNoRead = totalRejeitoNoRead,
                    Pendente = totalPendente,
                    Itens = separacaoItems
                };

                return Ok(resposta);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar dados de separação");
                return StatusCode(500, "Erro interno ao processar a requisição");
            }
        }

        [HttpGet("wave/{waveId}")]
        [ProducesResponseType(typeof(SeparacaoInfoDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<SeparacaoInfoDto>> GetSeparacaoDataByWave(string waveId)
        {
            try
            {
                // Buscar dados da wave específica
                return await GetSeparacaoData(waveId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Erro ao buscar dados de separação para wave {waveId}");
                return StatusCode(500, "Erro interno ao processar a requisição");
            }
        }

        // Método auxiliar para retornar dados mock
        private SeparacaoInfoDto GetMockSeparacaoData()
        {
            return new SeparacaoInfoDto
            {
                RejeitoNoRead = 37,
                Pendente = 1,
                Itens = new List<SeparacaoItemDto>
                {
                    new SeparacaoItemDto
                    {
                        Sku = "xxxxxxx",
                        QuantidadeEsperada = 100,
                        QuantidadeInduzida = 100,
                        PercentualInd = 100,
                        QuantidadeRecebida = 99,
                        PercentualRec = 99,
                        TempoUltimaCaixa = 12,
                        Descricao = "Shampoo Pantene Bambu",
                        QuantidadeRejeitoFinal = 0,
                        QuantidadeRejeitoNoRead = 1,
                        Pendente = 1
                    },
                    new SeparacaoItemDto
                    {
                        Sku = "yyyyyyy",
                        QuantidadeEsperada = 200,
                        QuantidadeInduzida = 180,
                        PercentualInd = 90,
                        QuantidadeRecebida = 180,
                        PercentualRec = 90,
                        TempoUltimaCaixa = 12,
                        Descricao = "Condicionador Pantene",
                        QuantidadeRejeitoFinal = 0,
                        QuantidadeRejeitoNoRead = 20,
                        Pendente = 0
                    },
                    new SeparacaoItemDto
                    {
                        Sku = "xxxxxxx",
                        QuantidadeEsperada = 60,
                        QuantidadeInduzida = 30,
                        PercentualInd = 50,
                        QuantidadeRecebida = 30,
                        PercentualRec = 50,
                        TempoUltimaCaixa = 0,
                        Descricao = "Shampoo Pantene Bambu",
                        QuantidadeRejeitoFinal = 0,
                        QuantidadeRejeitoNoRead = 30,
                        Pendente = 0
                    },
                    new SeparacaoItemDto
                    {
                        Sku = "yyyyyyy",
                        QuantidadeEsperada = 100,
                        QuantidadeInduzida = 50,
                        PercentualInd = 50,
                        QuantidadeRecebida = 50,
                        PercentualRec = 50,
                        TempoUltimaCaixa = 0,
                        Descricao = "Condicionador Pantene",
                        QuantidadeRejeitoFinal = 0,
                        QuantidadeRejeitoNoRead = 50,
                        Pendente = 0
                    }
                }
            };
        }
    }
}