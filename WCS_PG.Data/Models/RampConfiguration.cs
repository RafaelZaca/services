using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WCS_PG.Data.Models
{
    public class RampConfiguration
    {
        public int Id { get; set; } // PK, autoincrement
        public int RampId { get; set; } // FK para Ramp
        public int MaximumBoxes { get; set; } // Máximo de caixas
        public int WarningThreshold { get; set; } // Limite para aviso
        public bool AutomaticRelease { get; set; } // Liberação automática
        public int MaxWaitTime { get; set; } // Tempo máximo de espera (min)
        public string ValidCustomizations { get; set; } // Customizações válidas (JSON)
        public string SpecialInstructions { get; set; } // Instruções especiais
        public bool IsActive { get; set; } // Se configuração está ativa
        public DateTime UpdatedAt { get; set; } // Última atualização
        public int UpdatedByUserId { get; set; } // FK para User que atualizou

        public virtual Ramp Ramp { get; set; }
        public virtual User UpdatedByUser { get; set; }
    }
}
