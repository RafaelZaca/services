using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WCS_PG.Data.Models
{
    public class Rejection
    {
        public int Id { get; set; } // PK, autoincrement
        public string PickRequestId { get; set; } // FK para PickRequest
        public string Sku { get; set; } // SKU do item rejeitado
        public string RejectionType { get; set; } // Tipo (NoRead, Excesso, RampaCheia)
        public int RampId { get; set; } // FK para Ramp (rampa destino original)
        public DateTime RejectedAt { get; set; } // Data/hora da rejeição
        public bool IsTreated { get; set; } // Se já foi tratado
        public DateTime? TreatedAt { get; set; } // Data/hora do tratamento
        public int? TreatedByUserId { get; set; } // FK para User que tratou
        public string BatchNumber { get; set; } // Lote do item
        public string TreatmentNotes { get; set; } // Observações do tratamento

        public virtual PickRequest PickRequest { get; set; }
        public virtual Ramp Ramp { get; set; }
        public virtual User TreatedByUser { get; set; }
    }
}
