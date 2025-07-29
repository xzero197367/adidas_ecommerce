using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Adidas.Models.Feature;
using Adidas.Models.Operation;
using Adidas.Models.Separator;
using Models.Feature;
using Models.People;

namespace Adidas.Models.Main;

public class Product : BaseAuditableEntity
{
    // fields
    public required string Name { get; set; }
    public required string Description { get; set; }
    public required string ShortDescription { get; set; }
    public required decimal Price { get; set; }
    public decimal? SalePrice { get; set; }
    public required Gender GenderTarget { get; set; }
    
    public string MetaTitle { get; set; }

    public string MetaDescription { get; set; }
    public required string Sku { get; set; }

    public Dictionary<string, object> Specifications { get; set; }// JSON string for specs

    // calculated properties
    [NotMapped]
    public double AverageRating { get => Reviews.Any() ? Reviews.Average(r => r.Rating) : 0; }

    [NotMapped]
    public int ReviewCount { get => Reviews.Count; }

    // foreign keys
    public required Guid CategoryId { get; set; }
    public required Guid BrandId { get; set; }

    #region navigation properties single

    //[ForeignKey("CategoryId")]
    public Category Category { get; set; }
    
    //[ForeignKey("BrandId")]
    public Brand Brand { get; set; }

    #endregion
    
    
    // navigation properties many

    #region navigation properties many

    public virtual ICollection<ProductVariant> Variants { get; set; } = new List<ProductVariant>();
    public virtual ICollection<ProductImage> Images { get; set; } = new List<ProductImage>();
    public virtual ICollection<Review> Reviews { get; set; } = new List<Review>();
    public virtual ICollection<Wishlist> Wishlists { get; set; } = new List<Wishlist>();
    public virtual ICollection<ProductAttributeValue> AttributeValues { get; set; } = new List<ProductAttributeValue>();


    #endregion
  
    
}
