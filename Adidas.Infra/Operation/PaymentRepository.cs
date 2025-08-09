using System.Data.Entity;
using Adidas.DTOs.Common_DTOs;

namespace Adidas.Infra.Operation
{
    public class PaymentRepository : GenericRepository<Payment>, IPaymentRepository
    {
        public PaymentRepository(AdidasDbContext context) : base(context) { }

        public async Task<IEnumerable<Payment>> GetPaymentsByOrderIdAsync(Guid orderId)
        {
            return await GetAll(q=>q.Where(p => p.OrderId == orderId && !p.IsDeleted)).ToListAsync();
        }

        public async Task<IEnumerable<Payment>> GetPaymentsByStatusAsync(string status)
        {
            return await GetAll(q => q.Where(p => p.PaymentStatus == status && !p.IsDeleted)).ToListAsync();
        }

        public async Task<IEnumerable<Payment>> GetPaymentsByMethodAsync(string method)
        {
            return await GetAll(q => q.Where(p => p.PaymentMethod == method && !p.IsDeleted)).ToListAsync();
        }

        public async Task<Payment?> GetPaymentByTransactionIdAsync(string transactionId)
        {
            return await FindAsync(q=>q.Where(p => p.TransactionId == transactionId && !p.IsDeleted));
        }

        public async Task<decimal> GetTotalPaymentsAsync(DateTime? startDate = null, DateTime? endDate = null)
        {
            var query = GetAll(q=>q.Where(p => p.PaymentStatus == "Success" && !p.IsDeleted));

            if (startDate.HasValue)
                query = query.Where(p => p.ProcessedAt >= startDate.Value);

            if (endDate.HasValue)
                query = query.Where(p => p.ProcessedAt <= endDate.Value);  

            return await query.SumAsync(p => p.Amount);
        }

        public async Task<IEnumerable<Payment>> GetFailedPaymentsAsync()
        {
            return await GetAll(q => q.Where(p => p.PaymentStatus == "Failed" && !p.IsDeleted)).ToListAsync();
        }

        public async Task<PagedResultDto<Payment>> GetPaymentsPagedAsync(int pageNumber, int pageSize, string? status = null)
        {
            if (!string.IsNullOrEmpty(status))
            {
                var payments1 = await GetPagedAsync(pageNumber, pageSize, q=>q.Where(p => p.PaymentStatus == status && !p.IsDeleted));
                return payments1;
            }
            var payments = await GetPagedAsync(pageNumber, pageSize, q => q.Where(p => !p.IsDeleted));
            
            return payments;
        }
    }
}
