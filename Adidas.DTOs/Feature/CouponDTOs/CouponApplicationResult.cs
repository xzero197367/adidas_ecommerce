using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Adidas.DTOs.Feature.CouponDTOs
{
    public class CouponApplicationResult
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public decimal DiscountApplied { get; set; }
        public decimal NewTotal { get; set; }

        public static CouponApplicationResult Fail(string message) =>
            new() { Success = false, Message = message };
        public static CouponApplicationResult Ok() => new() { Success = true };
        public static CouponApplicationResult Ok(decimal discount, decimal newTotal) =>
            new() { Success = true, DiscountApplied = discount, NewTotal = newTotal };
    }

}
