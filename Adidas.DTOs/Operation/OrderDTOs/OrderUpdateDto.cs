namespace Adidas.DTOs.Operation.OrderDTOs;

public class OrderUpdateDto
{
    public Guid Id { get; set; }
    
    public string? UserId { get; set; }
    public string? Currency { get; set; } = "EGP";

    public string? ShippingAddress { get; set; }
    public string? BillingAddress { get; set; }
    public string? Notes { get; set; }
}