using Adidas.Application.Contracts.RepositoriesContracts.Operation;
using Adidas.DTOs.Operation.PaymentDTOs.Create;
using Adidas.DTOs.Operation.PaymentDTOs.Query;
using Adidas.DTOs.Operation.PaymentDTOs.Result;
using Adidas.DTOs.Operation.PaymentDTOs.Statistics;
using Adidas.DTOs.Operation.PaymentDTOs.Update;
using AutoMapper;
using Microsoft.Extensions.Logging;

namespace Adidas.Application.Contracts.ServicesContracts.Operation.PaymentServices;

public class PaymentService : IPaymentService
{
    private readonly IPaymentRepository _paymentRepository;
    private readonly IOrderRepository _orderRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<PaymentService> _logger;

    public PaymentService(
        IPaymentRepository paymentRepository,
        IOrderRepository orderRepository,
        IMapper mapper,
        ILogger<PaymentService> logger)
    {
        _paymentRepository = paymentRepository;
        _orderRepository = orderRepository;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<PaymentDto> CreatePaymentAsync(CreatePaymentDto dto)
    {
        var order = await _orderRepository.GetByIdAsync(dto.OrderId);
        if (order == null)
            throw new ArgumentException($"Order with ID {dto.OrderId} not found");

        var payment = _mapper.Map<Payment>(dto);
        payment.ProcessedAt = DateTime.UtcNow;

        var created = await _paymentRepository.AddAsync(payment);
        return _mapper.Map<PaymentDto>(created);
    }

    public async Task<PaymentDto?> GetPaymentByIdAsync(Guid id)
    {
        var payment = await _paymentRepository.GetByIdAsync(id);
        return payment != null ? _mapper.Map<PaymentDto>(payment) : null;
    }

    public async Task<PaymentDto?> GetPaymentByTransactionIdAsync(string transactionId)
    {
        var payment = await _paymentRepository.GetPaymentByTransactionIdAsync(transactionId);
        return payment != null ? _mapper.Map<PaymentDto>(payment) : null;
    }

    public async Task<IEnumerable<PaymentDto>> GetPaymentsByOrderIdAsync(Guid orderId)
    {
        var payments = await _paymentRepository.GetPaymentsByOrderIdAsync(orderId);
        return _mapper.Map<IEnumerable<PaymentDto>>(payments);
    }

    public async Task<IEnumerable<PaymentWithOrderDto>> GetPaymentsByOrderIdWithDetailsAsync(Guid orderId)
    {
        var payments = await _paymentRepository.GetPaymentsByOrderIdAsync(orderId);
        return _mapper.Map<IEnumerable<PaymentWithOrderDto>>(payments);
    }

    public async Task<IEnumerable<PaymentDto>> GetPaymentsByStatusAsync(string status)
    {
        var payments = await _paymentRepository.GetPaymentsByStatusAsync(status);
        return _mapper.Map<IEnumerable<PaymentDto>>(payments);
    }

    public async Task<IEnumerable<PaymentDto>> GetPaymentsByMethodAsync(string method)
    {
        var payments = await _paymentRepository.GetPaymentsByMethodAsync(method);
        return _mapper.Map<IEnumerable<PaymentDto>>(payments);
    }

    public async Task<IEnumerable<PaymentDto>> GetPaymentsByDateRangeAsync(DateTime start, DateTime end)
    {
        var payments = await _paymentRepository.FindAsync(p =>
            p.ProcessedAt >= start && p.ProcessedAt <= end && !p.IsDeleted);
        return _mapper.Map<IEnumerable<PaymentDto>>(payments);
    }

    public async Task<IEnumerable<PaymentDto>> GetFailedPaymentsAsync()
    {
        var payments = await _paymentRepository.GetFailedPaymentsAsync();
        return _mapper.Map<IEnumerable<PaymentDto>>(payments);
    }

    public async Task<IEnumerable<PaymentDto>> GetPendingPaymentsAsync()
    {
        return await GetPaymentsByStatusAsync("Pending");
    }

    public async Task<decimal> GetTotalPaymentsAsync(DateTime? start = null, DateTime? end = null)
    {
        return await _paymentRepository.GetTotalPaymentsAsync(start, end);
    }

    public async Task<PaymentStatsDto> GetPaymentStatsAsync(DateTime? start = null, DateTime? end = null)
    {
        var totalAmount = await _paymentRepository.GetTotalPaymentsAsync(start, end);

        var success = await _paymentRepository.GetPaymentsByStatusAsync("Success");
        var failed = await _paymentRepository.GetFailedPaymentsAsync();
        var pending = await _paymentRepository.GetPaymentsByStatusAsync("Pending");

        if (start.HasValue || end.HasValue)
        {
            success = FilterByDate(success, start, end);
            failed = FilterByDate(failed, start, end);
            pending = FilterByDate(pending, start, end);
        }

        var successCount = success.Count();
        var failedCount = failed.Count();
        var pendingCount = pending.Count();
        var totalCount = successCount + failedCount + pendingCount;

        return new PaymentStatsDto
        {
            TotalAmount = totalAmount,
            TotalCount = totalCount,
            SuccessfulCount = successCount,
            FailedCount = failedCount,
            PendingCount = pendingCount,
            SuccessRate = totalCount > 0 ? (decimal)successCount / totalCount * 100 : 0
        };
    }

    public async Task<PagedPaymentDto> GetPaymentsPagedAsync(int page, int size, string? status = null)
    {
        var (items, count) = await _paymentRepository.GetPaymentsPagedAsync(page, size, status);
        return new PagedPaymentDto
        {
            Payments = _mapper.Map<IEnumerable<PaymentDto>>(items),
            TotalCount = count,
            PageNumber = page,
            PageSize = size,
            TotalPages = (int)Math.Ceiling((double)count / size),
            HasNextPage = page * size < count,
            HasPreviousPage = page > 1
        };
    }

    public async Task<PagedPaymentDto> GetPaymentsPagedWithFiltersAsync(int page, int size, PaymentFilterDto filter)
    {
        return await GetPaymentsPagedAsync(page, size, filter.PaymentStatus);
    }

    public async Task<bool> DeletePaymentAsync(Guid id)
    {
        var payment = await _paymentRepository.GetByIdAsync(id);
        if (payment == null)
            return false;

        return true;
    }

    public async Task<bool> MarkPaymentAsSuccessfulAsync(Guid id, string transactionId)
    {
        var dto = new UpdatePaymentDto
        {
            PaymentStatus = "Success",
            TransactionId = transactionId,
            GatewayResponse = "Payment completed successfully"
        };
        return await UpdatePaymentAsync(id, dto) != null;
    }

    public async Task<bool> MarkPaymentAsFailedAsync(Guid id, string error)
    {
        var dto = new UpdatePaymentDto
        {
            PaymentStatus = "Failed",
            GatewayResponse = error
        };
        return await UpdatePaymentAsync(id, dto) != null;
    }

    public async Task<PaymentDto?> ProcessPaymentAsync(CreatePaymentDto dto)
    {
        return await CreatePaymentAsync(dto);
    }

    public async Task<PaymentDto?> RefundPaymentAsync(Guid id, decimal? amount = null)
    {
        var payment = await _paymentRepository.GetByIdAsync(id);
        if (payment == null || payment.PaymentStatus != "Success")
            return null;

        var refund = new Payment
        {
            OrderId = payment.OrderId,
            Amount = -(amount ?? payment.Amount),
            PaymentMethod = payment.PaymentMethod,
            PaymentStatus = "Refunded",
            TransactionId = $"REFUND_{payment.TransactionId}_{DateTime.UtcNow.Ticks}",
            GatewayResponse = "Refund processed",
            ProcessedAt = DateTime.UtcNow
        };

        var created = await _paymentRepository.AddAsync(refund);

        _logger.LogInformation("Refund processed for payment: {PaymentId}", id);
        return _mapper.Map<PaymentDto>(created);
    }

    public async Task<PaymentDto?> UpdatePaymentAsync(Guid id, UpdatePaymentDto dto)
    {
        var payment = await _paymentRepository.GetByIdAsync(id);
        if (payment == null)
            return null;

        _mapper.Map(dto, payment);
        payment.ProcessedAt = DateTime.UtcNow;

        await _paymentRepository.UpdateAsync(payment);
        return _mapper.Map<PaymentDto>(payment);
    }

    public async Task<bool> ValidatePaymentAsync(Guid id)
    {
        var payment = await _paymentRepository.GetByIdAsync(id);
        return payment != null &&
               !string.IsNullOrEmpty(payment.TransactionId) &&
               payment.Amount > 0 &&
               !string.IsNullOrEmpty(payment.PaymentMethod);
    }

    private IEnumerable<Payment> FilterByDate(IEnumerable<Payment> payments, DateTime? start, DateTime? end)
    {
        return payments.Where(p =>
            (!start.HasValue || p.ProcessedAt >= start.Value) &&
            (!end.HasValue || p.ProcessedAt <= end.Value));
    }
}
