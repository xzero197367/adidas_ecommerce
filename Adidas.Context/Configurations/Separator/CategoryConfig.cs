using Adidas.Models.Separator;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Adidas.Context.Configurations.People;

public class CategoryConfig: IEntityTypeConfiguration<Category>
{
    public void Configure(EntityTypeBuilder<Category> builder)
    {
        BaseAuditableEntityConfig.Configure(builder);
        
        // fields
        // indexes
        builder.HasIndex(e => e.Slug).IsUnique();
        builder.HasIndex(e => new { e.ParentCategoryId, e.IsActive });
        builder.HasIndex(e => e.SortOrder);
        
        builder.Property(e => e.Name).IsRequired().HasMaxLength(100);
        builder.Property(e => e.Slug).IsRequired().HasMaxLength(100);
        builder.Property(e => e.ImageUrl).HasMaxLength(500);

        // foreign keys

        // navigations
        builder.HasOne(e => e.ParentCategory)
            .WithMany(p => p.SubCategories)
            .HasForeignKey(e => e.ParentCategoryId)
            .OnDelete(DeleteBehavior.Restrict);
        
        
        // Seed Categories
        //builder.HasData(
        //    new Category { Name = "Footwear", Slug = "footwear", IsActive = true, SortOrder = 1 },
        //    new Category { Name = "Clothing", Slug = "clothing", IsActive = true, SortOrder = 2 },
        //    new Category { Name = "Accessories", Slug = "accessories", IsActive = true, SortOrder = 3 },
        //    new Category { Name = "Running Shoes", Slug = "running-shoes",  IsActive = true, SortOrder = 1 },
        //    new Category { Name = "Lifestyle Shoes", Slug = "lifestyle-shoes",  IsActive = true, SortOrder = 2 },
        //    new Category { Name = "Football Boots", Slug = "football-boots",  IsActive = true, SortOrder = 3 }
        //);
    }
}