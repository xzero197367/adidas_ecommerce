using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Adidas.Models.Main;

public class ProductImage : BaseAuditableEntity
{
    // fields
    public required string ImageUrl { get; set; }
    public string AltText { get; set; } = string.Empty;
    public int SortOrder { get; set; }
    public required bool IsPrimary { get; set; }
    // foreign keys
    public required Guid ProductId { get; set; }
    public Guid? VariantId { get; set; }
    
    // navigation properties
    public Product? Product { get; set; }
    public ProductVariant? Variant { get; set; }


}