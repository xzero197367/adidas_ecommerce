using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Adidas.Models.Main;

public class ProductAttributeValue : BaseEntity
{
    [Required]
    public Guid ProductId { get; set; }

    [ForeignKey("ProductId")]
    public Product Product { get; set; }

    [Required]
    public Guid AttributeId { get; set; }

    [ForeignKey("AttributeId")]
    public ProductAttribute Attribute { get; set; }

    [Required]
    public string Value { get; set; }
}