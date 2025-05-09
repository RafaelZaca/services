using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace WCS_PG.Data.Models
{
    public class User
    {
        public int Id { get; set; } // PK, autoincrement
        public string Username { get; set; } // Login do usuário, único
        public string Name { get; set; } // Nome completo
        public string Email { get; set; } // Email, único
        public string PasswordHash { get; set; } // Hash da senha
        public int GroupId { get; set; } // FK para Groups
        public bool IsActive { get; set; } // Status ativo/inativo
        public DateTime CreatedAt { get; set; } // Data de criação
        public DateTime? LastLogin { get; set; } // Último acesso

        public virtual Group Group { get; set; } // Navegação EF
    }


}
