using Adidas.Models.Operation;
using System;

namespace Adidas.Application.Contracts.RepositoriesContracts.Operation
{
    public interface IPaymentRepository : IGenericRepository<Payment>
    {
        Task<IEnumerable<Payment>> GetPaymentsByOrderIdAsync(Guid orderId);
        Task<IEnumerable<Payment>> GetPaymentsByStatusAsync(string status);
        Task<IEnumerable<Payment>> GetPaymentsByMethodAsync(string method);
        Task<Payment?> GetPaymentByTransactionIdAsync(string transactionId);
        Task<decimal> GetTotalPaymentsAsync(DateTime? startDate = null, DateTime? endDate = null);
        Task<IEnumerable<Payment>> GetFailedPaymentsAsync();
        Task<(IEnumerable<Payment> payments, int totalCount)> GetPaymentsPagedAsync(int pageNumber, int pageSize, string? status = null);
    }
}
 