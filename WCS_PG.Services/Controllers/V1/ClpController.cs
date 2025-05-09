// WCS_PG.Services/Controllers/V1/ClpController.cs

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WCS_PG.Data;
using WCS_PG.Services.Services;

namespace WCS_PG.Services.Controllers.V1
{
    [ApiController]
    [Route("api/[controller]")]
    public class ClpController : ControllerBase
    {
        private readonly ILogger<ClpController> _logger;
        private readonly ClpCommunicationService _clpService;
        private readonly WCSContext _context;

        public ClpController(
            ILogger<ClpController> logger,
            ClpCommunicationService clpService,
            WCSContext context)
        {
            _logger = logger;
            _clpService = clpService;
            _context = context;
        }

        [HttpGet("status")]
        [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
        public ActionResult<bool> GetStatus()
        {
            bool isConnected = _clpService.EnsureConnection();
            return Ok(isConnected);
        }

        [HttpPost("test")]
        [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
        public ActionResult<bool> TestWrite([FromQuery] int position = 0, [FromQuery] string sku = "TESTE", [FromQuery] int quantity = 10, [FromQuery] int ramp = 1)
        {
            bool success = _clpService.WriteItemToSorterBank(position, sku, quantity, ramp);
            return Ok(success);
        }

        [HttpPost("clear")]
        [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
        public ActionResult<bool> ClearBank()
        {
            bool success = _clpService.ClearSorterBank();
            return Ok(success);
        }

        [HttpPost("sync-wave/{waveNumber}")]
        [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<bool>> SyncWave(string waveNumber)
        {
            // Buscar a wave
            var wave = await _context.Waves
                .FirstOrDefaultAsync(w => w.WaveNumber == waveNumber);

            if (wave == null)
            {
                return NotFound($"Wave {waveNumber} não encontrada");
            }

            // Buscar pick requests associados
            var pickRequestIds = await _context.WavePickRequests
                .Where(wp => wp.WaveId == wave.Id)
                .Select(wp => wp.PickRequestId)
                .ToListAsync();

            if (!pickRequestIds.Any())
            {
                return BadRequest($"Nenhum pick request associado à wave {waveNumber}");
            }

            // Buscar alocações de rampa
            var rampAllocations = await _context.RampAllocations
                .Where(ra => pickRequestIds.Contains(ra.PickRequestId) && ra.ReleasedAt == null)
                .ToListAsync();

            // Criar mapeamento de pickRequestId para rampId
            Dictionary<string, int> rampMappings = rampAllocations
                .ToDictionary(ra => ra.PickRequestId, ra => ra.RampId);

            // Buscar todos os itens
            var items = await _context.PickRequestItems
                .Where(pri => pickRequestIds.Contains(pri.PickRequestId))
                .ToListAsync();

            // Sincronizar com o PLC
            bool success = _clpService.SyncWaveWithPlc(items, rampMappings);

            if (success)
            {
                _logger.LogInformation($"Wave {waveNumber} sincronizada com o PLC com sucesso");
            }
            else
            {
                _logger.LogWarning($"Falha ao sincronizar wave {waveNumber} com o PLC");
            }

            return Ok(success);
        }
    }
}