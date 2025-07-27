
using Adidas.Models.Main;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Adidas.Context.Configurations.Feature;

public class ProductConfig : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        BaseAuditableEntityConfig.Configure(builder);

        // properties
        // index
        builder.HasIndex(e => e.Sku).IsUnique();
        builder.HasIndex(e => new { e.CategoryId, e.IsActive });
        builder.HasIndex(e => new { e.BrandId, e.IsActive });
        builder.HasIndex(e => e.GenderTarget);
        builder.HasIndex(e => e.CreatedAt);
        
        
        builder.Property(e => e.Name).IsRequired().HasMaxLength(200);
        builder.Property(e => e.Description).IsRequired().HasMaxLength(5000);
        builder.Property(e => e.ShortDescription).IsRequired().HasMaxLength(1000);
        builder.Property(e => e.Sku).IsRequired().HasMaxLength(100);
        builder.Property(e => e.Price).HasPrecision(18, 2).IsRequired();
        builder.Property(e => e.SalePrice).HasPrecision(18, 2);
        builder.Property(e => e.GenderTarget).IsRequired();
        
  
        // calculated properties
        builder.Ignore(e => e.AverageRating);
        builder.Ignore(e => e.ReviewCount);
        
        // foreign keys
        builder.Property(e=>e.BrandId).IsRequired();
        builder.Property(e=>e.CategoryId).IsRequired();

        // relationships
        builder.HasOne(e => e.Brand)
            .WithMany(e => e.Products)
            .HasForeignKey(e => e.BrandId)
            .OnDelete(DeleteBehavior.Restrict);
        
        builder.HasOne(e => e.Category)
            .WithMany(e => e.Products)
            .HasForeignKey(e => e.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);
        
        
        
        
      


  


    }
}