namespace WCS_PG.Services.Models.request.V1
{
    public class RampaInfoDto
    {
        public int Numero { get; set; }
        public string NumeroEmbarque { get; set; }
        public string Cliente { get; set; }
        public string Status { get; set; }
        public int QuantidadeSolicitada { get; set; }
        public int QuantidadeSeparada { get; set; }
        public int ItensPendentes { get; set; }
        public decimal PercentualAtendido { get; set; }
    }

    public class RampaDetailDto
    {
        public int RampaId { get; set; }
        public string RampaNumber { get; set; }
        public string Embarque { get; set; }
        public string Cliente { get; set; }
        public string Status { get; set; }
        public int Percentage { get; set; }
        public List<RampaItemDto> Itens { get; set; }
    }

    public class RampaItemDto
    {
        public string Sku { get; set; }
        public string Descricao { get; set; }
        public int QuantidadeEsperada { get; set; }
        public int QuantidadeRecebida { get; set; }
        public int Percentual { get; set; }
    }
}
