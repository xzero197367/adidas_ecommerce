using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Adidas.Models.Feature;
using Adidas.Models.Operation;
using Adidas.Models.Separator;
using Models.People;

namespace Adidas.Models.Main;

public class Product : BaseEntity
{
    [Required, MaxLength(200)]
    public string Name { get; set; }

    [Required]
    public string Description { get; set; }

    [Required]
    public string ShortDescription { get; set; }

    [Required, Range(0.01, double.MaxValue)]
    [Column(TypeName = "decimal(18,2)")]
    public decimal Price { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal? SalePrice { get; set; }

    [Required]
    public Guid CategoryId { get; set; }

    [ForeignKey("CategoryId")]
    public Category Category { get; set; }

    [Required]
    public Guid BrandId { get; set; }

    [ForeignKey("BrandId")]
    public Brand Brand { get; set; }

    [Required]
    public Gender GenderTarget { get; set; }

    public string MetaTitle { get; set; }

    public string MetaDescription { get; set; }

    [Required, MaxLength(50)]
    public string Sku { get; set; }

    public string Specifications { get; set; } // JSON string for specs

    [NotMapped]
    public double AverageRating { get => Reviews.Any() ? Reviews.Average(r => r.Rating) : 0; }

    [NotMapped]
    public int ReviewCount { get => Reviews.Count; }

    // Relationships
    public virtual ICollection<ProductVariant> Variants { get; set; } = new List<ProductVariant>();
    public virtual ICollection<ProductImage> Images { get; set; } = new List<ProductImage>();
    public virtual ICollection<Review> Reviews { get; set; } = new List<Review>();
    public virtual ICollection<Wishlist> Wishlists { get; set; } = new List<Wishlist>();
    public virtual ICollection<AttributeValue> AttributeValues { get; set; } = new List<AttributeValue>();
}
