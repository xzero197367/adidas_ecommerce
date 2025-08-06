

using Adidas.DTOs.Operation.PaymentDTOs;

namespace Adidas.Application.Contracts.ServicesContracts.Operation
{

    public interface IPaymentService : IGenericService<Payment, PaymentDto, PaymentCreateDto, PaymentUpdateDto>
    {
        // Payment-specific Query Operations
        // Task<IEnumerable<OperationResult<PaymentDto>>> GetPaymentsByOrderIdAsync(Guid orderId);
        // Task<IEnumerable<PaymentWithOrderDto>> GetPaymentsByOrderIdWithDetailsAsync(Guid orderId);
        // Task<IEnumerable<PaymentDto>> GetPaymentsByStatusAsync(string status);
        // Task<IEnumerable<PaymentDto>> GetPaymentsByMethodAsync(string method);
        // Task<PaymentDto?> GetPaymentByTransactionIdAsync(string transactionId);
        // Task<IEnumerable<PaymentDto>> GetFailedPaymentsAsync();
        //
        // // Payment-specific Pagination
        // Task<PagedPaymentDto> GetPaymentsPagedAsync(int pageNumber, int pageSize, string? status = null);
        // Task<PagedPaymentDto> GetPaymentsPagedWithFiltersAsync(int pageNumber, int pageSize, PaymentFilterDto filters);
        //
        // // Analytics & Reports
        // Task<decimal> GetTotalPaymentsAsync(DateTime? startDate = null, DateTime? endDate = null);
        // Task<PaymentStatsDto> GetPaymentStatsAsync(DateTime? startDate = null, DateTime? endDate = null);
        // Task<IEnumerable<PaymentDto>> GetPaymentsByDateRangeAsync(DateTime startDate, DateTime endDate);
        //
        // // Business Logic
        // Task<PaymentDto?> ProcessPaymentAsync(CreatePaymentDto paymentDto);
        // Task<PaymentDto?> RefundPaymentAsync(Guid paymentId, decimal? refundAmount = null);
        // Task<bool> ValidatePaymentAsync(Guid paymentId);
        // Task<IEnumerable<PaymentDto>> GetPendingPaymentsAsync();
        // Task<bool> MarkPaymentAsSuccessfulAsync(Guid paymentId, string transactionId);
        // Task<bool> MarkPaymentAsFailedAsync(Guid paymentId, string errorReason);
    }
}
