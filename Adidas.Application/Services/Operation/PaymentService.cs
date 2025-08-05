using Adidas.Application.Contracts.RepositoriesContracts;
using Adidas.Application.Contracts.RepositoriesContracts.Operation;
using Adidas.Application.Contracts.ServicesContracts.Operation;
using Adidas.Application.Services;
using Adidas.DTOs.Common_DTOs;
using Adidas.DTOs.Operation.PaymentDTOs.Create;
using Adidas.DTOs.Operation.PaymentDTOs.Query;
using Adidas.DTOs.Operation.PaymentDTOs.Result;
using Adidas.DTOs.Operation.PaymentDTOs.Statistics;
using Adidas.DTOs.Operation.PaymentDTOs.Update;
using AutoMapper;
using Microsoft.Extensions.Logging;
using System.Linq.Expressions;

public class PaymentService : GenericService<Payment, PaymentDto, CreatePaymentDto, UpdatePaymentDto> ,IPaymentService
{
    private readonly IPaymentRepository _paymentRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<PaymentService> _logger;

    public PaymentService(
        IPaymentRepository paymentRepository,
        IMapper mapper,
        ILogger<PaymentService> logger):base(paymentRepository, mapper, logger)
    {
        _paymentRepository = paymentRepository;
        _mapper = mapper;
        _logger = logger;
    }

    

    #region Payment-Specific Methods

    // Legacy method that now calls the generic method
    public async Task<OperationResult<PaymentDto>> GetPaymentByIdAsync(Guid id)
    {
        var payment = await GetByIdAsync(id);
        return payment;
    }

    // Legacy method that now calls the generic method
    public async Task<OperationResult<PaymentDto>> CreatePaymentAsync(CreatePaymentDto createPaymentDto)
    {
        return await CreateAsync(createPaymentDto);
    }

    // Legacy method that now calls the generic method
    // public async Task<OperationResult<PaymentDto?> UpdatePaymentAsync(Guid id, UpdatePaymentDto updatePaymentDto)
    // {
    //     try
    //     {
    //         return await UpdateAsync(id, updatePaymentDto);
    //     }
    //     catch (NotFoundException)
    //     {
    //         return null;
    //     }
    // }

