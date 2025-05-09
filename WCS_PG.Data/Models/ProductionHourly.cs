using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WCS_PG.Data.Models
{
    public class ProductionHourly
    {
        public int Id { get; set; } // PK, autoincrement
        public DateTime Hour { get; set; } // Hora específica
        public int PlannedQuantity { get; set; } // Quantidade planejada
        public int ProducedQuantity { get; set; } // Quantidade produzida
        public int RejectedQuantity { get; set; } // Quantidade rejeitada
        public int Capacity { get; set; } // Capacidade da hora
        public string WaveNumber { get; set; } // Wave em execução
        public decimal EfficiencyRate { get; set; } // Taxa de eficiência
        public decimal RejectionRate { get; set; } // Taxa de rejeição

        public virtual ICollection<ProductionHourlyDetail> Details { get; set; }
    }
}
