using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Adidas.Models.Main;

namespace Adidas.Models.Operation;

public class OrderItem
{
    [Key]
    public int OrderItemId { get; set; }

    [Required]
    public int OrderId { get; set; }

    [ForeignKey("OrderId")]
    public Order Order { get; set; }

    [Required]
    public int VariantId { get; set; }

    [ForeignKey("VariantId")]
    public ProductVariant Variant { get; set; }

    [Required, Range(1, int.MaxValue)]
    public int Quantity { get; set; }

    [Required, Column(TypeName = "decimal(18,2)")]
    public decimal UnitPrice { get; set; }

    [Required, Column(TypeName = "decimal(18,2)")]
    public decimal TotalPrice { get; set; }

    [Required, MaxLength(200)]
    public string ProductName { get; set; }

    [MaxLength(500)]
    public string VariantDetails { get; set; }
}
