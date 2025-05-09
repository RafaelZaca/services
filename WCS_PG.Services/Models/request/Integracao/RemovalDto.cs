namespace WCS_PG.Services.Models.request.Integracao
{
    public class RemovalDto
    {
        public RemovalControlDto REMOVAL { get; set; }
    }
    public class RemovalControlDto
    {
        public RemovalControlSegmentDto CTRL_SEG { get; set; }
    }

    public class RemovalControlSegmentDto
    {
        public Int64 TRANID { get; set; }
        public Int64 TRANDT { get; set; }
        public string WCS_ID { get; set; }
        public string WH_ID { get; set; }
        public RemovalSegmentDto REMOVAL_SEG { get; set; }
    }

    public class RemovalSegmentDto
    {
        public string LODNUM { get; set; } // Pallet LPN
        public string STOLOC { get; set; } // Current pallet location
    }
}
