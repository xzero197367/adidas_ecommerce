using Adidas.DTOs.Operation.PaymentDTOs.PaymentMethod;

namespace Adidas.DTOs.Operation.PaymentDTOs;

public class PaymentReportDto
{
    public decimal TotalRevenue { get; set; }
    public int TotalTransactions { get; set; }
    public int SuccessfulTransactions { get; set; }
    public int FailedTransactions { get; set; }
    public int PendingTransactions { get; set; }
    public decimal SuccessRate { get; set; }
    public decimal AverageTransactionAmount { get; set; }
    public List<PaymentMethodStatDto> PaymentMethodStats { get; set; }
    public List<DailyPaymentStatDto> DailyStats { get; set; }
}