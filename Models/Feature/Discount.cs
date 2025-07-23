using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Adidas.Models.Feature;
using Adidas.Models.Operation;
using Adidas.Models;

namespace Models.Feature
{
    public class Discount : BaseEntity
    {
        [Required, MaxLength(50)]
        public string Code { get; set; }

        [Required, MaxLength(100)]
        public string Name { get; set; }

        [Required]
        public DiscountType DiscountType { get; set; }

        [Required, Column(TypeName = "decimal(18,2)")]
        public decimal DiscountValue { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal MinimumAmount { get; set; }

        [Required]
        public DateTime ValidFrom { get; set; }

        [Required]
        public DateTime ValidTo { get; set; }

        public int UsageLimit { get; set; }

        public int UsedCount { get; set; }

        // Relationships
        public virtual ICollection<OrderCoupon> OrderCoupons { get; set; } = new List<OrderCoupon>();
    }
}
