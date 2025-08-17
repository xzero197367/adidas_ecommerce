using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Adidas.DTOs.Operation.PaymentDTOs.PaypalDtos
{
    public class PayPalPurchaseUnit
    {
        [JsonPropertyName("payments")]
        public PayPalPayments Payments { get; set; }
    }
}
