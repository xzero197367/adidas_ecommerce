
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
        Amount
    }
    public class Coupon : BaseAuditableEntity
    {
        // fields
        public string Code { get; set; }
        public string Name { get; set; }
        public DiscountType DiscountType { get; set; }
        public decimal DiscountValue { get; set; }
        public decimal MinimumAmount { get; set; }
        public DateTime ValidFrom { get; set; }
        public DateTime ValidTo { get; set; }
        public int UsageLimit { get; set; }
        public int UsedCount { get; set; }

        // Relationships
        public virtual ICollection<OrderCoupon> OrderCoupons { get; set; } = new List<OrderCoupon>();
    }
}
