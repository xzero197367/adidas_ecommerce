using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Adidas.Models.Main;
using Models.People;

namespace Adidas.Models.Tracker;

public class InventoryLog
{
    [Key]
    public int LogId { get; set; }

    [Required]
    public int VariantId { get; set; }

    [ForeignKey("VariantId")]
    public ProductVariant Variant { get; set; }

    [Required]
    public int QuantityChange { get; set; }

    [Required]
    public int PreviousStock { get; set; }

    [Required]
    public int NewStock { get; set; }

    [Required, MaxLength(50)]
    public string ChangeType { get; set; }

    [MaxLength(500)]
    public string Reason { get; set; }

    [Required]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public int? CreatedByUserId { get; set; }

    [ForeignKey("CreatedByUserId")]
    public User CreatedBy { get; set; }
}