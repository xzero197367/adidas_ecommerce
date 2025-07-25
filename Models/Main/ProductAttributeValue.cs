
namespace Adidas.Models.Main;

public class ProductAttributeValue : BaseAuditableEntity
{
    // field
    public required string Value { get; set; }
    // foreign key
    public required Guid ProductId { get; set; }
    public required Guid AttributeId { get; set; }
    
    // navigation
    public ProductAttribute Attribute { get; set; }
    public Product Product { get; set; }
}