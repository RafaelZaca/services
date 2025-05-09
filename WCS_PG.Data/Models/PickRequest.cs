using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WCS_PG.Data.Models
{
    public class PickRequest
    {
        public string Id { get; set; } // PK, gerado pelo WMS
        public string OrderNumber { get; set; } // Número do embarque
        public string StopId { get; set; } // Identificador do stop
        public string CarMoveId { get; set; } // Identificador do CAR_MOVE_ID
        public string DeliveryType { get; set; } // Tipo de entrega (Multiplos, etc)
        public string ClientName { get; set; } // Nome do cliente
        public string Customization { get; set; } // Particularidade (Lotação paletizado, etc)
        public int TotalQuantity { get; set; } // Quantidade total de caixas
        public int TotalSkus { get; set; } // Quantidade de SKUs distintos
        public string Status { get; set; } // Status (Pendente, Em Execução, etc)
        public DateTime CreatedAt { get; set; } // Data de criação
        public DateTime? StartedAt { get; set; } // Data de início
        public DateTime? CompletedAt { get; set; } // Data de conclusão

        public virtual ICollection<WavePickRequest> WavePickRequests { get; set; }
        public virtual ICollection<PickRequestItem> Items { get; set; }
        public virtual ICollection<RampAllocation> RampAllocations { get; set; }
    }
}
