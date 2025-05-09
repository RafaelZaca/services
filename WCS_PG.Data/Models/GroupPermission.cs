using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WCS_PG.Data.Models
{
    public class GroupPermission
    {
        public int GroupId { get; set; } // PK, FK para Groups
        public string PermissionId { get; set; } // PK, FK para Permissions
        public DateTime CreatedAt { get; set; } // Data de criação

        public virtual Group Group { get; set; } // Navegação EF
        public virtual Permission Permission { get; set; } // Navegação EF
    }
}
