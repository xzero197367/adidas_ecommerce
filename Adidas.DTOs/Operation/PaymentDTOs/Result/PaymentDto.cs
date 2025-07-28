using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Adidas.DTOs.Operation.PaymentDTOs.Result
{
    public class PaymentDto
    {
        public Guid Id { get; set; }
        public string PaymentMethod { get; set; }
        public string PaymentStatus { get; set; }
        public decimal Amount { get; set; }
        public string? TransactionId { get; set; }
        public string? GatewayResponse { get; set; }
        public DateTime ProcessedAt { get; set; }
        public Guid OrderId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? ModifiedAt { get; set; }
        public string? CreatedBy { get; set; }
        public string? ModifiedBy { get; set; }
    }
}
