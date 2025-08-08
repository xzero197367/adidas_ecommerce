using Adidas.Application.Contracts.RepositoriesContracts.Operation;
using Adidas.Application.Contracts.ServicesContracts.Operation;
using Microsoft.Extensions.Logging;
using System.Linq.Expressions;
using Adidas.DTOs.CommonDTOs;
using Adidas.DTOs.Operation.PaymentDTOs;
using Mapster;
using MapsterMapper;
using Microsoft.EntityFrameworkCore;

namespace Adidas.Application.Services.Operation;

public class PaymentService : IPaymentService
{
    private readonly IPaymentRepository _paymentRepository;
    private readonly ILogger<PaymentService> _logger;

    public PaymentService(
        IPaymentRepository paymentRepository,
        ILogger<PaymentService> logger)
    {
        _paymentRepository = paymentRepository;
        _logger = logger;
    }


    #region Payment-Specific Methods

    // Legacy method that now calls the generic method
    public async Task<OperationResult<PaymentDto>> GetPaymentByIdAsync(Guid id)
    {
        try
        {
            var payment = await _paymentRepository.GetByIdAsync(id);
            return OperationResult<PaymentDto>.Success(payment.Adapt<PaymentDto>());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting payment by id {Id}", id);
            return OperationResult<PaymentDto>.Fail("Error getting payment by id");
        }
    }

    // Legacy method that now calls the generic method
    public async Task<OperationResult<PaymentDto>> CreatePaymentAsync(PaymentCreateDto paymentCreateDto)
    {
        try
        {
            var result = await _paymentRepository.AddAsync(paymentCreateDto.Adapt<Payment>());
            await _paymentRepository.SaveChangesAsync();
            result.State = EntityState.Detached;
            return OperationResult<PaymentDto>.Success(result.Entity.Adapt<PaymentDto>());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating payment");
            return OperationResult<PaymentDto>.Fail("Error creating payment");
        }
    }


    public async Task<OperationResult<PaymentDto>> ProcessPaymentAsync(PaymentCreateDto dto)
    {
        try
        {
            // Create the payment record with pending status
            var payment = dto.Adapt<Payment>();
            payment.Id = Guid.NewGuid();
            payment.CreatedAt = DateTime.UtcNow;
            payment.IsActive = true;
            payment.PaymentStatus = "Pending"; // Set initial status
            payment.ProcessedAt = DateTime.UtcNow;

            var createdPayment = await _paymentRepository.AddAsync(payment);

            // Simulate payment processing logic
            var isPaymentValid = ValidatePaymentData(dto);

            if (isPaymentValid)
            {
                // Generate a mock transaction ID
                var transactionId = $"TXN_{DateTime.UtcNow:yyyyMMddHHmmss}_{Guid.NewGuid().ToString("N")[..8]}";

                // Mark as successful
                await MarkPaymentAsSuccessfulAsync(createdPayment.Entity.Id, transactionId);
                var result = await _paymentRepository.GetByIdAsync(createdPayment.Entity.Id);
                return OperationResult<PaymentDto>.Success(result.Adapt<PaymentDto>());
            }
            else
            {
                // Mark as failed
                await MarkPaymentAsFailedAsync(createdPayment.Entity.Id, "Payment validation failed");
                var result = await _paymentRepository.GetByIdAsync(createdPayment.Entity.Id);
                return OperationResult<PaymentDto>.Success(result.Adapt<PaymentDto>());
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing payment");
            return OperationResult<PaymentDto>.Fail("Error processing payment");
        }
    }

    private bool ValidatePaymentData(PaymentCreateDto dto)
    {
        // Add your payment validation logic here
        if (dto.Amount <= 0)
            return false;

        if (string.IsNullOrEmpty(dto.PaymentMethod))
            return false;

        // Add more validation rules as needed
        // For example: check if payment method is supported, validate card details format, etc.

        return true;
    }

    public async Task<OperationResult<PaymentDto>> RefundPaymentAsync(Guid paymentId, decimal? refundAmount = null)
    {
        var payment = await _paymentRepository.GetByIdAsync(paymentId);
        if (payment == null || payment.PaymentStatus != "Completed")
            return null;

        var amountToRefund = refundAmount ?? payment.Amount;

        try
        {
            // Simulate refund processing
            var isRefundValid = await ValidateRefundAsync(payment, amountToRefund);

            if (isRefundValid.Data)
            {
                payment.PaymentStatus = "Refunded";
                payment.UpdatedAt = DateTime.UtcNow;

                var result = await _paymentRepository.UpdateAsync(payment);
                await _paymentRepository.SaveChangesAsync();
                result.State = EntityState.Detached;

                return OperationResult<PaymentDto>.Success(result.Entity.Adapt<PaymentDto>());
            }
            else
            {
                _logger.LogWarning("Refund validation failed for payment {PaymentId}", paymentId);
                return OperationResult<PaymentDto>.Fail("Refund validation failed");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing refund for payment {PaymentId}", paymentId);
            return OperationResult<PaymentDto>.Fail("Error processing refund");
        }
    }

    private async Task<OperationResult<bool>> ValidateRefundAsync(Payment payment, decimal refundAmount)
    {
        // Add your refund validation logic here
        if (refundAmount <= 0 || refundAmount > payment.Amount)
        {
            return OperationResult<bool>.Fail("Invalid refund amount"); // Refund amount is invalid();
        }

        if (payment.PaymentStatus == "Refunded")
        {
            return OperationResult<bool>.Fail("Payment is already refunded");
        }

        // Add more validation rules as needed
        // For example: check refund time limits, partial refund rules, etc.

        return OperationResult<bool>.Success(true);
    }

    public async Task<OperationResult<bool>> ValidatePaymentAsync(Guid paymentId)
    {
        try
        {
            var payment = await _paymentRepository.GetByIdAsync(paymentId);
            if (payment == null)
            {
                return OperationResult<bool>.Fail("Payment not found");
            }

            // Add your payment validation logic here
            return OperationResult<bool>.Success(!string.IsNullOrEmpty(payment.TransactionId) &&
                                                 payment.Amount > 0 &&
                                                 !string.IsNullOrEmpty(payment.PaymentMethod));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating payment {PaymentId}", paymentId);
            return OperationResult<bool>.Fail("Error validating payment");
        }
    }

    public async Task<OperationResult<IEnumerable<PaymentDto>>> GetPendingPaymentsAsync()
    {
        try
        {
            var payments =
                await _paymentRepository.FindAsync(q => q.Where(p => p.PaymentStatus == "Pending" && !p.IsDeleted));
            return OperationResult<IEnumerable<PaymentDto>>.Success(payments.Adapt<IEnumerable<PaymentDto>>());
        }catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving pending payments");
            return OperationResult<IEnumerable<PaymentDto>>.Fail("Error retrieving pending payments");
        }
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

    public async Task<OperationResult<bool>> MarkPaymentAsFailedAsync(Guid paymentId, string errorReason)
    {
        try
        {
            var payment = await _paymentRepository.GetByIdAsync(paymentId);
            if (payment == null)
            {
                return OperationResult<bool>.Fail("Payment not found");
            }

            payment.PaymentStatus = "Failed";
            payment.GatewayResponse = errorReason;
            payment.ProcessedAt = DateTime.UtcNow;
            payment.UpdatedAt = DateTime.UtcNow;

            await _paymentRepository.UpdateAsync(payment);
            return OperationResult<bool>.Success(true);
        }catch (Exception ex)
        {
            _logger.LogError(ex, "Error marking payment {PaymentId} as failed", paymentId);
            return OperationResult<bool>.Fail("Error marking payment as failed");
        }
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