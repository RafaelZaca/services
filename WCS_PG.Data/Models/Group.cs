using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading.Tasks;

namespace WCS_PG.Data.Models
{
    public class Group
    {
        public int Id { get; set; } // PK, autoincrement
        public string Name { get; set; } // Nome do grupo, único
        public string Description { get; set; } // Descrição
        public bool IsActive { get; set; } // Status ativo/inativo
        public DateTime CreatedAt { get; set; } // Data de criação

        public virtual ICollection<User> Users { get; set; } // Navegação EF
        public virtual ICollection<Permission> Permissions { get; set; } // Navegação EF
    }
}
