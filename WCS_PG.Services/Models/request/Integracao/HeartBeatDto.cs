using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System.Numerics;

namespace WCS_PG.Services.Models.request.Integracao
{
    public class HeartbeatConfirmDto
    {
        public HeartbeatControlDto HEARTBEAT_CONFIRM { get; set; }
    }
    public class HeartbeatInitiateDto
    {
        public HeartbeatControlDto HEARTBEAT_INITIATE { get; set; }
    }

    public class HeartbeatControlDto
    {
        public HeartbeatControlSegmentDto CTRL_SEG { get; set; }

    }

    public class HeartbeatSegmentDto
    {
        public string TEXT { get; set; }
    }

    // Add HeartbeatSeg property to ControlSegmentDto
    public class HeartbeatControlSegmentDto
    {
        public Int64 TRANID { get; set; }
        public Int64 TRANDT { get; set; }
        public string WCS_ID { get; set; }
        public string WH_ID { get; set; }
        public HeartbeatSegmentDto HEARTBEAT_SEG { get; set; }
        // Other properties remain the same
    }

}
