namespace WCS_PG.Services.Models.request.Integracao
{
    public class VarListPickConfirmDto
    {
        public VarListPickConfirm VAR_LIST_PICK_CONFIRM { get; set; }

        public class VarListPickConfirm
        {
            public ControlPickConfirmSegmentDto CTRL_SEG { get; set; }
        }
        public class ControlPickConfirmSegmentDto
        {

            public Int64 TRANID { get; set; }
            public Int64 TRANDT { get; set; }
            public string WCS_ID { get; set; }
            public string WH_ID { get; set; }
            public PickConfirmSegmentDto PICK_SEG { get; set; }
        }

        public class PickConfirmSegmentDto
        {
            public string LIST_ID { get; set; }
            public string WRKREF { get; set; }
            public string SRC_LODNUM { get; set; }
            public string DST_LODNUM { get; set; }
            public string DST_SUBNUM { get; set; }
            public string DSTLOC { get; set; }
            public string PCKQTY { get; set; }
            public string PCK_UOM { get; set; }
            public string LOTNUM { get; set; }
            public string ASSET_TYP { get; set; }
        }
    }

}
