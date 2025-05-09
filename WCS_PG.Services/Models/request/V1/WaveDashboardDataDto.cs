using Microsoft.AspNetCore.Mvc;

namespace WCS_PG.Services.Models.request.V1
{
    // WCS.Api/DTOs/WaveOverview/WaveDashboardDataDto.cs
    public class WaveDashboardDataDto
    {
        public BoxAllocationDto AllocatedBoxes { get; set; }
        public SkuAllocationDto ExpectedSkus { get; set; }
        public int AllocatedShipments { get; set; }
        public int ActiveOperators { get; set; }
        public List<TypeValueDto> ShipmentTypes { get; set; }
        public List<TypeValueDto> OperatorActivities { get; set; }
        public ReadRejectionDto ReadRejection { get; set; }
        public ExcessRejectionDto ExcessRejection { get; set; }
        public int Productivity { get; set; }
    }

    public class BoxAllocationDto
    {
        public int Total { get; set; }
        public int Separated { get; set; }
        public int Pending { get; set; }
    }

    public class SkuAllocationDto
    {
        public int Total { get; set; }
        public int Separated { get; set; }
        public int Pending { get; set; }
    }

    public class TypeValueDto
    {
        public string Type { get; set; }
        public string Value { get; set; }
    }

    public class ReadRejectionDto
    {
        public int TotalRejected { get; set; }
        public int Treated { get; set; }
        public int Pending { get; set; }
    }

    public class ExcessRejectionDto
    {
        public int Total { get; set; }
        public int NotExpected { get; set; }
        public int Excess { get; set; }
        public int FullRamp { get; set; }
    }

}
