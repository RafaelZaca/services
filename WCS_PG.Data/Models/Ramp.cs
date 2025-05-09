using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WCS_PG.Data.Models
{
    public class Ramp
    {
        public int Id { get; set; } // PK, autoincrement
        public string RampNumber { get; set; } // Número da rampa (01-12)
        public string Status { get; set; } // Status (Disponível, Em Uso, etc)
        public bool IsActive { get; set; } // Se a rampa está ativa
        public string? CurrentPickRequestId { get; set; } // FK nullable para PickRequest atual
        public virtual PickRequest? CurrentPickRequest { get; set; }
        public virtual ICollection<RampAllocation> RampAllocations { get; set; }
    }
}
