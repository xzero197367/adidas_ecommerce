
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using Adidas.Models.Feature;
using Adidas.Models.Operation;
using Adidas.Models;

namespace Models.Feature
{
    public enum DiscountType
    {
        Percentage,
        Amount,
        FixedAmount
    }
    public class Coupon : BaseAuditableEntity
    {
        // fields
        public required string Code { get; set; }
        public required string Name { get; set; }
        public required DiscountType DiscountType { get; set; }
        public required decimal DiscountValue { get; set; }
        public decimal MinimumAmount { get; set; }
        public required DateTime ValidFrom { get; set; }
        public required DateTime ValidTo { get; set; }
        public int UsageLimit { get; set; }
        public int UsedCount { get; set; }

        // Relationships
        public virtual ICollection<OrderCoupon> OrderCoupons { get; set; } = new List<OrderCoupon>();
    }
}
