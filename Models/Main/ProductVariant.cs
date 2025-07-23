using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Adidas.Models.Feature;
using Adidas.Models.Operation;
using Adidas.Models.Tracker;

namespace Adidas.Models.Main;

public class ProductVariant
{
    [Key]
    public int VariantId { get; set; }

    [Required]
    public int ProductId { get; set; }

    [ForeignKey("ProductId")]
    public Product Product { get; set; }

    [Required, MaxLength(50)]
    public string Sku { get; set; }

    [Required, MaxLength(50)]
    public string Size { get; set; }

    [Required, MaxLength(50)]
    public string Color { get; set; }

    [Required, Range(0, int.MaxValue)]
    public int StockQuantity { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal PriceAdjustment { get; set; }

    [MaxLength(500)]
    public string ImageUrl { get; set; }

    [Required]
    public bool IsActive { get; set; }

    // Relationships
    public virtual ICollection<ProductImage> Images { get; set; } = new List<ProductImage>();
    public virtual ICollection<ShoppingCart> CartItems { get; set; } = new List<ShoppingCart>();
    public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
    public virtual ICollection<InventoryLog> InventoryLogs { get; set; } = new List<InventoryLog>();
}
