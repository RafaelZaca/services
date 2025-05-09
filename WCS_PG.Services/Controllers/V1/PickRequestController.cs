using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WCS_PG.Data;
using WCS_PG.Services.Models.request.V1;

namespace WCS_PG.Services.Controllers.V1
{
    [ApiController]
    [Route("api/[controller]")]
    public class PickRequestController : ControllerBase
    {
        private readonly ILogger<PickRequestController> _logger;
        private readonly WCSContext _context;

        public PickRequestController(ILogger<PickRequestController> logger, WCSContext context)
        {
            _logger = logger;
            _context = context;
        }

        [HttpGet]
        [ProducesResponseType(typeof(List<PickRequestDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<List<PickRequestDto>>> GetPickRequests()
        {
            try
            {
                // Buscar os pick requests do banco
                var pickRequestsDb = await _context.PickRequests.Where(_ => _.Status == "Pendente")
                    .Include(pr => pr.RampAllocations.Where(ra => ra.ReleasedAt == null))
                    .ToListAsync();

                var pickRequests = new List<PickRequestDto>();

                // Se temos dados reais, usamos eles
                if (pickRequestsDb.Any())
                {
                    foreach (var pr in pickRequestsDb)
                    {
                        // Determinar a rampa atual, se houver
                        var rampaAtual = pr.RampAllocations
                            .Where(ra => ra.ReleasedAt == null)
                            .Select(ra => ra.RampId)
                            .FirstOrDefault();

                        pickRequests.Add(new PickRequestDto
                        {
                            Id = pr.Id,
                            NumeroEmbarque = pr.OrderNumber,
                            StopId = pr.StopId,
                            Delivery = pr.DeliveryType,
                            Cliente = pr.ClientName,
                            Particularidade = pr.Customization,
                            Quantidade = pr.TotalQuantity,
                            Skus = pr.TotalSkus,
                            Status = pr.Status,
                            Rampa = rampaAtual == 0 ? null : (int?)rampaAtual,
                            DataInicio = pr.StartedAt?.ToString("o")
                        });
                    }
                }
                else
                {
                    //// Dados mock para quando não há dados reais
                    //pickRequests = new List<PickRequestDto>
                    //{
                    //    new()
                    //    {
                    //        Id = "1",
                    //        NumeroEmbarque = "13345677",
                    //        StopId = "1",
                    //        Delivery = "Multiplos",
                    //        Cliente = "Atacadão",
                    //        Particularidade = "Lotação paletizado",
                    //        Quantidade = 930,
                    //        Skus = 50,
                    //        Status = "Em Execução",
                    //        Rampa = 1,
                    //        DataInicio = "2025-02-14T08:00:00"
                    //    },
                    //    new()
                    //    {
                    //        Id = "2",
                    //        NumeroEmbarque = "12345678",
                    //        StopId = "1",
                    //        Delivery = "Multiplos",
                    //        Cliente = "Sendas",
                    //        Particularidade = "Lotação paletizado",
                    //        Quantidade = 1233,
                    //        Skus = 189,
                    //        Status = "Pendente"
                    //    }
                    //};
                }

                return Ok(pickRequests);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar pick requests");
                return StatusCode(500, "Erro interno ao processar a requisição");
            }
        }

        [HttpPost("{id}/concluir")]
        [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<bool>> ConcluirPickRequest(string id)
        {
            try
            {
                _logger.LogInformation($"Concluindo pick request {id}");

                // Buscar o pick request
                var pickRequest = await _context.PickRequests
                    .FirstOrDefaultAsync(pr => pr.Id == id);

                if (pickRequest == null)
                {
                    return NotFound($"Pick request com ID {id} não encontrado");
                }

                // Atualizar status
                pickRequest.Status = "Concluído";
                pickRequest.CompletedAt = DateTime.Now;

                // Liberar rampas
                var rampasAlocadas = await _context.RampAllocations
                    .Where(ra => ra.PickRequestId == id && ra.ReleasedAt == null)
                    .ToListAsync();

                foreach (var rampAlloc in rampasAlocadas)
                {
                    rampAlloc.ReleasedAt = DateTime.Now;
                    rampAlloc.ReleasedByUserId = 1; // Usuário do sistema, ajustar com usuário atual
                }

                // Limpar referências nas rampas
                var rampasComReferencia = await _context.Ramps
                    .Where(r => r.CurrentPickRequestId == id)
                    .ToListAsync();

                foreach (var rampa in rampasComReferencia)
                {
                    rampa.CurrentPickRequestId = null;
                    rampa.Status = "Disponível";
                }

                await _context.SaveChangesAsync();

                return Ok(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Erro ao concluir pick request {id}");
                return StatusCode(500, "Erro interno ao processar a requisição");
            }
        }

        [HttpPost("{id}/cancelar")]
        [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<bool>> CancelarPickRequest(string id)
        {
            try
            {
                _logger.LogInformation($"Cancelando pick request {id}");

                // Buscar o pick request
                var pickRequest = await _context.PickRequests
                    .FirstOrDefaultAsync(pr => pr.Id == id);

                if (pickRequest == null)
                {
                    return NotFound($"Pick request com ID {id} não encontrado");
                }

                // Atualizar status
                pickRequest.Status = "Cancelado";

                // Liberar rampas (similar ao concluir)
                var rampasAlocadas = await _context.RampAllocations
                    .Where(ra => ra.PickRequestId == id && ra.ReleasedAt == null)
                    .ToListAsync();

                foreach (var rampAlloc in rampasAlocadas)
                {
                    rampAlloc.ReleasedAt = DateTime.Now;
                    rampAlloc.ReleasedByUserId = 1; // Usuário do sistema, ajustar com usuário atual
                }

                // Limpar referências nas rampas
                var rampasComReferencia = await _context.Ramps
                    .Where(r => r.CurrentPickRequestId == id)
                    .ToListAsync();

                foreach (var rampa in rampasComReferencia)
                {
                    rampa.CurrentPickRequestId = null;
                    rampa.Status = "Disponível";
                }

                await _context.SaveChangesAsync();

                return Ok(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Erro ao cancelar pick request {id}");
                return StatusCode(500, "Erro interno ao processar a requisição");
            }
        }
    }
}