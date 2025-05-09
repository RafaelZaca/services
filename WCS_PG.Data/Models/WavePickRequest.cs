using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WCS_PG.Data.Models
{
    public class WavePickRequest
    {
        public int WaveId { get; set; } // PK, FK para Waves
        public string PickRequestId { get; set; } // PK, FK para PickRequests
        public DateTime AddedAt { get; set; } // Data de adição à wave
        public int AddedByUserId { get; set; } // FK para Users

        public virtual Wave Wave { get; set; }
        public virtual PickRequest PickRequest { get; set; }
        public virtual User AddedByUser { get; set; }
    }


}