    // Legacy method that now calls the generic method
    // public async Task<OperationResult<bool> DeletePaymentAsync(Guid id)
    // {
    //     return await DeleteAsync(id);
    // }
    //
    // public async Task<IEnumerable<PaymentDto>> GetPaymentsByOrderIdAsync(Guid orderId)
    // {
    //     var payments = await _paymentRepository.FindAsync(p => p.OrderId == orderId);
    //     return _mapper.Map<IEnumerable<PaymentDto>>(payments);
    // }
    //
    // public async Task<OperationResult<IEnumerable<PaymentWithOrderDto>> GetPaymentsByOrderIdWithDetailsAsync(Guid orderId)
    // {
    //     var payments = await _paymentRepository.FindAsync(
    //         p => p.OrderId == orderId,
    //         p => p.Order); // Include Order navigation property
    //     return _mapper.Map<IEnumerable<PaymentWithOrderDto>>(payments);
    // }
    //
    // public async Task<OperationResult<IEnumerable<PaymentDto>> GetPaymentsByStatusAsync(string status)
    // {
    //     var payments = await _paymentRepository.FindAsync(p => p.PaymentStatus == status);
    //     return _mapper.Map<IEnumerable<PaymentDto>>(payments);
    // }
    //
    // public async Task<OperationResult<IEnumerable<PaymentDto>> GetPaymentsByMethodAsync(string method)
    // {
    //     var payments = await _paymentRepository.FindAsync(p => p.PaymentMethod == method);
    //     return _mapper.Map<IEnumerable<PaymentDto>>(payments);
    // }
    //
    // public async Task<OperationResult<PaymentDto?> GetPaymentByTransactionIdAsync(string transactionId)
    // {
    //     var payment = await _paymentRepository.FirstOrDefaultAsync(p => p.TransactionId == transactionId);
    //     return payment != null ? _mapper.Map<PaymentDto>(payment) : null;
    // }
    //
    // public async Task<OperationResult<IEnumerable<PaymentDto>> GetFailedPaymentsAsync()
    // {
    //     var payments = await _paymentRepository.FindAsync(p => p.PaymentStatus == "Failed");
    //     return _mapper.Map<IEnumerable<PaymentDto>>(payments);
    // }
    //
    // public async Task<OperationResult<PagedPaymentDto> GetPaymentsPagedAsync(int pageNumber, int pageSize, string? status = null)
    // {
    //     Expression<Func<Payment, bool>>? predicate = null;
    //     if (!string.IsNullOrEmpty(status))
    //         predicate = p => p.PaymentStatus == status;
    //
    //     var pagedResult = await GetPagedAsync(pageNumber, pageSize, predicate);
    //
    //     return new PagedPaymentDto
    //     {
    //         Payments = pagedResult.Items,
    //         TotalCount = pagedResult.TotalCount,
    //         PageNumber = pagedResult.PageNumber,
    //         PageSize = pagedResult.PageSize,
    //         TotalPages = pagedResult.TotalPages
    //     };
    // }
    //
    // public async Task<OperationResult<PagedPaymentDto> GetPaymentsPagedWithFiltersAsync(int pageNumber, int pageSize, PaymentFilterDto filters)
    // {
    //     Expression<Func<Payment, bool>> predicate = p => true;
    //
    //     if (!string.IsNullOrEmpty(filters.PaymentStatus))
    //         predicate = predicate.And(p => p.PaymentStatus == filters.PaymentStatus);
    //
    //     if (!string.IsNullOrEmpty(filters.PaymentMethod))
    //         predicate = predicate.And(p => p.PaymentMethod == filters.PaymentMethod);
    //
    //     if (filters.StartDate.HasValue)
    //         predicate = predicate.And(p => p.CreatedAt >= filters.StartDate);
    //
    //     if (filters.EndDate.HasValue)
    //         predicate = predicate.And(p => p.CreatedAt <= filters.EndDate);
    //
    //     if (filters.OrderId.HasValue)
    //         predicate = predicate.And(p => p.OrderId == filters.OrderId);
    //
    //     if (filters.MinAmount.HasValue)
    //         predicate = predicate.And(p => p.Amount >= filters.MinAmount);
    //
    //     if (filters.MaxAmount.HasValue)
    //         predicate = predicate.And(p => p.Amount <= filters.MaxAmount);
    //
    //     if (!string.IsNullOrEmpty(filters.TransactionId))
    //         predicate = predicate.And(p => p.TransactionId == filters.TransactionId);
    //
    //     var pagedResult = await GetPagedAsync(pageNumber, pageSize, predicate);
    //
    //     return new PagedPaymentDto
    //     {
    //         Payments = pagedResult.Items,
    //         TotalCount = pagedResult.TotalCount,
    //         PageNumber = pagedResult.PageNumber,
    //         PageSize = pagedResult.PageSize,
    //         TotalPages = pagedResult.TotalPages
    //     };
    // }

    // public async Task<decimal> GetTotalPaymentsAsync(DateTime? startDate = null, DateTime? endDate = null)
    // {
    //     Expression<Func<Payment, bool>> predicate = p => p.PaymentStatus == "Completed";
    //
    //     if (startDate.HasValue)
    //         predicate = predicate.And(p => p.CreatedAt >= startDate);
    //
    //     if (endDate.HasValue)
    //         predicate = predicate.And(p => p.CreatedAt <= endDate);
    //
    //     var payments = await _paymentRepository.FindAsync(predicate);
    //     return payments.Sum(p => p.Amount);
    // }

