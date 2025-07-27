
using Adidas.Models.Main;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Adidas.Context.Configurations.Feature;

public class ProductAttributeConfig : IEntityTypeConfiguration<ProductAttribute>
{
    public void Configure(EntityTypeBuilder<ProductAttribute> builder)
    {
        BaseAuditableEntityConfig.Configure(builder);

        // properties
        builder.Property(e => e.Name).IsRequired().HasMaxLength(100);
        builder.Property(e => e.DataType).IsRequired().HasMaxLength(50);
        builder.Property(e => e.IsFilterable).IsRequired();
        builder.Property(e => e.IsRequired).IsRequired();
        
        // foreign keys

        // relationships

    }
}