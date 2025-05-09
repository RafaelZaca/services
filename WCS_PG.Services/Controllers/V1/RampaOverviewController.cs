using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WCS_PG.Data;
using WCS_PG.Services.Models.request.V1;

namespace WCS_PG.Services.Controllers.V1
{
    [ApiController]
    [Route("api/[controller]")]
    public class RampaOverviewController : ControllerBase
    {
        private readonly ILogger<RampaOverviewController> _logger;
        private readonly WCSContext _context;

        public RampaOverviewController(ILogger<RampaOverviewController> logger, WCSContext context)
        {
            _logger = logger;
            _context = context;
        }

        [HttpGet]
        [ProducesResponseType(typeof(List<RampaDataDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<List<RampaDataDto>>> GetRampasData()
        {
            try
            {
                // Buscar dados de rampas e pick requests ativos
                var rampasDb = await _context.Ramps
                    .Include(r => r.CurrentPickRequest)
                    .ThenInclude(pr => pr.Items)
                    .OrderBy(r => r.RampNumber)
                    .ToListAsync();

                var rampasDto = new List<RampaDataDto>();

                if (rampasDb.Any())
                {
                    foreach (var rampa in rampasDb)
                    {
                        // Calcular percentual de conclusão
                        int percentual = 0;
                        if (rampa.CurrentPickRequest != null)
                        {
                            var items = rampa.CurrentPickRequest.Items;
                            int totalEsperado = (int)(items.Sum(i => i.ExpectedQuantity) == null ? 0 : items.Sum(i => i.ExpectedQuantity)!);
                            int totalRecebido = (int)(items.Sum(i => i.ReceivedQuantity) == null ? 0 : items.Sum(i => i.ExpectedQuantity)!);

                            if (totalEsperado > 0)
                                percentual = (int)Math.Round((double)totalRecebido / totalEsperado * 100);
                        }

                        rampasDto.Add(new RampaDataDto
                        {
                            Number = rampa.RampNumber,
                            Embarque = rampa.CurrentPickRequest?.OrderNumber ?? string.Empty,
                            Cliente = rampa.CurrentPickRequest?.ClientName ?? string.Empty,
                            Percentage = percentual
                        });
                    }
                }
                else
                {
                    //// Mock data caso não tenha dados reais
                    //for (int i = 1; i <= 12; i++)
                    //{
                    //    rampasDto.Add(new RampaDataDto
                    //    {
                    //        Number = i.ToString().PadLeft(2, '0'),
                    //        Embarque = "1234567890",
                    //        Cliente = "Sendas sup.",
                    //        Percentage = 85
                    //    });
                    //    i++;
                    //    rampasDto.Add(new RampaDataDto
                    //    {
                    //        Number = i.ToString().PadLeft(2, '0'),
                    //        Embarque = "05602456",
                    //        Cliente = "Varejo",
                    //        Percentage = 75
                    //    });
                    //}
                }

                return Ok(rampasDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar dados das rampas");
                return StatusCode(500, "Erro interno ao processar a requisição");
            }
        }

        [HttpGet("{rampaId}")]
        [ProducesResponseType(typeof(RampaDetailDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<RampaDetailDto>> GetRampaDetail(int rampaId)
        {
            try
            {
                var rampa = await _context.Ramps
                    .Include(r => r.CurrentPickRequest)
                    .ThenInclude(pr => pr.Items)
                    .FirstOrDefaultAsync(r => r.Id == rampaId);

                if (rampa == null)
                {
                    return NotFound($"Rampa com ID {rampaId} não encontrada");
                }

                // Calcular percentual de conclusão
                int percentual = 0;
                var itemDetails = new List<RampaItemDto>();

                if (rampa.CurrentPickRequest != null)
                {
                    var items = rampa.CurrentPickRequest.Items;
                    int totalEsperado = (int)(items.Sum(i => i.ExpectedQuantity) == null ? 0 : items.Sum(i => i.ExpectedQuantity)!);
                    int totalRecebido = (int)(items.Sum(i => i.ReceivedQuantity) == null ? 0 : items.Sum(i => i.ExpectedQuantity)!);

                    if (totalEsperado > 0)
                        percentual = (int)Math.Round((double)totalRecebido / totalEsperado * 100);

                    // Detalhes de cada item
                    itemDetails = items.Select(i => new RampaItemDto
                    {
                        Sku = i.Sku,
                        Descricao = i.Description,
                        QuantidadeEsperada = (int)(items.Sum(i => i.ExpectedQuantity) == null ? 0 : items.Sum(i => i.ExpectedQuantity)!),
                        QuantidadeRecebida = (int)(items.Sum(i => i.ReceivedQuantity) == null ? 0 : items.Sum(i => i.ExpectedQuantity)!),
                        Percentual = i.ExpectedQuantity > 0 ?
                            (int)Math.Round((double)((decimal)i.ReceivedQuantity! / i.ExpectedQuantity * 100)) : 0
                    }).ToList();
                }

                var result = new RampaDetailDto
                {
                    RampaId = rampa.Id,
                    RampaNumber = rampa.RampNumber,
                    Embarque = rampa.CurrentPickRequest?.OrderNumber ?? string.Empty,
                    Cliente = rampa.CurrentPickRequest?.ClientName ?? string.Empty,
                    Status = rampa.Status,
                    Percentage = percentual,
                    Itens = itemDetails
                };

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Erro ao buscar detalhes da rampa {rampaId}");
                return StatusCode(500, "Erro interno ao processar a requisição");
            }
        }
    }
}

