using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Adidas.DTOs.Operation.PaymentDTOs.Update
{
    public class UpdatePaymentDto
    {
        [MaxLength(50)]
        public string? PaymentMethod { get; set; }

        [MaxLength(50)]
        public string? PaymentStatus { get; set; }

        [Range(0.01, double.MaxValue)]
        public decimal? Amount { get; set; }

        [MaxLength(100)]
        public string? TransactionId { get; set; }

        [MaxLength(500)]
        public string? GatewayResponse { get; set; }

        public DateTime? ProcessedAt { get; set; }

        public Guid? OrderId { get; set; }
    }
}
