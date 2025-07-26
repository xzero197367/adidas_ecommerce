using Adidas.Application.Contracts.RepositoriesContracts.Operation;

using System.Data.Entity;
namespace Adidas.Infra.Operation
{
    public class PaymentRepository : GenericRepository<Payment>, IPaymentRepository
    {
        public PaymentRepository(AdidasDbContext context) : base(context) { }

        public async Task<IEnumerable<Payment>> GetPaymentsByOrderIdAsync(Guid orderId)
        {
            return await FindAsync(p => p.OrderId == orderId && !p.IsDeleted,
                                 p => p.Order);
        }

        public async Task<IEnumerable<Payment>> GetPaymentsByStatusAsync(string status)
        {
            return await FindAsync(p => p.PaymentStatus == status && !p.IsDeleted);
        }

        public async Task<IEnumerable<Payment>> GetPaymentsByMethodAsync(string method)
        {
            return await FindAsync(p => p.PaymentMethod == method && !p.IsDeleted);
        }

        public async Task<Payment?> GetPaymentByTransactionIdAsync(string transactionId)
        {
            return await FirstOrDefaultAsync(p => p.TransactionId == transactionId && !p.IsDeleted);
        }

        public async Task<decimal> GetTotalPaymentsAsync(DateTime? startDate = null, DateTime? endDate = null)
        {
            var query = GetQueryable(p => p.PaymentStatus == "Success" && !p.IsDeleted);

            if (startDate.HasValue)
                query = query.Where(p => p.ProcessedAt >= startDate.Value);

            if (endDate.HasValue)
                query = query.Where(p => p.ProcessedAt <= endDate.Value);

            return await query.SumAsync(p => p.Amount);
        }

        public async Task<IEnumerable<Payment>> GetFailedPaymentsAsync()
        {
            return await FindAsync(p => p.PaymentStatus == "Failed" && !p.IsDeleted);
        }

        public async Task<(IEnumerable<Payment> payments, int totalCount)> GetPaymentsPagedAsync(int pageNumber, int pageSize, string? status = null)
        {
            if (!string.IsNullOrEmpty(status))
            {
                return await GetPagedAsync(pageNumber, pageSize, p => p.PaymentStatus == status && !p.IsDeleted);
            }
            return await GetPagedAsync(pageNumber, pageSize, p => !p.IsDeleted);
        }
    }
}
