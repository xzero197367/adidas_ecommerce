using Adidas.Models.Tracker;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Adidas.Context.Configurations.People;

public class InventoryLogConfig : IEntityTypeConfiguration<InventoryLog>
{
    public void Configure(EntityTypeBuilder<InventoryLog> builder)
    {
        BaseAuditableEntityConfig.Configure(builder);

        // fields
        builder.Property(e => e.QuantityChange).IsRequired();
        builder.Property(e => e.PreviousStock).IsRequired();
        builder.Property(e => e.NewStock).IsRequired();
        builder.Property(e => e.ChangeType).IsRequired().HasMaxLength(50);
        builder.Property(e => e.Reason).HasMaxLength(500);

        // foreign keys
        builder.Property(e => e.VariantId).IsRequired();
        // navigations

        builder.HasOne(e => e.Variant)
            .WithMany(v => v.InventoryLogs)
            .HasForeignKey(e => e.VariantId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}