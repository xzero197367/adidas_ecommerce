using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Adidas.DTOs.Operation.PaymentDTOs.PaypalDtos
{
    public class PayPalExecutePaymentDto
    {
        [Required]
        public string PaymentId { get; set; }

        [Required]
        public string PayerId { get; set; }
    }
}
