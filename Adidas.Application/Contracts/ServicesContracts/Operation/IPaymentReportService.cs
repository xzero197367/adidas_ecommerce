using Adidas.DTOs.Operation.PaymentDTOs;
using Adidas.DTOs.Operation.PaymentDTOs.PaymentMethod;

namespace Adidas.Application.Contracts.ServicesContracts.Operation;

public interface IPaymentReportService
{
    Task<PaymentReportDto> GetPaymentReportAsync(DateTime startDate, DateTime endDate);
    Task<List<PaymentMethodStatDto>> GetPaymentMethodStatsAsync(DateTime startDate, DateTime endDate);
    Task<List<DailyPaymentStatDto>> GetDailyPaymentStatsAsync(DateTime startDate, DateTime endDate);
    Task<decimal> GetRevenueByPeriodAsync(DateTime startDate, DateTime endDate);
}