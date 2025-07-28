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
        [StringLength(50)]
        public required string PaymentMethod { get; set; }

        [Required]
        [StringLength(50)]
        public required string PaymentStatus { get; set; }

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than 0")]
        public required decimal Amount { get; set; }

        [StringLength(100)]
        public string? TransactionId { get; set; }

        public string? GatewayResponse { get; set; }

        [Required]
        public required Guid OrderId { get; set; }

    }
}
