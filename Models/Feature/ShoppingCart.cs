using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Adidas.Models.Main;
using Models.People;

namespace Adidas.Models.Feature;

public class ShoppingCart : BaseAuditableEntity
{
    //fields
    public int Quantity { get; set; }
    public DateTime AddedAt { get; set; } = DateTime.UtcNow;
    
    // foreign keys
    public Guid UserId { get; set; }
    public Guid VariantId { get; set; }
    
    // navigation properties
    public User User { get; set; }
    public ProductVariant Variant { get; set; }


}