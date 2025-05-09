using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WCS_PG.Data.Models
{
    public class Wave
    {
        public int Id { get; set; } // PK, autoincrement
        public string WaveNumber { get; set; } // Número da wave, único
        public string Status { get; set; } // Status da wave
        public DateTime CreatedAt { get; set; } // Data de criação
        public DateTime? StartedAt { get; set; } // Data de início
        public DateTime? CompletedAt { get; set; } // Data de conclusão
        public int CreatedByUserId { get; set; } // FK para Users

        public virtual User CreatedByUser { get; set; }
        public virtual ICollection<WavePickRequest> WavePickRequests { get; set; }
    }
}
