using Adidas.Application.Contracts.RepositoriesContracts.Operation;
using Adidas.Application.Contracts.ServicesContracts.Operation;
using Adidas.DTOs.Operation.PaymentDTOs;
using Adidas.DTOs.Operation.PaymentDTOs.PaymentMethod;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Adidas.Application.Services.Operation;

public class PaymentReportService : IPaymentReportService
{
    private readonly IPaymentRepository _paymentRepository;
    private readonly ILogger<PaymentReportService> _logger;

    public PaymentReportService(IPaymentRepository paymentRepository, ILogger<PaymentReportService> logger)
    {
        _paymentRepository = paymentRepository;
        _logger = logger;
    }

    public async Task<PaymentReportDto> GetPaymentReportAsync(DateTime startDate, DateTime endDate)
    {
        try
        {
            var payments = await _paymentRepository.GetAll().Where(p => 
                p.ProcessedAt >= startDate && 
                p.ProcessedAt <= endDate && 
                !p.IsDeleted).ToListAsync();

            var totalRevenue = payments.Where(p => p.PaymentStatus == "Completed").Sum(p => p.Amount);
            var totalTransactions = payments.Count;
            var successfulTransactions = payments.Count(p => p.PaymentStatus == "Completed");
            var failedTransactions = payments.Count(p => p.PaymentStatus == "Failed");
            var pendingTransactions = payments.Count(p => p.PaymentStatus == "Pending");

            var paymentMethodStats = await GetPaymentMethodStatsAsync(startDate, endDate);
            var dailyStats = await GetDailyPaymentStatsAsync(startDate, endDate);

            return new PaymentReportDto
            {
                TotalRevenue = totalRevenue,
                TotalTransactions = totalTransactions,
                SuccessfulTransactions = successfulTransactions,
                FailedTransactions = failedTransactions,
                PendingTransactions = pendingTransactions,
                SuccessRate = totalTransactions > 0 ? (decimal)successfulTransactions / totalTransactions * 100 : 0,
                AverageTransactionAmount = totalTransactions > 0 ? totalRevenue / totalTransactions : 0,
                PaymentMethodStats = paymentMethodStats,
                DailyStats = dailyStats
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating payment report");
            return null;
        }
    }

    public async Task<List<PaymentMethodStatDto>> GetPaymentMethodStatsAsync(DateTime startDate, DateTime endDate)
    {
        var payments = await _paymentRepository.GetAll().Where(p => 
            p.ProcessedAt >= startDate && 
            p.ProcessedAt <= endDate && 
            p.PaymentStatus == "Completed" &&
            !p.IsDeleted).ToListAsync();

        var totalAmount = payments.Sum(p => p.Amount);

        return payments
            .GroupBy(p => p.PaymentMethod)
            .Select(g => new PaymentMethodStatDto
            {
                PaymentMethod = g.Key,
                Count = g.Count(),
                Amount = g.Sum(p => p.Amount),
                Percentage = totalAmount > 0 ? g.Sum(p => p.Amount) / totalAmount * 100 : 0
            })
            .OrderByDescending(s => s.Amount)
            .ToList();
    }

    public async Task<List<DailyPaymentStatDto>> GetDailyPaymentStatsAsync(DateTime startDate, DateTime endDate)
    {
        var payments = await _paymentRepository.GetAll().Where(p => 
            p.ProcessedAt >= startDate && 
            p.ProcessedAt <= endDate && 
            p.PaymentStatus == "Completed" &&
            !p.IsDeleted).ToListAsync();

        return payments
            .GroupBy(p => p.ProcessedAt.Date)
            .Select(g => new DailyPaymentStatDto
            {
                Date = g.Key,
                Amount = g.Sum(p => p.Amount),
                Count = g.Count()
            })
            .OrderBy(s => s.Date)
            .ToList();
    }

    public async Task<decimal> GetRevenueByPeriodAsync(DateTime startDate, DateTime endDate)
    {
        return await _paymentRepository.GetTotalPaymentsAsync(startDate, endDate);
    }
}