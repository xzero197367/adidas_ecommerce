namespace Adidas.DTOs.Operation.OrderDTOs;

public class OrderSummaryDto
{
    public Guid Id { get; set; }
    public string OrderNumber { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal ItemCount { get; set; }
    public string Status { get; set; }
    public DateTime OrderDate { get; set; }
}