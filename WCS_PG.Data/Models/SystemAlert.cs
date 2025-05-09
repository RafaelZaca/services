using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WCS_PG.Data.Models
{
    public class SystemAlert
    {
        public int Id { get; set; } // PK, autoincrement
        public string AlertType { get; set; } // Tipo de alerta
        public string Severity { get; set; } // Severidade (Info, Warning, Error)
        public string Message { get; set; } // Mensagem do alerta
        public string Parameters { get; set; } // Parâmetros do alerta (JSON)
        public bool IsActive { get; set; } // Se está ativo
        public bool RequiresAcknowledgment { get; set; } // Se requer confirmação
        public DateTime CreatedAt { get; set; } // Data de criação
        public DateTime? AcknowledgedAt { get; set; } // Data de confirmação
        public int? AcknowledgedByUserId { get; set; } // FK para User que confirmou

        public virtual User AcknowledgedByUser { get; set; }
    }
}
