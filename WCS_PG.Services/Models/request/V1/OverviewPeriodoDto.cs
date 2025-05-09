namespace WCS_PG.Services.Models.request.V1
{
    public class SeriesItemDto
    {
        public string Name { get; set; }
        public decimal Value { get; set; }
    }

    public class ProductionDataDto
    {
        public string Name { get; set; }
        public List<SeriesItemDto> Series { get; set; }
    }

    public class SummaryDataDto
    {
        public int ProcessedBoxes { get; set; }
        public int RejectedBoxes { get; set; }
        public decimal OperatingHours { get; set; }
        public decimal StoppedHours { get; set; }
        public int Waves { get; set; }
        public int Shipments { get; set; }
    }
}