    // public async Task<PaymentStatsDto> GetPaymentStatsAsync(DateTime? startDate = null, DateTime? endDate = null)
    // {
    //     Expression<Func<Payment, bool>> predicate = p => true;
    //
    //     if (startDate.HasValue)
    //         predicate = predicate.And(p => p.CreatedAt >= startDate);
    //
    //     if (endDate.HasValue)
    //         predicate = predicate.And(p => p.CreatedAt <= endDate);
    //
    //     var payments = await _paymentRepository.FindAsync(predicate);
    //     var paymentsList = payments.ToList();
    //
    //     var stats = new PaymentStatsDto
    //     {
    //         TotalPayments = paymentsList.Count,
    //         TotalAmount = paymentsList.Where(p => p.PaymentStatus == "Completed").Sum(p => p.Amount),
    //         SuccessfulPayments = paymentsList.Count(p => p.PaymentStatus == "Completed"),
    //         FailedPayments = paymentsList.Count(p => p.PaymentStatus == "Failed"),
    //         PendingPayments = paymentsList.Count(p => p.PaymentStatus == "Pending"),
    //         RefundedPayments = paymentsList.Count(p => p.PaymentStatus == "Refunded"),
    //         AverageAmount = paymentsList.Any() ? paymentsList.Average(p => p.Amount) : 0
    //     };
    //
    //     // Payment method breakdown
    //     stats.PaymentMethodBreakdown = paymentsList
    //         .GroupBy(p => p.PaymentMethod)
    //         .ToDictionary(g => g.Key, g => g.Count());
    //
    //     stats.PaymentMethodAmounts = paymentsList
    //         .GroupBy(p => p.PaymentMethod)
    //         .ToDictionary(g => g.Key, g => g.Sum(p => p.Amount));
    //
    //     return stats;
    // }

    // public async Task<IEnumerable<PaymentDto>> GetPaymentsByDateRangeAsync(DateTime startDate, DateTime endDate)
    // {
    //     var payments = await _paymentRepository.FindAsync(p => p.CreatedAt >= startDate && p.CreatedAt <= endDate);
    //     return _mapper.Map<IEnumerable<PaymentDto>>(payments);
    // }

