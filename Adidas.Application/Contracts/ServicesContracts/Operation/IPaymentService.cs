using Adidas.DTOs.Operation.PaymentDTOs.Create;
using Adidas.DTOs.Operation.PaymentDTOs.Query;
using Adidas.DTOs.Operation.PaymentDTOs.Result;
using Adidas.DTOs.Operation.PaymentDTOs.Statistics;
using Adidas.DTOs.Operation.PaymentDTOs.Update;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Adidas.Application.Contracts.ServicesContracts.Operation
{
    public interface IPaymentService
    {
   
            // CRUD Operations
            Task<PaymentDto?> GetPaymentByIdAsync(Guid id);
            Task<PaymentDto> CreatePaymentAsync(CreatePaymentDto createPaymentDto);
            Task<PaymentDto?> UpdatePaymentAsync(Guid id, UpdatePaymentDto updatePaymentDto);
            Task<bool> DeletePaymentAsync(Guid id);

            // Query Operations
            Task<IEnumerable<PaymentDto>> GetPaymentsByOrderIdAsync(Guid orderId);
            Task<IEnumerable<PaymentWithOrderDto>> GetPaymentsByOrderIdWithDetailsAsync(Guid orderId);
            Task<IEnumerable<PaymentDto>> GetPaymentsByStatusAsync(string status);
            Task<IEnumerable<PaymentDto>> GetPaymentsByMethodAsync(string method);
            Task<PaymentDto?> GetPaymentByTransactionIdAsync(string transactionId);
            Task<IEnumerable<PaymentDto>> GetFailedPaymentsAsync();

            // Pagination
            Task<PagedPaymentDto> GetPaymentsPagedAsync(int pageNumber, int pageSize, string? status = null);
            Task<PagedPaymentDto> GetPaymentsPagedWithFiltersAsync(int pageNumber, int pageSize, PaymentFilterDto filters);

            // Analytics & Reports
            Task<decimal> GetTotalPaymentsAsync(DateTime? startDate = null, DateTime? endDate = null);
            Task<PaymentStatsDto> GetPaymentStatsAsync(DateTime? startDate = null, DateTime? endDate = null);
            Task<IEnumerable<PaymentDto>> GetPaymentsByDateRangeAsync(DateTime startDate, DateTime endDate);

            // Business Logic
            Task<PaymentDto?> ProcessPaymentAsync(CreatePaymentDto paymentDto);
            Task<PaymentDto?> RefundPaymentAsync(Guid paymentId, decimal? refundAmount = null);
            Task<bool> ValidatePaymentAsync(Guid paymentId);
            Task<IEnumerable<PaymentDto>> GetPendingPaymentsAsync();
            Task<bool> MarkPaymentAsSuccessfulAsync(Guid paymentId, string transactionId);
            Task<bool> MarkPaymentAsFailedAsync(Guid paymentId, string errorReason);
        
    }
}
