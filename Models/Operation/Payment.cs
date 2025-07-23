using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Adidas.Models.Operation;


public class Payment : BaseEntity
{
    [Required]
    public Guid OrderId { get; set; }

    [ForeignKey("OrderId")]
    public Order Order { get; set; }

    [Required, MaxLength(50)]
    public string PaymentMethod { get; set; }

    [Required, MaxLength(50)]
    public string PaymentStatus { get; set; }

    [Required, Column(TypeName = "decimal(18,2)")]
    public decimal Amount { get; set; }

    [MaxLength(100)]
    public string TransactionId { get; set; }

    public string GatewayResponse { get; set; }

    [Required]
    public DateTime ProcessedAt { get; set; } = DateTime.UtcNow;
}