using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Adidas.Models.Main;
using Models.People;

namespace Adidas.Models.Tracker;

public class InventoryLog : BaseAuditableEntity
{
    // fields
    public required int QuantityChange { get; set; }
    public required int PreviousStock { get; set; }
    public required int NewStock { get; set; }
    public required string ChangeType { get; set; }
    public string Reason { get; set; }
    // foreign key
    [Required]
    public required Guid VariantId { get; set; }
    // navigation
    public ProductVariant Variant { get; set; }

    
  
}