using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Adidas.Models.Operation;


public class Payment : BaseAuditableEntity
{
    // fields
    public required string PaymentMethod { get; set; }
    public required string PaymentStatus { get; set; }

    public required decimal Amount { get; set; }

    [MaxLength(100)]
    public string TransactionId { get; set; }

    public string GatewayResponse { get; set; }

    public required DateTime ProcessedAt { get; set; } = DateTime.UtcNow;
    
    // foreign keys
    public required Guid OrderId { get; set; }
    // navigations
    public Order Order { get; set; }
    
}