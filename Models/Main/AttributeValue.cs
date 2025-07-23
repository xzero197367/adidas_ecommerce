using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Adidas.Models.Main;

public class ProductAttributeValue
{
    [Key]
    public int ValueId { get; set; }

    [Required]
    public int ProductId { get; set; }

    [ForeignKey("ProductId")]
    public Product Product { get; set; }

    [Required]
    public int AttributeId { get; set; }

    [ForeignKey("AttributeId")]
    public ProductAttribute Attribute { get; set; }

    [Required]
    public string Value { get; set; }
}