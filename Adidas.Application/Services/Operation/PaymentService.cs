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
    private readonly IOrderRepository _orderRepository;
    private readonly ILogger<PaymentService> _logger;

    public PaymentService(
        IPaymentRepository paymentRepository,
        ILogger<PaymentService> logger,
        IOrderRepository orderRepository
        )
    {
        _paymentRepository = paymentRepository;
        _logger = logger;
        _orderRepository = orderRepository;
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
            _logger.LogInformation("Starting payment processing for Order {OrderId}, Amount {Amount}", dto.OrderId, dto.Amount);

            // Validate the order exists before creating payment
            var orderExists = await _orderRepository.GetByIdAsync(dto.OrderId);
            if (orderExists == null)
            {
                _logger.LogWarning("Order {OrderId} not found for payment processing", dto.OrderId);
                return OperationResult<PaymentDto>.Fail($"Order {dto.OrderId} not found");
            }

            // Create the payment record matching your exact Payment model
            var payment = new Payment
            {
                // Required fields
                PaymentMethod = dto.PaymentMethod ?? "Cash on Delivery",
                PaymentStatus = dto.PaymentMethod == "Cash on Delivery" ? "Pending" : "Processing",
                Amount = dto.Amount,
                ProcessedAt = DateTime.UtcNow,
                OrderId = dto.OrderId,

                // Nullable fields - explicitly set to avoid database issues
                TransactionId = $"TXN_{DateTime.UtcNow:yyyyMMddHHmmss}_{Guid.NewGuid().ToString("N")[..8]}",
                GatewayResponse = dto.PaymentMethod == "Cash on Delivery" ? "COD - Payment on delivery" : null,

                // BaseAuditableEntity fields
                Id = Guid.NewGuid(),
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };

            _logger.LogInformation("Created payment entity with ID {PaymentId} for Order {OrderId}", payment.Id, payment.OrderId);

            try
            {
                var createdPayment = await _paymentRepository.AddAsync(payment);
                await _paymentRepository.SaveChangesAsync();

                _logger.LogInformation("Payment {PaymentId} saved to database successfully", payment.Id);
            }
            catch (Exception dbEx)
            {
                _logger.LogError(dbEx, "Database error while saving payment for Order {OrderId}. Payment: PaymentMethod={PaymentMethod}, Amount={Amount}, TransactionId={TransactionId}",
                    dto.OrderId, payment.PaymentMethod, payment.Amount, payment.TransactionId);

                // Check for specific database errors
                if (dbEx.Message.Contains("duplicate key") || dbEx.Message.Contains("unique constraint"))
                {
                    return OperationResult<PaymentDto>.Fail("Duplicate payment transaction detected. Please try again.");
                }

                if (dbEx.Message.Contains("foreign key constraint"))
                {
                    return OperationResult<PaymentDto>.Fail("Invalid order reference. Please verify the order exists.");
                }

                return OperationResult<PaymentDto>.Fail($"Database error while saving payment: {dbEx.Message}");
            }

            // For COD, mark as confirmed immediately
            if (string.Equals(dto.PaymentMethod, "Cash on Delivery", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(dto.PaymentMethod, "COD", StringComparison.OrdinalIgnoreCase))
            {
                payment.PaymentStatus = "Confirmed";
                payment.GatewayResponse = "Cash on Delivery - Payment will be collected upon delivery";

                try
                {
                    await _paymentRepository.SaveChangesAsync();
                    _logger.LogInformation("COD Payment {PaymentId} marked as confirmed", payment.Id);
                }
                catch (Exception updateEx)
                {
                    _logger.LogError(updateEx, "Failed to update COD payment status for Payment {PaymentId}", payment.Id);
                }
            }
            else
            {
                // For other payment methods, validate and process
                var isPaymentValid = ValidatePaymentData(dto);
                if (isPaymentValid)
                {
                    await MarkPaymentAsSuccessfulAsync(payment.Id, payment.TransactionId);
                }
                else
                {
                    await MarkPaymentAsFailedAsync(payment.Id, "Payment validation failed");
                }
            }

            // Return the payment DTO directly from the created entity
            var paymentDto = new PaymentDto
            {
                Id = payment.Id,
                OrderId = payment.OrderId,
                Amount = payment.Amount,
                PaymentMethod = payment.PaymentMethod,
                PaymentStatus = payment.PaymentStatus,
                TransactionId = payment.TransactionId,
                ProcessedAt = payment.ProcessedAt
            };

            _logger.LogInformation("Payment processing completed successfully for Order {OrderId}, Payment {PaymentId}", dto.OrderId, payment.Id);

            return OperationResult<PaymentDto>.Success(paymentDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error processing payment for Order {OrderId}", dto.OrderId);
            return OperationResult<PaymentDto>.Fail($"Payment processing failed: {ex.Message}");
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