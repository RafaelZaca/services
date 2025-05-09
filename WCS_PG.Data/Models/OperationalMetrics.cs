using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WCS_PG.Data.Models
{
    public class OperationalMetrics
    {
        public int Id { get; set; } // PK, autoincrement
        public DateTime Date { get; set; } // Data da métrica
        public int ProcessedBoxes { get; set; } // Caixas processadas
        public int RejectedBoxes { get; set; } // Caixas rejeitadas
        public decimal OperatingHours { get; set; } // Horas em operação
        public decimal StoppedHours { get; set; } // Horas paradas
        public int CompletedWaves { get; set; } // Waves concluídas
        public int CompletedShipments { get; set; } // Embarques concluídos
        public decimal SystemAvailability { get; set; } // Disponibilidade do sistema
        public decimal AverageWaveCompletionTime { get; set; } // Tempo médio de conclusão de wave
        public string ShiftId { get; set; } // Identificador do turno

        public virtual ICollection<OperationalMetricsDetail> Details { get; set; }
    }
}
