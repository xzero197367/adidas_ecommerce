using Adidas.Models.Feature;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Models.Feature;

namespace Adidas.Context.Configurations.Feature;

public class ShoppingCartConfig: IEntityTypeConfiguration<ShoppingCart>
{
    public void Configure(EntityTypeBuilder<ShoppingCart> builder)
    {
        BaseAuditableEntityConfig.Configure(builder);
        
        // properties
        // indexes
        builder.HasIndex(e => new { e.UserId, e.VariantId }).IsUnique();
        builder.HasIndex(e => e.AddedAt);
        
        builder.Property(e => e.UserId).IsRequired();
        builder.Property(e => e.VariantId).IsRequired();
        builder.Property(e => e.Quantity).IsRequired();
        
        // relationships
        builder.HasOne(e => e.User)
            .WithMany(e => e.CartItems)
            .HasForeignKey(e => e.UserId)
            .OnDelete(DeleteBehavior.Restrict);
        
        builder.HasOne(e => e.Variant)
            .WithMany(e => e.CartItems)
            .HasForeignKey(e => e.VariantId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}