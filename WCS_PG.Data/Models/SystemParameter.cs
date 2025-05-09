using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WCS_PG.Data.Models
{
    public class SystemParameter
    {
        public int Id { get; set; } // PK, autoincrement
        public string Key { get; set; } // Chave do parâmetro, única
        public string Value { get; set; } // Valor do parâmetro
        public string Description { get; set; } // Descrição do parâmetro
        public string DataType { get; set; } // Tipo de dado (int, string, bool, etc)
        public bool IsEditable { get; set; } // Se pode ser editado via interface
        public string Category { get; set; } // Categoria do parâmetro
        public DateTime UpdatedAt { get; set; } // Última atualização
        public int? UpdatedByUserId { get; set; } // FK para User que atualizou

        public virtual User UpdatedByUser { get; set; }
    }
}
