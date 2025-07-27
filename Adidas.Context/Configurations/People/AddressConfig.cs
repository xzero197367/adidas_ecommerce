using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Models.People;

namespace Adidas.Context.Configurations.People;

public class AddressConfig: IEntityTypeConfiguration<Address>
{
    public void Configure(EntityTypeBuilder<Address> builder)
    {
        BaseAuditableEntityConfig.Configure(builder);
        
        // fields
        // indexes
        builder.HasIndex(e => new { e.UserId, e.IsDefault });
        builder.HasIndex(e => new { e.Country, e.StateProvince, e.City });
        
        builder.Property(e => e.AddressType).IsRequired().HasMaxLength(50);
        builder.Property(e => e.StreetAddress).IsRequired().HasMaxLength(200);
        builder.Property(e => e.City).IsRequired().HasMaxLength(100);
        builder.Property(e => e.StateProvince).HasMaxLength(100);
        builder.Property(e => e.PostalCode).HasMaxLength(20);
        builder.Property(e => e.Country).HasMaxLength(100);
        
        // relation
        builder.HasOne(e => e.User)
        .WithMany(e => e.Addresses)
        .HasForeignKey(e => e.UserId)
        .OnDelete(DeleteBehavior.Restrict);
        
    }
}