namespace Adidas.DTOs.Operation.OrderDTOs
{
    public class OrderWithCreatorDto : OrderDto
    {
        public string? AddedById { get; set; }
        public string? AddedByName { get; set; }
        public string? AddedByEmail { get; set; }
    }
}