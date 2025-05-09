using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WCS_PG.Data.Models
{
    public class SkuBatch
    {
        public int Id { get; set; } // PK, autoincrement
        public string Sku { get; set; } // SKU do item
        public string BatchNumber { get; set; } // Número do lote
        public DateTime RegisteredAt { get; set; } // Data/hora do registro
        public int RegisteredByUserId { get; set; } // FK para User que registrou
        public bool IsActive { get; set; } // Se o lote está ativo
        public string WaveNumber { get; set; } // Número da wave em que foi usado
        public DateTime? FirstUsedAt { get; set; } // Primeira utilização
        public DateTime? LastUsedAt { get; set; } // Última utilização

        public virtual User RegisteredByUser { get; set; }
    }
}
