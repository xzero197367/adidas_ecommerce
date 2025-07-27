using Adidas.Models.Feature;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;


namespace Adidas.Context.Configurations.Feature;

public class OrderCouponConfig: IEntityTypeConfiguration<OrderCoupon>
{
    public void Configure(EntityTypeBuilder<OrderCoupon> builder)
    {
        BaseAuditableEntityConfig.Configure(builder);
        
        builder.Property(e => e.OrderId).IsRequired();
        builder.Property(e => e.CouponId).IsRequired();
        builder.Property(e=>e.DiscountApplied).IsRequired();
        
        
        builder.HasOne(e => e.Order)
        .WithMany(e => e.OrderCoupons)
        .HasForeignKey(e => e.OrderId)
        .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.Coupon)
        .WithMany(e => e.OrderCoupons)
        .HasForeignKey(e => e.CouponId)
        .OnDelete(DeleteBehavior.Restrict);


    }
}