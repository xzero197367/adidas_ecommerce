using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Adidas.DTOs.Operation.OrderDTOs.Calculation
{
    public class AppliedCouponDto
    {
        public string CouponCode { get; set; }
        public decimal DiscountAmount { get; set; }
        public string DiscountType { get; set; }
        public bool IsValid { get; set; }
        public string ErrorMessage { get; set; }
    }
}
