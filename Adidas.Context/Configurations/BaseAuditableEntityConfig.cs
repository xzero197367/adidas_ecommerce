

using Adidas.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Adidas.Context.Configurations
{
    public static class BaseAuditableEntityConfig
    {
        public static void Configure<TEntity>(EntityTypeBuilder<TEntity> builder) where TEntity : BaseAuditableEntity
        {
            builder.HasKey(x => x.Id);

            builder.HasQueryFilter(x=> !x.IsDeleted);

            builder.Property(e => e.IsDeleted)
            .HasDefaultValue(false);

            builder.Property(e => e.CreatedAt)
               .HasDefaultValueSql("GETDATE()");

            builder.Property(e => e.UpdatedAt)
          .HasDefaultValueSql("GETDATE()");

            builder.HasOne(e => e.AddedBy)
            .WithMany()
            .HasForeignKey(e => e.AddedById)
            .OnDelete(DeleteBehavior.SetNull);

        }
    }
}
