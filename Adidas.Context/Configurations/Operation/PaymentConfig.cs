
using Adidas.Models.Operation;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Adidas.Context.Configurations.Feature;

public class PaymentConfig : IEntityTypeConfiguration<Payment>
{
    public void Configure(EntityTypeBuilder<Payment> builder)
    {
        BaseAuditableEntityConfig.Configure(builder);

        // properties
        // indexes
        builder.HasIndex(e => e.OrderId);
        builder.HasIndex(e => e.TransactionId);
        builder.HasIndex(e => e.PaymentStatus);
        builder.HasIndex(e => e.ProcessedAt);
        
        builder.Property(e => e.PaymentMethod).IsRequired().HasMaxLength(100);
        builder.Property(e => e.PaymentStatus).IsRequired().HasMaxLength(100);
        builder.Property(e => e.Amount).HasPrecision(18, 2).IsRequired();
        builder.Property(e => e.ProcessedAt).IsRequired();

        // foreign keys
        builder.Property(e => e.OrderId).IsRequired();
        // relationships

        builder.HasOne(e => e.Order)
            .WithMany(o => o.Payments)
            .HasForeignKey(e => e.OrderId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}