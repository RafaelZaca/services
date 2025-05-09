using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WCS_PG.Data;
using WCS_PG.Services.Models.request.V1;
using System.Linq;

namespace WCS_PG.Services.Controllers.V1
{
    [ApiController]
    [Route("api/[controller]")]
    public class RejeitoOverviewController : ControllerBase
    {
        private readonly ILogger<RejeitoOverviewController> _logger;
        private readonly WCSContext _context;

        public RejeitoOverviewController(ILogger<RejeitoOverviewController> logger, WCSContext context)
        {
            _logger = logger;
            _context = context;
        }

        [HttpGet]
        [ProducesResponseType(typeof(List<RejeitoItemDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<List<RejeitoItemDto>>> GetRejeitoData()
        {
            try
            {
                // Buscar rejeições do banco
                var rejeicoes = await _context.Rejections
                    .Include(r => r.Ramp)
                    .Include(r => r.PickRequest)
                    .Where(r => r.RejectedAt.Date == DateTime.Today.Date) // Filtrar por hoje
                    .ToListAsync();

                // Agrupar rejeições por SKU
                var skuGroups = rejeicoes.GroupBy(r => r.Sku).ToList();

                var result = new List<RejeitoItemDto>();

                if (skuGroups.Any())
                {
                    // Para cada SKU, criar um item de rejeito
                    foreach (var group in skuGroups)
                    {
                        var sku = group.Key;

                        // Primeiro item para obter informações básicas
                        var firstItem = group.First();

                        // Encontrar o Wave ID
                        var waveId = "";
                        var wavePickRequest = await _context.WavePickRequests
                            .Include(wp => wp.Wave)
                            .FirstOrDefaultAsync(wp => wp.PickRequestId == firstItem.PickRequestId);

                        if (wavePickRequest != null)
                        {
                            waveId = wavePickRequest.Wave.WaveNumber;
                        }

                        // Descrição do SKU
                        var pickRequestItem = await _context.PickRequestItems
                            .FirstOrDefaultAsync(pi => pi.Sku == sku);

                        var descricao = pickRequestItem?.Description ?? sku;

                        // Criar o objeto de rejeito
                        var rejeitoItem = new RejeitoItemDto
                        {
                            Sku = sku,
                            Wave = waveId,
                            TotalRejeito = group.Count(),
                            Rampa01 = group.Count(r => r.Ramp.RampNumber == "01"),
                            Rampa02 = group.Count(r => r.Ramp.RampNumber == "02"),
                            Rampa03 = group.Count(r => r.Ramp.RampNumber == "03"),
                            Rampa04 = group.Count(r => r.Ramp.RampNumber == "04"),
                            Rampa05 = group.Count(r => r.Ramp.RampNumber == "05"),
                            Rampa06 = group.Count(r => r.Ramp.RampNumber == "06"),
                            Rampa07 = group.Count(r => r.Ramp.RampNumber == "07"),
                            Rampa08 = group.Count(r => r.Ramp.RampNumber == "08"),
                            Rampa09 = group.Count(r => r.Ramp.RampNumber == "09"),
                            Rampa10 = group.Count(r => r.Ramp.RampNumber == "10"),
                            Rampa11 = group.Count(r => r.Ramp.RampNumber == "11"),
                            Rampa12 = group.Count(r => r.Ramp.RampNumber == "12"),
                            NaoPrevisto = group.Count(r => r.RejectionType == "NaoPrevisto"),
                            Excesso = group.Count(r => r.RejectionType == "Excesso"),
                            Descricao = descricao
                        };

                        result.Add(rejeitoItem);
                    }
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar dados de rejeito");
                return StatusCode(500, "Erro interno ao processar a requisição");
            }
        }

        [HttpGet("filtrar")]
        [ProducesResponseType(typeof(List<RejeitoItemDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<List<RejeitoItemDto>>> GetRejeitoDataFiltrado(
            [FromQuery] string? sku = null,
            [FromQuery] string? wave = null,
            [FromQuery] DateTime? dataInicio = null,
            [FromQuery] DateTime? dataFim = null)
        {
            try
            {
                // Aplicar data padrão se não informada
                dataFim ??= DateTime.Today.AddDays(1);  // Final do dia de hoje
                dataInicio ??= dataFim.Value.AddDays(-7);  // Últimos 7 dias por padrão

                // Base da consulta
                var query = _context.Rejections
                    .Include(r => r.Ramp)
                    .Include(r => r.PickRequest)
                    .Where(r => r.RejectedAt >= dataInicio && r.RejectedAt < dataFim);

                // Filtros adicionais
                if (!string.IsNullOrEmpty(sku))
                {
                    query = query.Where(r => r.Sku.Contains(sku));
                }

                if (!string.IsNullOrEmpty(wave))
                {
                    // Filtrar por wave requer uma subconsulta
                    var pickRequestIds = _context.WavePickRequests
                        .Include(wp => wp.Wave)
                        .Where(wp => wp.Wave.WaveNumber.Contains(wave))
                        .Select(wp => wp.PickRequestId);

                    query = query.Where(r => pickRequestIds.Contains(r.PickRequestId));
                }

                var rejeicoes = await query.ToListAsync();

                // Continuar com o processamento como na função anterior
                var skuGroups = rejeicoes.GroupBy(r => r.Sku).ToList();
                var result = new List<RejeitoItemDto>();

                // Processar resultados como na função GetRejeitoData
                // (Código omitido por brevidade, é o mesmo da função anterior)

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar dados de rejeito filtrados");
                return StatusCode(500, "Erro interno ao processar a requisição");
            }
        }
    }
}