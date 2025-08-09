namespace Adidas.DTOs.Operation.PaymentDTOs.PaymentMethod;

public class PaymentMethodDto
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string Code { get; set; }
    public bool IsActive { get; set; }
    public string? Description { get; set; }
    public decimal? ProcessingFee { get; set; }
    public string? GatewayProvider { get; set; }
    public Dictionary<string, string>? Configuration { get; set; }
}