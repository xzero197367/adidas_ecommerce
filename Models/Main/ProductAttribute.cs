using System.ComponentModel.DataAnnotations;

namespace Adidas.Models.Main;

public class ProductAttribute
{
    [Key]
    public int AttributeId { get; set; }

    [Required, MaxLength(100)]
    public string Name { get; set; }

    [Required, MaxLength(50)]
    public string DataType { get; set; }

    [Required]
    public bool IsFilterable { get; set; }

    [Required]
    public bool IsRequired { get; set; }

    public int SortOrder { get; set; }

    // Relationships
    public virtual ICollection<ProductAttributeValue> AttributeValues { get; set; } = new List<ProductAttributeValue>();
}