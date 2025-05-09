using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WCS_PG.Data.Models
{
    public class ClientCustomization
    {
        public int Id { get; set; } // PK, autoincrement
        public string ClientName { get; set; } // Nome do cliente
        public string CustomizationType { get; set; } // Tipo de customização
        public string CustomizationRule { get; set; } // Regra de customização (JSON)
        public bool IsActive { get; set; } // Se está ativo
        public string Description { get; set; } // Descrição da customização
        public DateTime CreatedAt { get; set; } // Data de criação
        public int CreatedByUserId { get; set; } // FK para User que criou
        public DateTime? UpdatedAt { get; set; } // Última atualização
        public int? UpdatedByUserId { get; set; } // FK para User que atualizou

        public virtual User CreatedByUser { get; set; }
        public virtual User UpdatedByUser { get; set; }
    }
}
