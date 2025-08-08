

namespace Adidas.DTOs.Operation.PaymentDTOs
{
    public class PaymentDto
    {
        public Guid Id { get; set; }
        public string PaymentMethod { get; set; }
        public string PaymentStatus { get; set; }
        public decimal Amount { get; set; }
        public string? TransactionId { get; set; }
        public string? GatewayResponse { get; set; }
        public DateTime ProcessedAt { get; set; }
        public Guid OrderId { get; set; }

        // From BaseAuditableEntity
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string? CreatedBy { get; set; }
        public string? UpdatedBy { get; set; }
        public bool IsActive { get; set; }
    }
}
