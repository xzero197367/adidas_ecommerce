using Adidas.Models.Main;

namespace Adidas.Models.Separator;

public enum CategoryType
{
    Men,
    Women,
    Kids,
    Sports,
}

public class Category : BaseAuditableEntity
{
    // field 
    public bool IsParent { get; set; } = false;
    public required string Name { get; set; }
    public required string Slug { get; set; }
    public string Description { get; set; }
    public string? ImageUrl { get; set; }
    public int SortOrder { get; set; }
    public CategoryType? Type { get; set; }

    // foreign key
    public Guid? ParentCategoryId { get; set; }

    // navigation
    public Category ParentCategory { get; set; }

    // Relationships
    public virtual ICollection<Category> SubCategories { get; set; } = new List<Category>();
    public virtual ICollection<Product> Products { get; set; } = new List<Product>();
}