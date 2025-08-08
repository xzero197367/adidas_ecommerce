
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
        //builder.HasIndex(e => new { e.ProductId, e.IsApproved });
        //builder.HasIndex(e => new { e.UserId, e.CreatedAt });
        //builder.HasIndex(e => e.Rating);

        //builder.Property(e => e.ProductId).IsRequired();
        //builder.Property(e => e.IsVerifiedPurchase).IsRequired();
        //builder.Property(e => e.IsApproved).IsRequired();
        //builder.Property(e => e.Title).HasMaxLength(200);

        //// foreign keys
        //builder.Property(e => e.Rating).IsRequired();
        //builder.Property(e => e.UserId).IsRequired();

        //// relationships
        //builder.HasOne(e => e.Product)
        //    .WithMany(p => p.Reviews)
        //    .HasForeignKey(e => e.ProductId)
        //    .OnDelete(DeleteBehavior.Restrict);

        //builder.HasOne(e => e.User)
        //    .WithMany(u => u.Reviews)
        //    .HasForeignKey(e => e.UserId)
        //    .OnDelete(DeleteBehavior.Restrict);
        builder.ToTable("Reviews");

        builder.HasKey(r => r.Id);

        builder.Property(r => r.Rating)
            .IsRequired()
            .HasAnnotation("Range", new[] { 1, 5 });

        builder.Property(r => r.Title)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(r => r.ReviewText)
            .HasMaxLength(1000);

        builder.Property(r => r.IsVerifiedPurchase)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(r => r.IsApproved)
            .IsRequired()
            .HasDefaultValue(false);

        // Foreign Key Relationships
        builder.HasOne(r => r.Product)
            .WithMany(p => p.Reviews)
            .HasForeignKey(r => r.ProductId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(r => r.User)
            .WithMany(u => u.Reviews)
            .HasForeignKey(r => r.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes for better performance
        builder.HasIndex(r => r.ProductId);
        builder.HasIndex(r => r.UserId);
        builder.HasIndex(r => r.IsApproved);
        builder.HasIndex(r => r.CreatedAt);
    }
}