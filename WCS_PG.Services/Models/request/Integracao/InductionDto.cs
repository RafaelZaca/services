namespace WCS_PG.Services.Models.request.Integracao
{
    public class InductionDto
    {
        public InductionControlDto INDUCTION { get; set; }
    }
    public class InductionControlDto
    {
        public InductionControlSegmentDto CTRL_SEG { get; set; }
    }

    public class InductionControlSegmentDto
    {
        public Int64 TRANID { get; set; }
        public Int64 TRANDT { get; set; }
        public string WCS_ID { get; set; }
        public string WH_ID { get; set; }
        public MoveRequestSegmentDto MOVE_REQ_SEG { get; set; }
    }

    public class MoveRequestSegmentDto
    {
        public string LODNUM { get; set; } // Pallet LPN
        public string SRCLOC { get; set; } // Source location
        public string DSTLOC { get; set; } // Immediate destination location
        public string LODHGT { get; set; } // Load height
        public string LODWGT { get; set; } // Load weight
        public string ASSET_TYP { get; set; } // Asset type
        public string BUNDLED_FLG { get; set; }
        public string DISTRO_PALOPN_FLG { get; set; }
        public string LOAD_ATTR1_FLG { get; set; }
        public string LOAD_ATTR2_FLG { get; set; }
        public string LOAD_ATTR3_FLG { get; set; }
        public string LOAD_ATTR4_FLG { get; set; }
        public string LOAD_ATTR5_FLG { get; set; }
        public string FINAL_DSTLOC { get; set; }
        public string DSTARE { get; set; }
        public string GROUP_ID { get; set; }
        public string LIST_ID { get; set; }
        public string VC_GROUP_SEQUENCE { get; set; }
        public InvSegmentDto INV_SEG { get; set; }
    }

    public class InvSegmentDto
    {
        public string SUBNUM { get; set; }
        public string DTLNUM { get; set; }
        public string UNTQTY { get; set; }
        public string PRTNUM { get; set; }
        public string PRT_CLIENT_ID { get; set; }
        public string FTPCOD { get; set; }
        public string UNTCAS { get; set; }
        public string UNTPAK { get; set; }
        public string LOTNUM { get; set; }
        public string SUP_LOTNUM { get; set; }
        public string REVLVL { get; set; }
        public string ORGCOD { get; set; }
        public string CATCH_QTY { get; set; }
        public Int64 MANDTE { get; set; }
        public string EXPIRE_DTE { get; set; }
        public string SUPNUM { get; set; }
        public string INVSTS { get; set; }
        public int CSTMS_BOND_FLG { get; set; }
        public int DTY_STMP_FLG { get; set; }
        public int CNSG_FLG { get; set; }
        public string STKUOM { get; set; }
        public string CAR_MOVE_ID { get; set; }
        public string STOP_NAME { get; set; }
        public string STOP_SEQ { get; set; }
        public string SHIP_ID { get; set; }
    }
}
