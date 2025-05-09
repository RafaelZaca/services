namespace WCS_PG.Services.Models.request.V1
{
    public class PickRequestDto
    {
        public string Id { get; set; }
        public string NumeroEmbarque { get; set; }
        public string StopId { get; set; }
        public string Delivery { get; set; }
        public string Cliente { get; set; }
        public string Particularidade { get; set; }
        public int Quantidade { get; set; }
        public int Skus { get; set; }
        public string Status { get; set; }
        public int? Rampa { get; set; }
        public string? DataInicio { get; set; }
    }
}
