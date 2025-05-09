using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WCS_PG.Data.Models
{
    public class ProductionHourlyDetail
    {
        public int Id { get; set; } // PK, autoincrement
        public int ProductionHourlyId { get; set; } // FK para ProductionHourly
        public int RampId { get; set; } // FK para Ramp
        public int ProducedQuantity { get; set; } // Quantidade produzida na rampa
        public int RejectedQuantity { get; set; } // Quantidade rejeitada na rampa
        public decimal EfficiencyRate { get; set; } // Taxa de eficiência da rampa

        public virtual ProductionHourly ProductionHourly { get; set; }
        public virtual Ramp Ramp { get; set; }
    }
}
