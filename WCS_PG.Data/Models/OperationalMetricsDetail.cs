using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WCS_PG.Data.Models
{
    public class OperationalMetricsDetail
    {
        public int Id { get; set; } // PK, autoincrement
        public int OperationalMetricsId { get; set; } // FK para OperationalMetrics
        public DateTime StartTime { get; set; } // Início do período
        public DateTime EndTime { get; set; } // Fim do período
        public int ProcessedBoxes { get; set; } // Caixas processadas no período
        public int RejectedBoxes { get; set; } // Caixas rejeitadas no período
        public string StopReason { get; set; } // Motivo de parada (se houver)
        public decimal ProductivityRate { get; set; } // Taxa de produtividade

        public virtual OperationalMetrics OperationalMetrics { get; set; }
    }
}
