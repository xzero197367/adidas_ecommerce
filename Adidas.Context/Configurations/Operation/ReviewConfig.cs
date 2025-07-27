
using Adidas.Models.Operation;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Adidas.Context.Configurations.Feature;

public class ReviewConfig : IEntityTypeConfiguration<Review>
{
    public void Configure(EntityTypeBuilder<Review> builder)
    {
        BaseAuditableEntityConfig.Configure(builder);

        // properties
        // indexes
        builder.HasIndex(e => new { e.ProductId, e.IsApproved });
        builder.HasIndex(e => new { e.UserId, e.CreatedAt });
        builder.HasIndex(e => e.Rating);
        
        builder.Property(e => e.ProductId).IsRequired();
        builder.Property(e => e.IsVerifiedPurchase).IsRequired();
        builder.Property(e => e.IsApproved).IsRequired();
        builder.Property(e => e.Title).HasMaxLength(200);

        // foreign keys
        builder.Property(e => e.Rating).IsRequired();
        builder.Property(e => e.UserId).IsRequired();
        
        // relationships
        builder.HasOne(e => e.Product)
            .WithMany(p => p.Reviews)
            .HasForeignKey(e => e.ProductId)
            .OnDelete(DeleteBehavior.Restrict);
        
        builder.HasOne(e => e.User)
            .WithMany(u => u.Reviews)
            .HasForeignKey(e => e.UserId)
            .OnDelete(DeleteBehavior.Restrict);
        
    }
}