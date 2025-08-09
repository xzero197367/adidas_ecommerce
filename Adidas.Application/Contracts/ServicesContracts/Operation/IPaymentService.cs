

using Adidas.DTOs.CommonDTOs;
using Adidas.DTOs.Operation.PaymentDTOs;

namespace Adidas.Application.Contracts.ServicesContracts.Operation
{

    public interface IPaymentService 
    {
        // Payment-specific Query Operations
        // Task<OperationResult<IEnumerable<OperationResult<PaymentDto>>>> GetPaymentsByOrderIdAsync(Guid orderId);
        // Task<OperationResult<IEnumerable<PaymentWithOrderDto>>> GetPaymentsByOrderIdWithDetailsAsync(Guid orderId);
        // Task<OperationResult<IEnumerable<PaymentDto>>> GetPaymentsByStatusAsync(string status);
        // Task<OperationResult<IEnumerable<PaymentDto>>> GetPaymentsByMethodAsync(string method);
        // Task<OperationResult<PaymentDto>> GetPaymentByTransactionIdAsync(string transactionId);
        // Task<OperationResult<IEnumerable<PaymentDto>>> GetFailedPaymentsAsync();
        //
        // // Payment-specific Pagination
        // Task<OperationResult<PagedPaymentDto>> GetPaymentsPagedAsync(int pageNumber, int pageSize, string? status = null);
        // Task<OperationResult<PagedPaymentDto>> GetPaymentsPagedWithFiltersAsync(int pageNumber, int pageSize, PaymentFilterDto filters);
        //
        // // Analytics & Reports
        // Task<OperationResult<decimal>> GetTotalPaymentsAsync(DateTime? startDate = null, DateTime? endDate = null);
        // Task<OperationResult<PaymentStatsDto>> GetPaymentStatsAsync(DateTime? startDate = null, DateTime? endDate = null);
        // Task<OperationResult<IEnumerable<PaymentDto>>> GetPaymentsByDateRangeAsync(DateTime startDate, DateTime endDate);
        //
        // // Business Logic
        Task<OperationResult<PaymentDto>> GetPaymentByIdAsync(Guid id);
        Task<OperationResult<PaymentDto>> ProcessPaymentAsync(PaymentCreateDto paymentDto);
        Task<OperationResult<PaymentDto>> RefundPaymentAsync(Guid paymentId, decimal? refundAmount = null);
        // Task<OperationResult<bool>> ValidatePaymentAsync(Guid paymentId);
        // Task<OperationResult<IEnumerable<PaymentDto>>> GetPendingPaymentsAsync();
        // Task<OperationResult<bool>> MarkPaymentAsSuccessfulAsync(Guid paymentId, string transactionId);
        // Task<OperationResult<bool>> MarkPaymentAsFailedAsync(Guid paymentId, string errorReason);
    }
}
