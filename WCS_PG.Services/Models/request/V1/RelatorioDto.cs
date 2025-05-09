namespace WCS_PG.Services.Models.request.V1
{
    public class ColunasDisponiveisDto
    {
        public string Campo { get; set; }
        public string Titulo { get; set; }
        public string Tipo { get; set; }
    }

    public class FiltroRelatorioDto
    {
        public DateTime? DataInicio { get; set; }
        public DateTime? DataFim { get; set; }
        public List<string> Status { get; set; }
        public List<int> Rampas { get; set; }
        public List<string> ColunasSelecionadas { get; set; }
    }

    public class DadosRelatorioDto
    {
        public string Id { get; set; }
        public string Data { get; set; }
        public int Rampa { get; set; }
        public string Status { get; set; }
        public string NumeroEmbarque { get; set; }
        public string Cliente { get; set; }
        public int QuantidadeEsperada { get; set; }
        public int QuantidadeSeparada { get; set; }
        public int ItensPendentes { get; set; }
        public decimal PercentualAtendido { get; set; }
        public string DataInicio { get; set; }
        public string DataFim { get; set; }
        public int TempoTotal { get; set; }
        public string UsuarioResponsavel { get; set; }
    }

    public class DownloadRelatorioDto
    {
        public List<DadosRelatorioDto> Dados { get; set; }
        public List<string> Colunas { get; set; }
    }
}
