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
        [Required]
        [StringLength(50)]
        public required string PaymentStatus { get; set; }

        [StringLength(100)]
        public string? TransactionId { get; set; }

        public string? GatewayResponse { get; set; }
    }
}
