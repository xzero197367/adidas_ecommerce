using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Adidas.DTOs.Operation.OrderDTOs
{
    public class CreateOrderDTO
    {
        public string UserId { get; set; }
        [Required] public string ShippingAddress { get; set; } // JSON string

        [Required] public string BillingAddress { get; set; } // JSON string
        [Required] public string Currency { get; set; } // JSON string
        

    }
}