    public async Task<PaymentDto?> ProcessPaymentAsync(CreatePaymentDto paymentDto)
    {
        try
        {
            // Create the payment record with pending status
            var payment = _mapper.Map<Payment>(paymentDto);
            payment.Id = Guid.NewGuid();
            payment.CreatedAt = DateTime.UtcNow;
            payment.IsActive = true;
            payment.PaymentStatus = "Pending"; // Set initial status
            payment.ProcessedAt = DateTime.UtcNow;

            var createdPayment = await _paymentRepository.AddAsync(payment);

            // Simulate payment processing logic
            var isPaymentValid = ValidatePaymentData(paymentDto);

            if (isPaymentValid)
            {
                // Generate a mock transaction ID
                var transactionId = $"TXN_{DateTime.UtcNow:yyyyMMddHHmmss}_{Guid.NewGuid().ToString("N")[..8]}";

                // Mark as successful
                await MarkPaymentAsSuccessfulAsync(createdPayment.Entity.Id, transactionId);
                var result = await GetByIdAsync(createdPayment.Entity.Id);
                return result.Data;
            }
            else
            {
                // Mark as failed
                await MarkPaymentAsFailedAsync(createdPayment.Entity.Id, "Payment validation failed");
                var result = await GetByIdAsync(createdPayment.Entity.Id);
                return result.Data;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing payment");
            return null;
        }
    }

    private bool ValidatePaymentData(CreatePaymentDto paymentDto)
    {
        // Add your payment validation logic here
        if (paymentDto.Amount <= 0)
            return false;

        if (string.IsNullOrEmpty(paymentDto.PaymentMethod))
            return false;

        // Add more validation rules as needed
        // For example: check if payment method is supported, validate card details format, etc.

        return true;
    }

    public async Task<PaymentDto?> RefundPaymentAsync(Guid paymentId, decimal? refundAmount = null)
    {
        var payment = await _paymentRepository.GetByIdAsync(paymentId);
        if (payment == null || payment.PaymentStatus != "Completed")
            return null;

        var amountToRefund = refundAmount ?? payment.Amount;

        try
        {
            // Simulate refund processing
            var isRefundValid = await ValidateRefundAsync(payment, amountToRefund);

            if (isRefundValid)
            {
                payment.PaymentStatus = "Refunded";
                payment.UpdatedAt = DateTime.UtcNow;

                await _paymentRepository.UpdateAsync(payment);
                return _mapper.Map<PaymentDto>(payment);
            }
            else
            {
                _logger.LogWarning("Refund validation failed for payment {PaymentId}", paymentId);
                return null;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing refund for payment {PaymentId}", paymentId);
        }

        return null;
    }

    private async Task<bool> ValidateRefundAsync(Payment payment, decimal refundAmount)
    {
        // Add your refund validation logic here
        if (refundAmount <= 0 || refundAmount > payment.Amount)
            return false;

        if (payment.PaymentStatus == "Refunded")
            return false; // Already refunded

        // Add more validation rules as needed
        // For example: check refund time limits, partial refund rules, etc.

        return true;
    }

    public async Task<bool> ValidatePaymentAsync(Guid paymentId)
    {
        var payment = await _paymentRepository.GetByIdAsync(paymentId);
        if (payment == null) return false;

        // Add your payment validation logic here
        return !string.IsNullOrEmpty(payment.TransactionId) &&
               payment.Amount > 0 &&
               !string.IsNullOrEmpty(payment.PaymentMethod);
    }

    public async Task<IEnumerable<PaymentDto>> GetPendingPaymentsAsync()
    {
        var payments = await _paymentRepository.FindAsync(q=>q.Where(p => p.PaymentStatus == "Pending" && !p.IsDeleted));
        return _mapper.Map<IEnumerable<PaymentDto>>(payments);
    }

    public async Task<bool> MarkPaymentAsSuccessfulAsync(Guid paymentId, string transactionId)
    {
        var payment = await _paymentRepository.GetByIdAsync(paymentId);
        if (payment == null) return false;

        payment.PaymentStatus = "Completed";
        payment.TransactionId = transactionId;
        payment.ProcessedAt = DateTime.UtcNow;
        payment.UpdatedAt = DateTime.UtcNow;

        await _paymentRepository.UpdateAsync(payment);
        return true;
    }

    public async Task<bool> MarkPaymentAsFailedAsync(Guid paymentId, string errorReason)
    {
        var payment = await _paymentRepository.GetByIdAsync(paymentId);
        if (payment == null) return false;

        payment.PaymentStatus = "Failed";
        payment.GatewayResponse = errorReason;
        payment.ProcessedAt = DateTime.UtcNow;
        payment.UpdatedAt = DateTime.UtcNow;

        await _paymentRepository.UpdateAsync(payment);
        return true;
    }

    #endregion
}

// Extension method for combining expressions
public static class ExpressionExtensions
{
    public static Expression<Func<T, bool>> And<T>(this Expression<Func<T, bool>> left, Expression<Func<T, bool>> right)
    {
        var parameter = Expression.Parameter(typeof(T));
        var leftVisitor = new ReplaceExpressionVisitor(left.Parameters[0], parameter);
        var rightVisitor = new ReplaceExpressionVisitor(right.Parameters[0], parameter);
        return Expression.Lambda<Func<T, bool>>(
            Expression.AndAlso(leftVisitor.Visit(left.Body), rightVisitor.Visit(right.Body)), parameter);
    }

    private class ReplaceExpressionVisitor : ExpressionVisitor
    {
        private readonly Expression _oldValue;
        private readonly Expression _newValue;

        public ReplaceExpressionVisitor(Expression oldValue, Expression newValue)
        {
            _oldValue = oldValue;
            _newValue = newValue;
        }

        public override Expression Visit(Expression node)
        {
            return node == _oldValue ? _newValue : base.Visit(node);
        }
    }
}