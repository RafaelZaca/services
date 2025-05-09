using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WCS_PG.Data.Models
{
    public class RampMetrics
    {
        public int Id { get; set; } // PK, autoincrement
        public int RampId { get; set; } // FK para Ramp
        public DateTime Date { get; set; } // Data da métrica
        public int TotalPickRequests { get; set; } // Total de pick requests
        public int TotalBoxes { get; set; } // Total de caixas
        public int TotalSkus { get; set; } // Total de SKUs distintos
        public decimal AverageProcessingTime { get; set; } // Tempo médio de processamento
        public decimal UtilizationRate { get; set; } // Taxa de utilização
        public int NoReadRejections { get; set; } // Rejeições no-read
        public int ExcessRejections { get; set; } // Rejeições por excesso
        public int FullRampRejections { get; set; } // Rejeições por rampa cheia

        public virtual Ramp Ramp { get; set; }
    }
}
