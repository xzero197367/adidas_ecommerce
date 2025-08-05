using System.ComponentModel.DataAnnotations;

namespace Adidas.Models.Main;

public class ProductAttribute : BaseAuditableEntity
{
    // fields
    public required string Name { get; set; }
    public required string DataType { get; set; }
    public required bool IsFilterable { get; set; }
    public required bool IsRequired { get; set; }
    public int SortOrder { get; set; }
    
    // foreign keys

    // Relationships many
    public virtual ICollection<ProductAttributeValue> AttributeValues { get; set; } = new List<ProductAttributeValue>();
}