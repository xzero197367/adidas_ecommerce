using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Adidas.Models.Main;
using Models.People;

namespace Adidas.Models.Feature;

public class ShoppingCart : BaseEntity
{
    [Required]
    public Guid UserId { get; set; }

    [ForeignKey("UserId")]
    public User User { get; set; }

    [Required]
    public Guid VariantId { get; set; }

    [ForeignKey("VariantId")]
    public ProductVariant Variant { get; set; }

    [Required, Range(1, int.MaxValue)]
    public int Quantity { get; set; }

    [Required]
    public DateTime AddedAt { get; set; } = DateTime.UtcNow;
}