using Adidas.Models.Feature;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Models.Feature;

namespace Adidas.Context.Configurations.Feature;

public class WishlistConfig : IEntityTypeConfiguration<Wishlist>
{
    public void Configure(EntityTypeBuilder<Wishlist> builder)
    {
        BaseAuditableEntityConfig.Configure(builder);

        // properties
        // indexes
        builder.HasIndex(e => new { e.UserId, e.ProductId }).IsUnique();
        builder.HasIndex(e => e.AddedAt);
        
        builder.Property(e => e.UserId).IsRequired();
        builder.Property(e => e.ProductId).IsRequired();
        builder.Property(e => e.AddedAt).IsRequired();

        // relationships
        builder.HasOne(e => e.User)
            .WithMany(e => e.Wishlists)
            .HasForeignKey(e => e.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.Product)
            .WithMany(e => e.Wishlists)
            .HasForeignKey(e => e.ProductId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}