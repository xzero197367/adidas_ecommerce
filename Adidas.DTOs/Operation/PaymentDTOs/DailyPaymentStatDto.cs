namespace Adidas.DTOs.Operation.PaymentDTOs;

public class DailyPaymentStatDto
{
    public DateTime Date { get; set; }
    public decimal Amount { get; set; }
    public int Count { get; set; }
}