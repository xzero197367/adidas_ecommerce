namespace Adidas.DTOs.Operation.OrderDTOs
{
    public class OrderLastUpdateDto
    {
        public Guid OrderId { get; set; }
        public string UpdatedBy { get; set; } = string.Empty;
        public DateTime UpdatedAt { get; set; }
    }
}
