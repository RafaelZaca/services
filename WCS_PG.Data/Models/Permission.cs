using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WCS_PG.Data.Models
{
    public class Permission
    {
        public string Id { get; set; } // PK, código único (ex: "WAVE_CREATE")
        public string Name { get; set; } // Nome amigável
        public string Description { get; set; } // Descrição detalhada
        public string Resource { get; set; } // Recurso associado (ex: "Wave")

        public virtual ICollection<Group> Groups { get; set; } // Navegação EF
    }
}
