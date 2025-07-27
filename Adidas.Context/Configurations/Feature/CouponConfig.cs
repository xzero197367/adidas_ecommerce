using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Models.Feature;

namespace Adidas.Context.Configurations.Feature;

public class CouponConfig: IEntityTypeConfiguration<Coupon>
{
    public void Configure(EntityTypeBuilder<Coupon> builder)
    {
        BaseAuditableEntityConfig.Configure(builder);
        
        // properties
        builder.Property(e => e.Name).IsRequired().HasMaxLength(100);
        builder.Property(e => e.Code).IsRequired().HasMaxLength(50);
        builder.Property(e => e.DiscountType).IsRequired();
        builder.Property(e => e.DiscountValue).IsRequired();
        builder.Property(e => e.ValidFrom).IsRequired();
        builder.Property(e => e.ValidTo).IsRequired();
        builder.Property(e => e.UsageLimit).HasDefaultValue(0);
        
        
        
    }
}