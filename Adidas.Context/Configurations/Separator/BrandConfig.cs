using Adidas.Models.Separator;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Adidas.Context.Configurations.People;

public class BrandConfig: IEntityTypeConfiguration<Brand>
{
    public void Configure(EntityTypeBuilder<Brand> builder)
    {
        BaseAuditableEntityConfig.Configure(builder);
        
        // fields
        builder.Property(e => e.Name).IsRequired().HasMaxLength(100);
        builder.Property(e => e.LogoUrl).HasMaxLength(500);
        builder.Property(e => e.Description).HasMaxLength(500);

        // foreign keys

        // navigations
        
        
        // Seed Brands
        builder.HasData(
            new Brand {  Name = "Adidas", Description = "Impossible is Nothing", IsActive = true },
            new Brand {  Name = "Adidas Originals", Description = "Original is Never Finished", IsActive = true },
            new Brand {  Name = "Adidas Performance", Description = "Nothing is Impossible", IsActive = true }
        );
    }
}