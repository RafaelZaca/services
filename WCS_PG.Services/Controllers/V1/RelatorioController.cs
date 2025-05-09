using Microsoft.AspNetCore.Mvc;
using WCS_PG.Services.Models.request.V1;

namespace WCS_PG.Services.Controllers.V1
{
    [ApiController]
    [Route("api/[controller]")]
    public class RelatorioController : ControllerBase
    {
        private readonly ILogger<RelatorioController> _logger;

        public RelatorioController(ILogger<RelatorioController> logger)
        {
            _logger = logger;
        }

        [HttpGet("colunas")]
        [ProducesResponseType(typeof(List<ColunasDisponiveisDto>), StatusCodes.Status200OK)]
        public ActionResult<List<ColunasDisponiveisDto>> GetColunasDisponiveis()
        {
            var colunas = new List<ColunasDisponiveisDto>
        {
            new() { Campo = "data", Titulo = "Data", Tipo = "data" },
            new() { Campo = "rampa", Titulo = "Rampa", Tipo = "numero" },
            new() { Campo = "status", Titulo = "Status", Tipo = "texto" },
            new() { Campo = "numeroEmbarque", Titulo = "Nº Embarque", Tipo = "texto" },
            new() { Campo = "cliente", Titulo = "Cliente", Tipo = "texto" },
            new() { Campo = "quantidadeEsperada", Titulo = "Qtd. Esperada", Tipo = "numero" },
            new() { Campo = "quantidadeSeparada", Titulo = "Qtd. Separada", Tipo = "numero" },
            new() { Campo = "itensPendentes", Titulo = "Itens Pendentes", Tipo = "numero" },
            new() { Campo = "percentualAtendido", Titulo = "% Atendido", Tipo = "numero" },
            new() { Campo = "dataInicio", Titulo = "Data Início", Tipo = "data" },
            new() { Campo = "dataFim", Titulo = "Data Fim", Tipo = "data" },
            new() { Campo = "tempoTotal", Titulo = "Tempo Total (min)", Tipo = "numero" },
            new() { Campo = "usuarioResponsavel", Titulo = "Usuário Responsável", Tipo = "texto" }
        };

            return Ok(colunas);
        }

        [HttpGet("status")]
        [ProducesResponseType(typeof(List<string>), StatusCodes.Status200OK)]
        public ActionResult<List<string>> GetStatusDisponiveis()
        {
            return Ok(new List<string> { "Completo", "Em separação", "Vazio", "Pendente" });
        }

        [HttpGet("rampas")]
        [ProducesResponseType(typeof(List<int>), StatusCodes.Status200OK)]
        public ActionResult<List<int>> GetRampasDisponiveis()
        {
            return Ok(Enumerable.Range(1, 12).ToList());
        }

        [HttpPost("gerar")]
        [ProducesResponseType(typeof(List<DadosRelatorioDto>), StatusCodes.Status200OK)]
        public ActionResult<List<DadosRelatorioDto>> GerarRelatorio([FromBody] FiltroRelatorioDto filtros)
        {
            var dadosMock = new List<DadosRelatorioDto>
        {
            new()
            {
                Id = "1",
                Data = "2025-02-14",
                Rampa = 1,
                Status = "Completo",
                NumeroEmbarque = "12345678",
                Cliente = "Supermercado BH",
                QuantidadeEsperada = 850,
                QuantidadeSeparada = 850,
                ItensPendentes = 0,
                PercentualAtendido = 100,
                DataInicio = "2025-02-14T08:00:00",
                DataFim = "2025-02-14T10:30:00",
                TempoTotal = 150,
                UsuarioResponsavel = "João Silva"
            }
        };

            // Aplicar filtros
            var dadosFiltrados = dadosMock;
            if (filtros.DataInicio.HasValue)
            {
                dadosFiltrados = dadosFiltrados.Where(d =>
                    DateTime.Parse(d.Data) >= filtros.DataInicio.Value).ToList();
            }
            // ... demais filtros

            return Ok(dadosFiltrados);
        }

        [HttpPost("download")]
        [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK)]
        public ActionResult DownloadRelatorio([FromBody] DownloadRelatorioDto request)
        {
            // Aqui viria a lógica de geração do arquivo
            byte[] fileBytes = System.Text.Encoding.UTF8.GetBytes("Conteúdo do relatório");
            string fileName = $"relatorio_{DateTime.Now:yyyyMMddHHmmss}.csv";

            return File(fileBytes, "text/csv", fileName);
        }
    }
}

