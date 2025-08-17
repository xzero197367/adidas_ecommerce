using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Adidas.DTOs.Feature.CouponDTOs
{
    public class ApplyCouponRequestDto
    {
        public Guid OrderId { get; set; }
        public string CouponCode { get; set; } = string.Empty;
    }
}
