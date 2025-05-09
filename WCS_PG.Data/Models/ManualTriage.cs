using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WCS_PG.Data.Models
{
    public class ManualTriage
    {
        public int Id { get; set; } // PK, autoincrement
        public string Sku { get; set; } // SKU triado
        public int DestinationRampId { get; set; } // FK para Ramp de destino
        public string BatchNumber { get; set; } // Lote do item
        public int Quantity { get; set; } // Quantidade movimentada
        public DateTime TriagedAt { get; set; } // Data/hora da triagem
        public int TriagedByUserId { get; set; } // FK para User que realizou
        public string PickRequestId { get; set; } // FK para PickRequest
        public int? RejectionId { get; set; } // FK nullable para Rejection (se veio de rejeição)

        public virtual Ramp DestinationRamp { get; set; }
        public virtual User TriagedByUser { get; set; }
        public virtual PickRequest PickRequest { get; set; }
        public virtual Rejection Rejection { get; set; }
    }
}
