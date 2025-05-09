namespace WCS_PG.Services.Models.request.V1
{
    public class SeparacaoInfoDto
    {
        public int RejeitoNoRead { get; set; }
        public int Pendente { get; set; }
        public List<SeparacaoItemDto> Itens { get; set; }
    }

    public class SeparacaoItemDto
    {
        public string Sku { get; set; }
        public int QuantidadeEsperada { get; set; }
        public int QuantidadeInduzida { get; set; }
        public int PercentualInd { get; set; }
        public int QuantidadeRecebida { get; set; }
        public int PercentualRec { get; set; }
        public int TempoUltimaCaixa { get; set; }
        public string Descricao { get; set; }
        public int QuantidadeRejeitoFinal { get; set; }
        public int QuantidadeRejeitoNoRead { get; set; }
        public int Pendente { get; set; }
    }
}
