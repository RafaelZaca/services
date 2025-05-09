using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WCS_PG.Data.Models
{
    public class RampAllocation
    {
        public int Id { get; set; } // PK, autoincrement
        public int RampId { get; set; } // FK para Ramps
        public string PickRequestId { get; set; } // FK para PickRequests
        public DateTime AllocatedAt { get; set; } // Data de alocação
        public DateTime? ReleasedAt { get; set; } // Data de liberação
        public int AllocatedByUserId { get; set; } // FK para Users
        public int? ReleasedByUserId { get; set; } // FK nullable para Users

        public virtual Ramp Ramp { get; set; }
        public virtual PickRequest PickRequest { get; set; }
        public virtual User AllocatedByUser { get; set; }
        public virtual User ReleasedByUser { get; set; }
    }
}
