using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Adidas.Models.Main;

namespace Adidas.Models.Operation;

public class OrderItem : BaseAuditableEntity
{
    // fields
    public required int Quantity { get; set; }
    public required decimal UnitPrice { get; set; }
    public required decimal TotalPrice { get; set; }
    public required  string ProductName { get; set; }
    public string VariantDetails { get; set; }
    
    // foreign keys
    public required Guid OrderId { get; set; }
    public required Guid VariantId { get; set; }
    
    // navigation properties
    public Order Order { get; set; }
    public ProductVariant Variant { get; set; }
}
