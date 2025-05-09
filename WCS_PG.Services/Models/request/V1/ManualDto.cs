namespace WCS_PG.Services.Models.request.V1
{
    public class SkuInfoDto
    {
        public string Sku { get; set; }
        public string Descricao { get; set; }
        public List<DestinationInfoDto> Destinos { get; set; }
    }

    public class DestinationInfoDto
    {
        public int Rampa { get; set; }
        public int Caixas { get; set; }
    }

    public class MovimentacaoDto
    {
        public string Sku { get; set; }
        public int Rampa { get; set; }
        public string Lote { get; set; }
        public int Quantidade { get; set; }
    }

    public class ValidacaoQuantidadeDto
    {
        public string Sku { get; set; }
        public int Rampa { get; set; }
        public int Quantidade { get; set; }
    }

    public class ValidacaoQuantidadeResultDto
    {
        public bool Valido { get; set; }
        public string Mensagem { get; set; }
    }
}
