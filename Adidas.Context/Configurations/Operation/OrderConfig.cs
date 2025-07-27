
using Adidas.Models.Operation;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Adidas.Context.Configurations.Feature;

public class OrderConfig : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        BaseAuditableEntityConfig.Configure(builder);

        // fields
        // indexes
        builder.HasIndex(e => e.OrderNumber).IsUnique();
        builder.HasIndex(e => new { e.UserId, e.OrderDate });
        builder.HasIndex(e => e.OrderStatus);
        builder.HasIndex(e => e.OrderDate);
        
       builder.Property(e=>e.OrderNumber).IsRequired().HasMaxLength(100);
       builder.Property(e => e.OrderStatus).IsRequired();
       builder.Property(e => e.OrderDate).IsRequired();
       builder.Property(e => e.ShippingAddress).IsRequired();
       builder.Property(e => e.BillingAddress).IsRequired();
       
       builder.Property(e=>e.Currency).IsRequired().HasMaxLength(30);
       builder.Property(e => e.Subtotal).HasPrecision(18, 2).IsRequired();
       builder.Property(e => e.TotalAmount).HasPrecision(18, 2).IsRequired();
       builder.Property(e => e.TaxAmount).HasPrecision(18, 2);
       builder.Property(e => e.ShippingAmount).HasPrecision(18, 2);
       builder.Property(e => e.DiscountAmount).HasPrecision(18, 2);
       

        // foreign keys
        builder.Property(e => e.UserId).IsRequired();
        // relationships
        builder.HasOne(e => e.User)
            .WithMany(e => e.Orders)
            .HasForeignKey(e => e.UserId)
            .OnDelete(DeleteBehavior.Restrict);

    }
}