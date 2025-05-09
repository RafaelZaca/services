using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WCS_PG.Data.Models
{
    public class PickRequestItem
    {
        public int Id { get; set; } // PK, autoincrement
        public string PickRequestId { get; set; } // FK para PickRequest
        public string Sku { get; set; } // Código do SKU
        public string Description { get; set; } // Descrição do SKU
        public int? ExpectedQuantity { get; set; } // Quantidade esperada
        public int? InducedQuantity { get; set; } // Quantidade induzida
        public int? ReceivedQuantity { get; set; } // Quantidade recebida
        public decimal? InductionPercentage { get; set; } // Percentual induzido
        public decimal? ReceiptPercentage { get; set; } // Percentual recebido
        public string BatchNumber { get; set; } // Número do lote
        public int? NoReadRejectionCount { get; set; } // Quantidade de rejeições no-read
        public int? FinalRejectionCount { get; set; } // Quantidade de rejeições finais
        public int? PendingCount { get; set; } // Quantidade pendente
        public string? PackageUnit { get; set; } //Unidade de contagem
        public string?  AssetType { get; set; }
        public string? SourcePallet { get; set; }
        public string WorkReference { get; set; }
        public DateTime? LastBoxReceivedAt { get; set; } // Data/hora da última caixa recebida

        public virtual PickRequest PickRequest { get; set; }
    }
}
