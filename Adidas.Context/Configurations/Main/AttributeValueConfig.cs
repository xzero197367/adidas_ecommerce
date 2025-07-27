
using Adidas.Models.Main;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;


namespace Adidas.Context.Configurations.Feature;

public class ProductAttributeValueConfig : IEntityTypeConfiguration<ProductAttributeValue>
{
    public void Configure(EntityTypeBuilder<ProductAttributeValue> builder)
    {
        BaseAuditableEntityConfig.Configure(builder);

        // properties
        builder.Property(e => e.AttributeId).IsRequired();
        builder.Property(e => e.ProductId).IsRequired();
        builder.Property(e => e.Value).IsRequired();

        // relationships
        builder.HasOne(e => e.Attribute)
            .WithMany(e => e.AttributeValues)
            .HasForeignKey(e => e.AttributeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.Product)
            .WithMany(e => e.AttributeValues)
            .HasForeignKey(e => e.ProductId)
            .OnDelete(DeleteBehavior.Restrict);
       
    }
}