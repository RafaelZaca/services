namespace WCS_PG.Services.Models.request.Integracao
{
    public class PickCancelDto
    {
        public PickCancel PICK_CANCEL { get; set; }

        public class PickCancel
        {
            public ControlPickCancelSegmentDto CTRL_SEG { get; set; }
        }
        public class ControlPickCancelSegmentDto
        {

            public Int64 TRANID { get; set; }
            public Int64 TRANDT { get; set; }
            public string WCS_ID { get; set; }
            public string WH_ID { get; set; }
            public PickCancelSegmentDto PCK_CAN_SEG { get; set; }
        }

        public class PickCancelSegmentDto
        {
            public string WRKREF { get; set; }
            public string CANCOD { get; set; }
        }
    }
}
