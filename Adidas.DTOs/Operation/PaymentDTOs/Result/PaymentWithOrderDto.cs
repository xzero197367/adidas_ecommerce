using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Adidas.DTOs.Operation.PaymentDTOs.Result
{
    public class PaymentWithOrderDto : PaymentDto
    {
        public OrderSummaryDto? Order { get; set; }
    }
}
