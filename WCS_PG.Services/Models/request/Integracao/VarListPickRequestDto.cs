namespace WCS_PG.Services.Models.request.Integracao
{
    public class VarListPickRequestDto
    {
        public VarListPickRequestControlDto VAR_LIST_PICK_REQUEST { get; set; }
    }
    public class VarListPickRequestControlDto
    {
        public ControlSegmentDto CTRL_SEG { get; set; }
    }

    public class ControlSegmentDto
    {
        public Int64 TRANID { get; set; }
        public Int64 TRANDT { get; set; }
        public string WCS_ID { get; set; }
        public string WH_ID { get; set; }
        public PalletSegmentDto PALLET_SEG { get; set; }
    }

    public class PalletSegmentDto
    {
        public string LIST_ID { get; set; }
        public string VC_PAL_TYP { get; set; }
        public string ASSET_TYP { get; set; }
        public string CAR_MOVE_ID { get; set; }
        public string STOP_NAM { get; set; }
        public string ORDNUM { get; set; }
        public string CPONUM { get; set; }
        public string CARCOD { get; set; }
        public string CARNAM { get; set; }
        public string RTCUST { get; set; }
        public string ADRNAM { get; set; }
        public string ADRLN1 { get; set; }
        public string ADRLN2 { get; set; }
        public string ADRCTY { get; set; }
        public string ADRSTC { get; set; }
        public string CTRY_NAME { get; set; }
        public int VC_LIVE_LOAD_FLG { get; set; }
        public string DSTLOC { get; set; }
        public List<PickSegmentDto> PICK_SEG { get; set; }
    }

    public class PickSegmentDto
    {
        public string PRTNUM { get; set; }
        public string PRT_CLIENT_ID { get; set; }
        public int LIST_SEQNUM { get; set; }
        public string WRKREF { get; set; }
        public string SRCLOC { get; set; }
        public int PCKQTY { get; set; }
        public string PCK_UOM { get; set; }
        public string SRCARE { get; set; }
        public string EARLY_SHPDTE { get; set; }
        public string INVSTS { get; set; }
        public string INVSTS_PRG { get; set; }
        public string CSTPRT { get; set; }
        public int CASQTY { get; set; }
        public List<PickInstructionSegmentDto>? PICK_INSTRUCTION_SEG { get; set; }
    }

    public class PickInstructionSegmentDto
    {
        public string ASSET_TYPE { get; set; }
        public string PRT_NUM { get; set; }
        public string PICK_INSTR { get; set; }
        public int SEQ_NUM { get; set; }
    }
}
