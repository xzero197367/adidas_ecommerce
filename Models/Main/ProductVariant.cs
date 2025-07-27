using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Adidas.Models.Feature;
using Adidas.Models.Operation;
using Adidas.Models.Tracker;

namespace Adidas.Models.Main;


public class ProductVariant : BaseAuditableEntity
{
    // fields
    public required string Sku { get; set; }
    public required string Size { get; set; }
    public required string Color { get; set; }
    public required int StockQuantity { get; set; }
    
    public decimal PriceAdjustment { get; set; }
    public string ImageUrl { get; set; }
    
    // foriegn keys
    public required Guid ProductId { get; set; }
    
    // navigation properties
    public Product Product { get; set; }

    
    // Relationships
    public virtual ICollection<ProductImage> Images { get; set; } = new List<ProductImage>();
    public virtual ICollection<ShoppingCart> CartItems { get; set; } = new List<ShoppingCart>();
    public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
    public virtual ICollection<InventoryLog> InventoryLogs { get; set; } = new List<InventoryLog>();
}
