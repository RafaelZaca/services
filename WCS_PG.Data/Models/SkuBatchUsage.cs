using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WCS_PG.Data.Models
{
    public class SkuBatchUsage
    {
        public int Id { get; set; } // PK, autoincrement
        public int SkuBatchId { get; set; } // FK para SkuBatch
        public string PickRequestId { get; set; } // FK para PickRequest
        public int RampId { get; set; } // FK para Ramp
        public int Quantity { get; set; } // Quantidade utilizada
        public DateTime UsedAt { get; set; } // Data/hora da utilização

        public virtual SkuBatch SkuBatch { get; set; }
        public virtual PickRequest PickRequest { get; set; }
        public virtual Ramp Ramp { get; set; }
    }
}
