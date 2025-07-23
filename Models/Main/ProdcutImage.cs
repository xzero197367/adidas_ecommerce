using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Adidas.Models.Main;

public class ProductImage
{
    [Key]
    public int ImageId { get; set; }

    [Required]
    public int ProductId { get; set; }

    [ForeignKey("ProductId")]
    public Product Product { get; set; }

    public int? VariantId { get; set; }

    [ForeignKey("VariantId")]
    public ProductVariant Variant { get; set; }

    [Required, MaxLength(500)]
    public string ImageUrl { get; set; }

    [MaxLength(200)]
    public string AltText { get; set; }

    public int SortOrder { get; set; }

    [Required]
    public bool IsPrimary { get; set; }
}