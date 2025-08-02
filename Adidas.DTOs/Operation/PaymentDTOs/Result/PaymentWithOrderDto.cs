using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Adidas.DTOs.Operation.OrderDTOs.Result;

namespace Adidas.DTOs.Operation.PaymentDTOs.Result
{
    public class PaymentWithOrderDto
    {
        public Guid Id { get; set; }
        public string PaymentMethod { get; set; }
        public string PaymentStatus { get; set; }
        public decimal Amount { get; set; }
        public string? TransactionId { get; set; }
        public string? GatewayResponse { get; set; }
        public DateTime ProcessedAt { get; set; }
        public Guid OrderId { get; set; }

        // Order details
        public OrderDto? Order { get; set; }

        // From BaseAuditableEntity
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public bool IsActive { get; set; }
    }
}
