using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Adidas.Models.Main;
using Models.People;

namespace Adidas.Models.Feature;

public class ShoppingCart : BaseAuditableEntity
{
    //fields
    public required int Quantity { get; set; }
    public DateTime AddedAt { get; set; } = DateTime.UtcNow;
    
    // foreign keys
    public required string UserId { get; set; }
    public required Guid VariantId { get; set; }
    
    // navigation properties
    public User User { get; set; }
    public ProductVariant Variant { get; set; }


}