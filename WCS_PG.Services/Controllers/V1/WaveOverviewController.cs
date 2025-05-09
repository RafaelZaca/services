using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WCS_PG.Data;
using WCS_PG.Data.Models;
using WCS_PG.Services.Models.request.V1;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace WCS_PG.Services.Controllers.V1
{
    [ApiController]
    [Route("api/[controller]")]
    public class WaveOverviewController : ControllerBase
    {
        private readonly ILogger<WaveOverviewController> _logger;
        private readonly WCSContext _context;

        public WaveOverviewController(ILogger<WaveOverviewController> logger, WCSContext context)
        {
            _logger = logger;
            _context = context;
        }

        [HttpGet]
        [ProducesResponseType(typeof(WaveDashboardDataDto), StatusCodes.Status200OK)]
        public async Task<ActionResult<WaveDashboardDataDto>> GetDashboardData()
        {
            try
            {
                // Buscar a wave ativa
                var activeWave = await _context.Waves
                    .Where(w => w.Status == "Em Execução")
                    .OrderByDescending(w => w.CreatedAt)
                    .FirstOrDefaultAsync();

                if (activeWave == null)
                {
                    // Retorna um dashboard vazio ou com valores zerados se não houver wave ativa
                    return Ok(new WaveDashboardDataDto
                    {
                        AllocatedBoxes = new BoxAllocationDto { Total = 0, Separated = 0, Pending = 0 },
                        ExpectedSkus = new SkuAllocationDto { Total = 0, Separated = 0, Pending = 0 },
                        AllocatedShipments = 0,
                        ActiveOperators = 0,
                        ShipmentTypes = new List<TypeValueDto>(),
                        OperatorActivities = new List<TypeValueDto>(),
                        ReadRejection = new ReadRejectionDto { TotalRejected = 0, Treated = 0, Pending = 0 },
                        ExcessRejection = new ExcessRejectionDto { Total = 0, NotExpected = 0, Excess = 0, FullRamp = 0 },
                        Productivity = 0
                    });
                }

                // Buscar pick requests associados à wave ativa
                var wavePickRequests = await _context.WavePickRequests
                    .Where(wpr => wpr.WaveId == activeWave.Id)
                    .Include(wpr => wpr.PickRequest)
                    .ToListAsync();

                var pickRequestIds = wavePickRequests.Select(wpr => wpr.PickRequestId).ToList();

                // Buscar todos os itens de pick requests da wave ativa
                var allItems = await _context.PickRequestItems
                    .Where(pri => pickRequestIds.Contains(pri.PickRequestId))
                    .ToListAsync();

                // Calcular totais de caixas
                int totalBoxes = allItems.Sum(i => i.ExpectedQuantity ?? 0);
                int separatedBoxes = allItems.Sum(i => i.ReceivedQuantity ?? 0);
                int pendingBoxes = totalBoxes - separatedBoxes;

                // Calcular totais de SKUs
                int totalSkus = allItems.Select(i => i.Sku).Distinct().Count();
                int separatedSkus = allItems
                    .Where(i => (i.ReceivedQuantity ?? 0) > 0)
                    .Select(i => i.Sku)
                    .Distinct()
                    .Count();
                int pendingSkus = totalSkus - separatedSkus;

                // Contar embarques alocados
                int allocatedShipments = pickRequestIds.Count;

                // Buscar operadores ativos (usuários logados com atividade recente)
                // Este método pode precisar ser ajustado dependendo de como você rastreia atividades de usuários
                var activeOperatorsCount = 12; // Valor fixo como fallback se não houver rastreamento de atividades

                // Verifica se existe a tabela/entidade de atividades de usuário
                if (_context.Model.FindEntityType(typeof(User)) != null)
                {
                    activeOperatorsCount = await _context.Set<User>()
                        .Where(ua => ua.LastLogin > DateTime.Now.AddHours(-1))
                        .Select(ua => ua.Id)
                        .Distinct()
                        .CountAsync();
                }

                // Contagem de tipos de embarque
                var shipmentTypes = await _context.PickRequests
                    .Where(pr => pickRequestIds.Contains(pr.Id))
                    .GroupBy(pr => pr.Customization)
                    .Select(g => new TypeValueDto
                    {
                        Type = g.Key,
                        Value = g.Count().ToString("D2")
                    })
                    .ToListAsync();


                // Contar atividades de operadores (simplificado, ajustar conforme necessário)
                var operatorActivities = new List<TypeValueDto>
                {
                    new() { Type = "Separação A", Value = "04" },
                    new() { Type = "Separação B", Value = "01" },
                    new() { Type = "Customização Sorter", Value = "06" },
                    new() { Type = "Rejeito", Value = "01" }
                };

                // Buscar rejeições no-read
                var noReadRejections = await _context.Rejections
                    .Where(r => r.RejectionType == "NoRead" &&
                           pickRequestIds.Contains(r.PickRequestId) &&
                           r.RejectedAt > activeWave.CreatedAt)
                    .ToListAsync();

                var totalNoReadRejected = noReadRejections.Count;
                var treatedNoRead = noReadRejections.Count(r => r.IsTreated);
                var pendingNoRead = totalNoReadRejected - treatedNoRead;

                // Buscar rejeições por excesso e outros motivos
                var otherRejections = await _context.Rejections
                    .Where(r => r.RejectionType != "NoRead" &&
                           pickRequestIds.Contains(r.PickRequestId) &&
                           r.RejectedAt > activeWave.CreatedAt)
                    .ToListAsync();

                var totalExcessRejected = otherRejections.Count;

                // Contagens específicas por tipo de rejeição
                var notExpectedRejections = otherRejections.Count(r => r.RejectionType == "NotExpected");
                var excessRejections = otherRejections.Count(r => r.RejectionType == "Excess");
                var fullRampRejections = otherRejections.Count(r => r.RejectionType == "FullRamp");

                // Calcular produtividade (caixas por hora)
                int productivity = 0;

                if (activeWave.StartedAt.HasValue)
                {
                    var elapsedHours = (DateTime.Now - activeWave.StartedAt.Value).TotalHours;
                    if (elapsedHours > 0)
                    {
                        productivity = (int)(separatedBoxes / elapsedHours);
                    }
                }

                // Construir o objeto de resposta
                var dashboardData = new WaveDashboardDataDto
                {
                    AllocatedBoxes = new BoxAllocationDto
                    {
                        Total = totalBoxes,
                        Separated = separatedBoxes,
                        Pending = pendingBoxes
                    },
                    ExpectedSkus = new SkuAllocationDto
                    {
                        Total = totalSkus,
                        Separated = separatedSkus,
                        Pending = pendingSkus
                    },
                    AllocatedShipments = allocatedShipments,
                    ActiveOperators = activeOperatorsCount,
                    ShipmentTypes = shipmentTypes,
                    OperatorActivities = operatorActivities,
                    ReadRejection = new ReadRejectionDto
                    {
                        TotalRejected = totalNoReadRejected,
                        Treated = treatedNoRead,
                        Pending = pendingNoRead
                    },
                    ExcessRejection = new ExcessRejectionDto
                    {
                        Total = totalExcessRejected,
                        NotExpected = notExpectedRejections,
                        Excess = excessRejections,
                        FullRamp = fullRampRejections
                    },
                    Productivity = productivity
                };

                return Ok(dashboardData);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar dados do dashboard de wave");

                // Em ambiente de desenvolvimento, retorna detalhes do erro
                if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development")
                {
                    return StatusCode(500, new { message = "Erro interno ao processar a requisição", details = ex.ToString() });
                }

                return StatusCode(500, "Erro interno ao processar a requisição");
            }
        }
    }
}

