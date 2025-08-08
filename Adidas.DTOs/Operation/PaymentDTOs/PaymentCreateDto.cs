
using System.ComponentModel.DataAnnotations;


namespace Adidas.DTOs.Operation.PaymentDTOs
{
    public class PaymentCreateDto
    {
        [Required]
        [MaxLength(50)]
        public required string PaymentMethod { get; set; }

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than 0")]
        public required decimal Amount { get; set; }

        [Required]
        public required Guid OrderId { get; set; }

        [MaxLength(500)]
        public string? GatewayResponse { get; set; }

        // PaymentStatus will be set to "Pending" by default in the service
        // ProcessedAt will be set to DateTime.UtcNow by default in the service
    }
}
