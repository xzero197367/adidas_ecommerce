using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Adidas.DTOs.Operation.PaymentDTOs.PaypalDtos
{
    public class PayPalCreatePaymentDto
    {
        [Required]
        public Guid OrderId { get; set; }

        [Required]
        [Range(0.01, double.MaxValue)]
        public decimal Amount { get; set; }

        [Required]
        [MaxLength(3)]
        public string Currency { get; set; } = "USD";

        [MaxLength(200)]
        public string? Description { get; set; }

        [Required]
        [Url]
        public string ReturnUrl { get; set; }

        [Required]
        [Url]
        public string CancelUrl { get; set; }
    }
}
