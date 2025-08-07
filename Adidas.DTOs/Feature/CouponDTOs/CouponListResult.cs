using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Adidas.DTOs.Feature.CouponDTOs
{
    public class CouponListResult
    {
        public IEnumerable<CouponDto> Coupons { get; set; } = new List<CouponDto>();
        public int TotalCount { get; set; }
        public int ActiveCount { get; set; }
        public int ExpiredCount { get; set; }
        public int TotalUsage { get; set; }
        public decimal TotalSavings { get; set; }
    }

}
