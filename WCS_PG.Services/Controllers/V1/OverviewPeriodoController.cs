using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WCS_PG.Data;
using WCS_PG.Services.Models.request.V1;

namespace WCS_PG.Services.Controllers.V1
{
    [ApiController]
    [Route("api/[controller]")]
    public class OverviewController : ControllerBase
    {
        private readonly ILogger<OverviewController> _logger;
        private readonly WCSContext _context;

        public OverviewController(ILogger<OverviewController> logger, WCSContext context)
        {
            _logger = logger;
            _context = context;
        }

        [HttpGet("production")]
        [ProducesResponseType(typeof(List<ProductionDataDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<List<ProductionDataDto>>> GetProductionData()
        {
            try
            {
                // Buscar dados do dia atual
                var hoje = DateTime.Today;
                var producaoHoraria = await _context.ProductionHourlies
                    .Where(p => p.Hour > hoje.AddDays(-1))
                    .OrderBy(p => p.Hour)
                    .ToListAsync();

                // Preparar os dados para o formato esperado
                var planejado = new ProductionDataDto
                {
                    Name = "Planejado",
                    Series = new List<SeriesItemDto>()
                };

                var produzido = new ProductionDataDto
                {
                    Name = "Produzido",
                    Series = new List<SeriesItemDto>()
                };

                var rejeito = new ProductionDataDto
                {
                    Name = "Rejeito",
                    Series = new List<SeriesItemDto>()
                };

                var capacidade = new ProductionDataDto
                {
                    Name = "Capacidade",
                    Series = new List<SeriesItemDto>()
                };

                // Se temos dados reais, usamos eles
                if (producaoHoraria.Any())
                {
                    foreach (var item in producaoHoraria)
                    {
                        var horaFormatada = item.Hour.ToString("HH:00");

                        planejado.Series.Add(new SeriesItemDto { Name = horaFormatada, Value = item.PlannedQuantity });
                        produzido.Series.Add(new SeriesItemDto { Name = horaFormatada, Value = item.ProducedQuantity });
                        rejeito.Series.Add(new SeriesItemDto { Name = horaFormatada, Value = item.RejectedQuantity });
                        capacidade.Series.Add(new SeriesItemDto { Name = horaFormatada, Value = item.Capacity });
                    }
                }
                else
                {
                    //// Dados mock para quando não há dados reais
                    //var horasDodia = Enumerable.Range(0, 24).Select(h => $"{h:D2}:00").ToList();

                    //// Valores simulados
                    //var valoresPlanejados = new[] { 1583, 1480, 2179, 3006, 0, 1006, 2868, 3393, 2480, 1380, 1684, 0, 1024, 2668, 2714, 3074, 3322, 1203, 3090, 0, 2389, 2037, 1700, 1630 };
                    //var valoresProduzidos = new[] { 1500, 1400, 2000, 2800, 0, 950, 2700, 3200, 2300, 1300, 1600, 0, 1000, 2500, 2600, 2900, 3100, 1100, 2900, 0, 2200, 1900, 1600, 1500 };
                    //var valoresRejeitados = new[] { 1, 39, 48, 56, 6, 22, 77, 35, 64, 0, 63, 34, 31, 13, 56, 75, 76, 60, 58, 4, 43, 18, 17, 38 };
                    //var valoresCapacidade = Enumerable.Repeat(3500, 24).ToArray();

                    //for (int i = 0; i < horasDodia.Count; i++)
                    //{
                    //    planejado.Series.Add(new SeriesItemDto { Name = horasDodia[i], Value = valoresPlanejados[i] });
                    //    produzido.Series.Add(new SeriesItemDto { Name = horasDodia[i], Value = valoresProduzidos[i] });
                    //    rejeito.Series.Add(new SeriesItemDto { Name = horasDodia[i], Value = valoresRejeitados[i] });
                    //    capacidade.Series.Add(new SeriesItemDto { Name = horasDodia[i], Value = valoresCapacidade[i] });
                    //}
                }

                var data = new List<ProductionDataDto>
                {
                    planejado,
                    produzido,
                    rejeito,
                    capacidade
                };

                return Ok(data);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar dados de produção");
                return StatusCode(500, "Erro interno ao processar a requisição");
            }
        }

        [HttpGet("summary")]
        [ProducesResponseType(typeof(SummaryDataDto), StatusCodes.Status200OK)]
        public async Task<ActionResult<SummaryDataDto>> GetSummaryData()
        {
            try
            {
                // Buscar dados do dia atual
                var hoje = DateTime.Today;
                var metricas = await _context.OperationalMetrics
                    .Where(p => p.Date > hoje.AddDays(-1))
                    .ToListAsync();

                SummaryDataDto summary = new SummaryDataDto
                {
                    ProcessedBoxes = metricas.Sum(m => m.ProcessedBoxes),
                    RejectedBoxes = metricas.Sum(m => m.RejectedBoxes),
                    OperatingHours = metricas.Sum(m => m.OperatingHours),
                    StoppedHours = metricas.Sum(m => m.StoppedHours),
                    Waves = metricas.Sum(m => m.CompletedWaves),
                    Shipments = metricas.Sum(m => m.CompletedShipments)
                };


                return Ok(summary);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar dados de resumo");
                return StatusCode(500, "Erro interno ao processar a requisição");
            }
        }
    }
}