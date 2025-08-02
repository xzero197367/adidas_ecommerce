using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Adidas.DTOs.Operation.PaymentDTOs.Create
{
    public class CreatePaymentDto
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
