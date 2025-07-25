
using Adidas.Models.Operation;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Adidas.Context.Configurations.Feature;

public class OrderItemConfig : IEntityTypeConfiguration<OrderItem>
{
    public void Configure(EntityTypeBuilder<OrderItem> builder)
    {
        BaseAuditableEntityConfig.Configure(builder);

        // properties
        // indexes
        builder.HasIndex(e => e.OrderId);
        builder.HasIndex(e => e.VariantId);
        
        builder.Property(e => e.Quantity).IsRequired();
        builder.Property(e => e.UnitPrice).HasPrecision(18, 2).IsRequired();
        builder.Property(e => e.TotalPrice).HasPrecision(18, 2).IsRequired();
        builder.Property(e => e.ProductName).IsRequired();
        builder.Property(e => e.VariantDetails).HasMaxLength(1000);

        // foreign keys
        builder.Property(e => e.OrderId).IsRequired();
        builder.Property(e => e.VariantId).IsRequired();
        // relationships
        builder.HasOne(e => e.Order)
            .WithMany(e => e.OrderItems)
            .HasForeignKey(e => e.OrderId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.Variant)
            .WithMany(e => e.OrderItems)
            .HasForeignKey(e => e.VariantId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}