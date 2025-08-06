
using System.ComponentModel.DataAnnotations;

namespace Adidas.DTOs.Operation.PaymentDTOs
{
    public class PaymentUpdateDto
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
