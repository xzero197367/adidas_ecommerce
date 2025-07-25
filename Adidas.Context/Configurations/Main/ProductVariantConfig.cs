
using Adidas.Models.Main;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Adidas.Context.Configurations.Feature;

public class ProductVariantConfig : IEntityTypeConfiguration<ProductVariant>
{
    public void Configure(EntityTypeBuilder<ProductVariant> builder)
    {
        BaseAuditableEntityConfig.Configure(builder);

        // properties
        // indexes
        builder.HasIndex(e => e.Sku).IsUnique();
        builder.HasIndex(e => new { e.ProductId, e.Size, e.Color }).IsUnique();
        builder.HasIndex(e => new { e.ProductId, e.IsActive });
        builder.HasIndex(e => e.StockQuantity);
        
        builder.Property(e => e.Sku).IsRequired().HasMaxLength(100);
        builder.Property(e => e.Size).IsRequired().HasMaxLength(50);
        builder.Property(e => e.Color).IsRequired().HasMaxLength(50);
        builder.Property(e => e.StockQuantity).IsRequired();
        builder.Property(e => e.ImageUrl).HasMaxLength(500);
        builder.Property(e => e.PriceAdjustment).HasPrecision(18, 2);

        // foreign keys
        builder.Property(e => e.ProductId).IsRequired();

        // relationships
        builder.HasOne(e => e.Product)
            .WithMany(e => e.Variants)
            .HasForeignKey(e => e.ProductId)
            .OnDelete(DeleteBehavior.Restrict);
        
    }
}