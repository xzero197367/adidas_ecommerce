
using Adidas.Models.Main;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;


namespace Adidas.Context.Configurations.Feature;

public class ProductImageConfig : IEntityTypeConfiguration<ProductImage>
{
    public void Configure(EntityTypeBuilder<ProductImage> builder)
    {
        BaseAuditableEntityConfig.Configure(builder);

        // properties
        // indexes
        builder.HasIndex(e => new { e.ProductId, e.IsPrimary });
        builder.HasIndex(e => new { e.VariantId, e.SortOrder });
        
        builder.Property(e => e.ProductId).IsRequired();
        builder.Property(e => e.IsPrimary).IsRequired();
        builder.Property(e => e.ImageUrl).IsRequired().HasMaxLength(500);
        builder.Property(e => e.AltText).HasMaxLength(200);

        // relationships
        builder.HasOne(e => e.Product)
            .WithMany(e => e.Images)
            .HasForeignKey(e => e.ProductId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.Variant)
            .WithMany(e => e.Images)
            .HasForeignKey(e => e.VariantId)
            .OnDelete(DeleteBehavior.Restrict);
        
    }
}