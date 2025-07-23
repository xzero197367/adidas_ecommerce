using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Adidas.Models.Main;

namespace Adidas.Models.Separator;
public class Category : BaseEntity
{
    [Required, MaxLength(100)]
    public string Name { get; set; }

    [Required, MaxLength(100)]
    public string Slug { get; set; }

    public string Description { get; set; }

    [MaxLength(500)]
    public string ImageUrl { get; set; }

    public Guid? ParentCategoryId { get; set; }

    [ForeignKey("ParentCategoryId")]
    public Category ParentCategory { get; set; }

    public int SortOrder { get; set; }

    // Relationships
    public virtual ICollection<Category> SubCategories { get; set; } = new List<Category>();
    public virtual ICollection<Product> Products { get; set; } = new List<Product>();
}