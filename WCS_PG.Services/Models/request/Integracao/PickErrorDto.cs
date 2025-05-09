namespace WCS_PG.Services.Models.request.Integracao
{
    public class PickErrorDto
    {
        public PickError PICK_ERROR { get; set; }

        public class PickError
        {
            public ControlPickErrorSegmentDto CTRL_SEG { get; set; }
        }
        public class ControlPickErrorSegmentDto
        {

            public Int64 TRANID { get; set; }
            public Int64 TRANDT { get; set; }
            public string WCS_ID { get; set; }
            public string WH_ID { get; set; }
            public PickErrorSegmentDto PICK_ERR_SEG { get; set; }
        }

        public class PickErrorSegmentDto
        {
            public string WRKREF { get; set; }
            public string CANCOD { get; set; }
            public string ERROR_DESCR { get; set; }
        }
    }
}
