namespace Adidas.DTOs.Operation.PaymentDTOs.PaymentMethod;

public class PaymentMethodStatDto
{
    public string PaymentMethod { get; set; }
    public int Count { get; set; }
    public decimal Amount { get; set; }
    public decimal Percentage { get; set; }
}