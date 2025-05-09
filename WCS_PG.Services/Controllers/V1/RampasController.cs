using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WCS_PG.Data;
using WCS_PG.Data.Models;
using WCS_PG.Services.Models.request.V1;

namespace WCS_PG.Services.Controllers.V1
{
    [ApiController]
    [Route("api/[controller]")]
    public class RampasController : ControllerBase
    {
        private readonly ILogger<RampasController> _logger;
        private readonly WCSContext _context;

        public RampasController(ILogger<RampasController> logger, WCSContext context)
        {
            _logger = logger;
            _context = context;
        }

        [HttpGet]
        [ProducesResponseType(typeof(List<RampaInfoDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<List<RampaInfoDto>>> GetRampasData()
        {
            try
            {
                // Buscar rampas do banco de dados
                var rampasDb = await _context.Ramps
                    .Include(r => r.CurrentPickRequest)
                    .ThenInclude(pr => pr.Items)
                    .OrderBy(r => r.Id)
                    .ToListAsync();

                var rampasData = new List<RampaInfoDto>();

                if (rampasDb.Any())
                {
                    foreach (var rampa in rampasDb)
                    {
                        // Se não tem pick request associado, continua para a próxima
                        if (rampa.CurrentPickRequest == null)
                            continue;

                        var items = rampa.CurrentPickRequest.Items;
                        int qtdEsperada = (int)(items.Sum(i => i.ExpectedQuantity) == null ? 0 : items.Sum(i => i.ExpectedQuantity)!);
                        int qtdRecebida = (int)(items.Sum(i => i.ReceivedQuantity) == null ? 0 : items.Sum(i => i.ReceivedQuantity)!);
                        int itensPendentes = qtdEsperada - qtdRecebida;
                        decimal percentual = qtdEsperada > 0
                            ? Math.Round((decimal)qtdRecebida / qtdEsperada * 100, 1)
                            : 0;

                        rampasData.Add(new RampaInfoDto
                        {
                            Numero = rampa.Id,
                            NumeroEmbarque = rampa.CurrentPickRequest.OrderNumber,
                            Cliente = rampa.CurrentPickRequest.ClientName,
                            Status = qtdEsperada == qtdRecebida ? "Completo" : "Em separação",
                            QuantidadeSolicitada = qtdEsperada,
                            QuantidadeSeparada = qtdRecebida,
                            ItensPendentes = itensPendentes,
                            PercentualAtendido = percentual
                        });
                    }
                }


                return Ok(rampasData);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar dados das rampas");
                return StatusCode(500, "Erro interno ao processar a requisição");
            }
        }

        [HttpPost("{rampaId}/concluir")]
        [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<bool>> ConcluirEmbarque(int rampaId)
        {
            try
            {
                _logger.LogInformation($"Concluindo embarque da rampa {rampaId}");

                var rampa = await _context.Ramps
                    .Include(r => r.CurrentPickRequest)
                    .FirstOrDefaultAsync(r => r.Id == rampaId);

                if (rampa == null)
                {
                    return NotFound($"Rampa com ID {rampaId} não encontrada");
                }

                if (rampa.CurrentPickRequest == null)
                {
                    return NotFound($"Não há pick request associado à rampa {rampaId}");
                }

                // Atualizar status do pick request
                rampa.CurrentPickRequest.Status = "Concluído";
                rampa.CurrentPickRequest.CompletedAt = DateTime.Now;

                // Registrar liberação da rampa
                var rampAllocation = new RampAllocation
                {
                    RampId = rampa.Id,
                    PickRequestId = rampa.CurrentPickRequest.Id,
                    ReleasedAt = DateTime.Now,
                    ReleasedByUserId = 1 // Ajustar para pegar o usuário atual quando houver autenticação
                };

                await _context.RampAllocations.AddAsync(rampAllocation);

                // Liberar rampa
                rampa.CurrentPickRequestId = null;
                rampa.Status = "Disponível";

                await _context.SaveChangesAsync();

                return Ok(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Erro ao concluir embarque da rampa {rampaId}");
                return StatusCode(500, "Erro interno ao processar a requisição");
            }
        }
    }
}