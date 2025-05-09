namespace WCS_PG.Services.Models.request.V1
{
    public class EmbarquePickRequestDto
    {
        public string PickRequestId { get; set; }
        public string NumeroEmbarque { get; set; }
        public string StopId { get; set; }
        public string CarMoveId { get; set; }
    }

    public class EmbarqueItemDto
    {
        public string Sku { get; set; }
        public string Descricao { get; set; }
        public int? QuantidadeEsperada { get; set; }
        public int? QuantidadeRecebida { get; set; }
        public int TempoUltimaCaixa { get; set; }
        public int? QuantidadeRejeitoFinal { get; set; }
        public string PickRequestId { get; set; } // Adicionado para identificar a qual pick request pertence
    }

    public class EmbarqueInfoDto
    {
        public string NumeroEmbarque { get; set; }
        public string Cliente { get; set; }
        public string Transportadora { get; set; }
        public List<string> Customizacao { get; set; }
        public int CaixasAlocadas { get; set; }
        public int SkusEsperados { get; set; }
        public List<EmbarqueItemDto> Itens { get; set; }
        public List<EmbarquePickRequestDto> PickRequests { get; set; } // Lista de pick requests associados
    }

}
