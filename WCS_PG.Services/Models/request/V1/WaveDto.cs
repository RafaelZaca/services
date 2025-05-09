namespace WCS_PG.Services.Models.request.V1
{
    public class WavePickRequestDto
    {
        public string Embarque { get; set; }
        public string StopId { get; set; }
        public string CarMoveId { get; set; } // Novo campo
        public string Delivery { get; set; }
        public string Cliente { get; set; }
        public string Particularidade { get; set; }
        public int Quantidade { get; set; }
        public int Skus { get; set; }
        public string Status { get; set; }
        public int? Rampa { get; set; }
        public string GroupingKey { get; set; }
    }

    public class AlocacaoRampaDto
    {
        public string Embarque { get; set; }
        public int Rampa { get; set; }
    }
    public class WaveInfoDto
    {
        public string WaveNumber { get; set; }
        public string Status { get; set; }
        public string CreatedAt { get; set; }
        public List<WavePickRequestDto> PickRequests { get; set; }
    }
    public class WaveConfigDto
    {
        public string GroupingField { get; set; } // "CAR_MOVE_ID" ou "STOP_NAM"
    }
}
