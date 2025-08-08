using Adidas.DTOs.Feature.OrderCouponDTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Adidas.DTOs.Feature.CouponDTOs
{
    public class CouponDetailsDTO
    {
        public CouponDto CouponDto { get; set; }

        public IEnumerable<OrderCouponDto> orderCouponDtos { get; set; }
        public int TotalUsage { get; set; }
        public decimal TotalSavings { get; set; }
    }
}
