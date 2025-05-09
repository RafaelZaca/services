using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WCS_PG.Data;
using WCS_PG.Data.Models;
using WCS_PG.Services.Models.request.V1;

[ApiController]
[Route("api/[controller]")]
public class EmbarqueController : ControllerBase
{
    private readonly ILogger<EmbarqueController> _logger;
    private readonly WCSContext _context;

    public EmbarqueController(ILogger<EmbarqueController> logger, WCSContext context)
    {
        _logger = logger;
        _context = context;
    }
    [HttpGet("{rampaId}")]
    [ProducesResponseType(typeof(EmbarqueInfoDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<EmbarqueInfoDto>> GetEmbarqueData(int rampaId)
    {
        try
        {
            // Buscar a rampa
            var rampa = await _context.Ramps
                .FirstOrDefaultAsync(r => r.Id == rampaId);

            if (rampa == null)
            {
                return NotFound($"Rampa com ID {rampaId} não encontrada");
            }

            if (rampa.CurrentPickRequestId == null)
            {
                return NotFound($"Não há pick request associado à rampa {rampaId}");
            }

            // Buscar o primeiro pick request associado à rampa
            var primaryPickRequest = await _context.PickRequests
                .Include(pr => pr.Items)
                .FirstOrDefaultAsync(pr => pr.Id == rampa.CurrentPickRequestId);

            if (primaryPickRequest == null)
            {
                return NotFound($"Pick request não encontrado para a rampa {rampaId}");
            }

            // Buscar outros pick requests que compartilham o mesmo StopId ou CarMoveId
            var relatedPickRequests = await _context.PickRequests
                .Include(pr => pr.Items)
                .Where(pr => pr.Id != primaryPickRequest.Id &&
                            (pr.StopId == primaryPickRequest.StopId ||
                             pr.CarMoveId == primaryPickRequest.CarMoveId))
                .Where(pr => _context.RampAllocations
                            .Any(ra => ra.PickRequestId == pr.Id &&
                                       ra.RampId == rampaId &&
                                       ra.ReleasedAt == null))
                .ToListAsync();

            // Combinar todos os pick requests
            var allPickRequests = new List<PickRequest> { primaryPickRequest };
            allPickRequests.AddRange(relatedPickRequests);

            // Agrupar todos os itens de todos os pick requests
            var allItems = allPickRequests.SelectMany(pr => pr.Items).ToList();

            // Buscar wave associada, se houver
            var wave = await _context.WavePickRequests
                .Include(wp => wp.Wave)
                .Where(wp => wp.PickRequestId == primaryPickRequest.Id)
                .Select(wp => wp.Wave)
                .FirstOrDefaultAsync();

            // Criar a lista de todos os números de embarque
            var allEmbarques = allPickRequests.Select(pr => pr.OrderNumber).Distinct().ToList();
            var embarquesStr = string.Join(", ", allEmbarques);

            // Preparar resposta
            var embarqueData = new EmbarqueInfoDto
            {
                NumeroEmbarque = embarquesStr,
                Cliente = primaryPickRequest.ClientName,
                Transportadora = "DHL Supply Chain Brasil", // Obter de onde?
                Customizacao = primaryPickRequest.Customization?.Split(',').ToList() ?? new List<string>(),
                CaixasAlocadas = allPickRequests.Sum(pr => pr.TotalQuantity),
                SkusEsperados = allPickRequests.Sum(pr => pr.TotalSkus),
                Itens = allItems.Select(item => new EmbarqueItemDto
                {
                    Sku = item.Sku,
                    Descricao = item.Description,
                    QuantidadeEsperada = item.ExpectedQuantity,
                    QuantidadeRecebida = item.ReceivedQuantity,
                    TempoUltimaCaixa = item.LastBoxReceivedAt.HasValue
                        ? (int)Math.Floor((DateTime.Now - item.LastBoxReceivedAt.Value).TotalMinutes)
                        : 0,
                    QuantidadeRejeitoFinal = item.FinalRejectionCount,
                    // Adicionar referência ao ID do pick request para diferenciar
                    PickRequestId = item.PickRequestId
                }).ToList(),
                // Adicionar a lista de pick requests associados
                PickRequests = allPickRequests.Select(pr => new EmbarquePickRequestDto
                {
                    PickRequestId = pr.Id,
                    NumeroEmbarque = pr.OrderNumber,
                    StopId = pr.StopId,
                    CarMoveId = pr.CarMoveId
                }).ToList()
            };

            return Ok(embarqueData);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Erro ao buscar dados do embarque para a rampa {rampaId}");
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

            // Obter o pick request principal
            var primaryPickRequest = rampa.CurrentPickRequest;

            // Identificar outros pick requests relacionados pelo mesmo StopId ou CarMoveId
            var relatedPickRequests = await _context.PickRequests
                .Where(pr => pr.Id != primaryPickRequest.Id &&
                            (pr.StopId == primaryPickRequest.StopId ||
                             pr.CarMoveId == primaryPickRequest.CarMoveId))
                .Where(pr => _context.RampAllocations
                            .Any(ra => ra.PickRequestId == pr.Id &&
                                       ra.RampId == rampaId &&
                                       ra.ReleasedAt == null))
                .ToListAsync();

            // Juntar todos os pick requests
            var allPickRequests = new List<PickRequest> { primaryPickRequest };
            allPickRequests.AddRange(relatedPickRequests);

            // Atualizar o status de todos os pick requests relacionados
            foreach (var pickRequest in allPickRequests)
            {
                pickRequest.Status = "Concluído";
                pickRequest.CompletedAt = DateTime.Now;
            }

            // Identificar todas as alocações de rampa relacionadas
            var rampAllocations = await _context.RampAllocations
                .Where(ra => allPickRequests.Select(pr => pr.Id).Contains(ra.PickRequestId) &&
                             ra.RampId == rampaId &&
                             ra.ReleasedAt == null)
                .ToListAsync();

            foreach (var rampAllocation in rampAllocations)
            {
                rampAllocation.ReleasedAt = DateTime.Now;
                rampAllocation.ReleasedByUserId = 1; // Ajustar para pegar o usuário atual
            }

            // Liberar rampa
            rampa.CurrentPickRequestId = null;
            rampa.Status = "Disponível";

            await _context.SaveChangesAsync();

            _logger.LogInformation($"Embarques concluídos para rampa {rampaId}: {string.Join(", ", allPickRequests.Select(pr => pr.OrderNumber))}");
            return Ok(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Erro ao concluir embarque da rampa {rampaId}");
            return StatusCode(500, "Erro interno ao processar a requisição");
        }
    }
}