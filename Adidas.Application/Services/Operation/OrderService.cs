using Adidas.Application.Contracts.RepositoriesContracts.Feature;
using Adidas.Application.Contracts.RepositoriesContracts.Main;
using Adidas.Application.Contracts.RepositoriesContracts.Operation;
using Adidas.Application.Contracts.ServicesContracts.Feature;
using Adidas.Application.Contracts.ServicesContracts.Operation;
using Adidas.Application.Contracts.ServicesContracts.Static;
using Adidas.Application.Contracts.ServicesContracts.Tracker;
using Adidas.DTOs.Common_DTOs;
using Adidas.DTOs.CommonDTOs;
using Adidas.DTOs.Operation.OrderDTOs;
using Adidas.DTOs.Operation.OrderDTOs.Create;
using Adidas.Models.Feature;
using ClosedXML.Excel;
using iTextSharp.text;
using iTextSharp.text.pdf;
using Mapster;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Adidas.Application.Services.Operation;

public class OrderService : GenericService<Order, OrderDto, OrderCreateDto, OrderUpdateDto>, IOrderService
{
    private readonly IOrderRepository _orderRepository;
    private readonly IShoppingCartRepository _cartRepository;
    private readonly IProductVariantRepository _variantRepository;
    private readonly IInventoryService _inventoryService;
    private readonly INotificationService _notificationService;
    private readonly ICouponService _couponService;
    private readonly ILogger<OrderService> _logger;

    public OrderService(
        IOrderRepository orderRepository,
        IShoppingCartRepository cartRepository,
        IProductVariantRepository variantRepository,
        IInventoryService inventoryService,
        INotificationService notificationService,
        ICouponService couponService,
        ILogger<OrderService> logger) : base(orderRepository, logger)
    {
        _orderRepository = orderRepository;
        _cartRepository = cartRepository;
        _variantRepository = variantRepository;
        _inventoryService = inventoryService;
        _notificationService = notificationService;
        _couponService = couponService;
        _logger = logger;
    }

    public async Task<BillingSummaryDto> GetBillingSummaryAsync(string userId, string? promoCode = null)
    {
        var cartItems = await _cartRepository.GetCartItemsByUserIdAsync(userId) ?? new List<ShoppingCart>();

        decimal itemsTotal = 0;

        foreach (var item in cartItems)
        {
            var variant = await _variantRepository.GetByIdAsync(item.VariantId);

            if (variant == null)
            {
                _logger.LogWarning("Variant not found for VariantId: {VariantId}", item.VariantId);
                continue;
            }

            if (variant.Product == null)
            {
                _logger.LogWarning("Product not found for VariantId: {VariantId}", item.VariantId);
                continue;
            }

            var price = (variant.Product.SalePrice ?? variant.Product.Price) + variant.PriceAdjustment;
            itemsTotal += price * item.Quantity;
        }

        decimal shipping = itemsTotal >= 100 ? 0 : 10;
        string shippingText = shipping == 0 ? "Free" : $"EGP {shipping:N2}";

        decimal discount = 0;

        if (!string.IsNullOrEmpty(promoCode))
        {
            var couponResult = await _couponService.ApplyCouponToCartAsync(userId, promoCode, itemsTotal);
            if (couponResult?.Success == true)
            {
                discount = couponResult.DiscountApplied;
            }
        }

        decimal total = itemsTotal + shipping - discount;

        return new BillingSummaryDto
        {
            Subtotal = itemsTotal,
            Shipping = shipping,
            ShippingText = shippingText,
            Discount = discount,
            Total = total
        };
    }


    public async Task<OperationResult<PagedResultDto<OrderDto>>> GetPagedOrdersAsync(int pageNumber, int pageSize,
        OrderFilterDto? filter = null)
    {
        var pagedOrders = await _orderRepository.GetPagedAsync(pageNumber, pageSize, q =>
        {
            var query = q.Where(o => o.IsDeleted == false).OrderByDescending(o => o.OrderDate).AsQueryable();
            if (filter != null)
            {
                if (filter?.OrderNumber != null)
                {
                    query = query.Where(o => o.OrderNumber.Contains(filter.OrderNumber));
                }

                if (filter?.OrderStatus != null)
                {
                    query = query.Where(o => o.OrderStatus == filter.OrderStatus);
                }

                if (filter?.OrderDate != null)
                {
                    var orderDate = filter.OrderDate.Value.Date;
                    query = query.Where(o => o.OrderDate == orderDate);
                }
            }

            return query;
        });

        return OperationResult<PagedResultDto<OrderDto>>.Success(pagedOrders.Adapt<PagedResultDto<OrderDto>>());
    }

    public async Task<OperationResult<IEnumerable<OrderDto>>> GetOrdersByUserIdAsync(string userId)
    {
        try
        {
            var orders = await _orderRepository.GetOrdersByUserIdAsync(userId);
            return OperationResult<IEnumerable<OrderDto>>.Success(orders.Adapt<IEnumerable<OrderDto>>());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting orders for user {UserId}", userId);
            return OperationResult<IEnumerable<OrderDto>>.Fail(ex.Message);
        }
    }

    public async Task<OperationResult<OrderDto>> GetOrderByOrderNumberAsync(string orderNumber)
    {
        try
        {
            var order = await _orderRepository.GetOrderByNumberAsync(orderNumber);
            if (order == null)
            {
                return OperationResult<OrderDto>.Fail($"Order with number {orderNumber} was not found.");
            }

            return OperationResult<OrderDto>.Success(order.Adapt<OrderDto>());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting order {OrderNumber}", orderNumber);
            return OperationResult<OrderDto>.Fail(ex.Message);
        }
    }

    public async Task<OperationResult<IEnumerable<OrderDto>>> GetOrdersByStatusAsync(OrderStatus status)
    {
        try
        {
            var orders = await _orderRepository.GetOrdersByStatusAsync(status);
            return OperationResult<IEnumerable<OrderDto>>.Success(orders.Adapt<IEnumerable<OrderDto>>());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting orders by status {Status}", status);
            return OperationResult<IEnumerable<OrderDto>>.Fail(ex.Message);
        }
    }


    public async Task<OperationResult<bool>> UpdateOrderStatusAsync(Guid orderId, OrderStatus newStatus)
    {
        try
        {
            var order = await _orderRepository.GetByIdAsync(orderId);
            if (order == null)
            {
                return OperationResult<bool>.Fail($"Order with id {orderId} was not found.");
            }

            order.OrderStatus = newStatus;
            var result = await _orderRepository.UpdateAsync(order);
            await _orderRepository.SaveChangesAsync();
            result.State = EntityState.Detached;
            await _notificationService.SendOrderStatusUpdateAsync(orderId);
            return OperationResult<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating order status");
            return OperationResult<bool>.Fail(ex.Message);
        }
    }

    public async Task<OperationResult<OrderDto>> GetOrderWithItemsAsync(Guid orderId)
    {
        try
        {
            var order = await _orderRepository.GetOrderWithItemsAsync(orderId);
            if (order == null)
            {
                return OperationResult<OrderDto>.Fail($"Order with id {orderId} was not found.");
            }

            return OperationResult<OrderDto>.Success(order.Adapt<OrderDto>());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting order with items");
            return OperationResult<OrderDto>.Fail(ex.Message);
        }
    }

    public async Task<OperationResult<decimal>> CalculateOrderTotalAsync(string userId, string? discountCode = null)
    {
        try
        {
            var cartItems = await _cartRepository.GetCartItemsByUserIdAsync(userId);
            decimal subtotal = 0;

            foreach (var item in cartItems)
            {
                var variant = await _variantRepository.GetByIdAsync(item.VariantId);
                var itemPrice = (variant.Product.SalePrice ?? variant.Product.Price) + variant.PriceAdjustment;
                subtotal += itemPrice * item.Quantity;
            }

            var shipping = CalculateShippingCost(subtotal);
            var tax = CalculateTax(subtotal);
            var total = subtotal + shipping + tax;

            // Apply discount if provided
            if (!string.IsNullOrEmpty(discountCode))
            {
                // Implementation would calculate discount
            }

            return OperationResult<decimal>.Success(total);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating order total");
            return OperationResult<decimal>.Fail(ex.Message);
        }
    }

    public async Task<OperationResult<bool>> CancelOrderAsync(Guid orderId, string reason)
    {
        try
        {
            var order = await _orderRepository.GetOrderWithItemsAsync(orderId);
            if (order == null || order.OrderStatus != OrderStatus.Pending)
            {
                return OperationResult<bool>.Fail($"Order with id {orderId} was not found.");
            }

            // Release inventory
            foreach (var item in order.OrderItems)
            {
                await _inventoryService.ReleaseStockAsync(item.VariantId, item.Quantity);
            }

            order.OrderStatus = OrderStatus.Cancelled;

            var result = await _orderRepository.UpdateAsync(order);

            await _orderRepository.SaveChangesAsync();
            result.State = EntityState.Detached;

            return OperationResult<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error cancelling order");
            return OperationResult<bool>.Fail(ex.Message);
        }
    }

    public async Task<OperationResult<OrderSummaryDto>> GetOrderSummaryAsync(DateTime? startDate = null,
        DateTime? endDate = null)
    {
        try
        {
            var totalSales = _orderRepository.GetTotalSalesAsync(startDate, endDate);
            var orders = _orderRepository.GetOrdersByDateRangeAsync(
                startDate ?? DateTime.UtcNow.AddDays(-30),
                endDate ?? DateTime.UtcNow);

            return OperationResult<OrderSummaryDto>.Success(new OrderSummaryDto
            {
                TotalAmount = totalSales,
                ItemCount = orders.Count()
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting order summary");
            return OperationResult<OrderSummaryDto>.Fail(ex.Message);
        }
    }

    private async Task<OperationResult<string>> GenerateOrderNumberAsync()
    {
        try
        {
            var prefix = "ADI";
            var timestamp = DateTime.UtcNow.ToString("yyyyMMdd");
            var counter = 1;
            var orderNumber = $"{prefix}{timestamp}{counter:D4}";

            while (await _orderRepository.GetOrderByNumberAsync(orderNumber) != null)
            {
                counter++;
                orderNumber = $"{prefix}{timestamp}{counter:D4}";
            }

            return OperationResult<string>.Success(orderNumber);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating order number");
            return OperationResult<string>.Fail(ex.Message);
        }
    }

    private static decimal CalculateShippingCost(decimal subtotal)
    {
        return subtotal >= 100 ? 0 : 10; // Free shipping over $100
    }

    private static decimal CalculateTax(decimal subtotal)
    {
        return subtotal * 0.08m; // 8% tax
    }

    public override async Task BeforeCreateAsync(Order entity)
    {
        var result = await GenerateOrderNumberAsync();
        if (result.IsSuccess)
        {
            entity.OrderNumber = result.Data;
        }
    }


    public async Task<byte[]> ExportToExcelAsync(DateTime? startDate = null, DateTime? endDate = null)
    {
        var result = await GetOrderSummaryAsync(startDate, endDate);

        if (!result.IsSuccess)
        {
            throw new Exception(result.ErrorMessage);
        }

        var summary = result.Data;

        using (var workbook = new XLWorkbook())
        {
            var worksheet = workbook.Worksheets.Add("Order Summary");

            // Header
            worksheet.Cell(1, 1).Value = "Order Summary Report";
            worksheet.Range(1, 1, 1, 4).Merge();
            worksheet.Cell(2, 1).Value = $"Date: {DateTime.Now:yyyy-MM-dd HH:mm}";
            worksheet.Range(2, 1, 2, 4).Merge();

            // Period
            var period = startDate.HasValue || endDate.HasValue
                ? $"From {startDate?.ToString("yyyy-MM-dd") ?? "Start"} to {endDate?.ToString("yyyy-MM-dd") ?? "End"}"
                : "All Time";
            worksheet.Cell(3, 1).Value = $"Period: {period}";
            worksheet.Range(3, 1, 3, 4).Merge();

            // Summary
            int row = 5;
            worksheet.Cell(row++, 1).Value = "Total Count";
            worksheet.Cell(row - 1, 2).Value = summary.ItemCount;

            worksheet.Cell(row++, 1).Value = "Total Amount";
            worksheet.Cell(row - 1, 2).Value = summary.TotalAmount;

            // Formatting
            worksheet.Columns().AdjustToContents();

            using (var stream = new MemoryStream())
            {
                workbook.SaveAs(stream);
                return stream.ToArray();
            }
        }
    }

    public async Task<byte[]> ExportToPdfAsync(DateTime? startDate = null, DateTime? endDate = null)
    {
        var result = await GetOrderSummaryAsync(startDate, endDate);

        if (!result.IsSuccess)
        {
            throw new Exception(result.ErrorMessage);
        }

        var summary = result.Data;

        using (var stream = new MemoryStream())
        {
            var document = new Document(PageSize.A4, 15, 15, 15, 15);
            var writer = PdfWriter.GetInstance(document, stream);
            document.Open();

            // Title
            var titleFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 18);
            var title = new Paragraph("Order Summary Report", titleFont)
            {
                Alignment = Element.ALIGN_CENTER,
                SpacingAfter = 20f
            };
            document.Add(title);

            // Date and Period
            var dateFont = FontFactory.GetFont(FontFactory.HELVETICA, 10);
            document.Add(new Paragraph($"Generated on: {DateTime.Now:yyyy-MM-dd HH:mm}", dateFont));

            var period = startDate.HasValue || endDate.HasValue
                ? $"Period: From {startDate?.ToString("yyyy-MM-dd") ?? "Start"} to {endDate?.ToString("yyyy-MM-dd") ?? "End"}"
                : "Period: All Time";
            document.Add(new Paragraph(period, dateFont));

            // Add some space
            document.Add(new Paragraph("\n"));

            // Summary
            var boldFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD);
            var normalFont = FontFactory.GetFont(FontFactory.HELVETICA);

            document.Add(new Paragraph("Summary:", boldFont));
            document.Add(new Paragraph($"Total Count: {summary.ItemCount}", normalFont));
            document.Add(new Paragraph($"Total Amount: {summary.TotalAmount:C}", normalFont));

            document.Close();

            return stream.ToArray();
        }
    }
    public async Task<OperationResult<object>> GetFormattedOrderSummaryAsync(string userId, string? couponCode = null)
    {
        try
        {
            var cartItems = await _cartRepository.GetCartItemsByUserIdAsync(userId) ?? new List<ShoppingCart>();
            decimal subtotal = 0;

            foreach (var item in cartItems)
            {
                var variant = await _variantRepository.GetByIdAsync(item.VariantId);

                if (variant == null)
                {
                    _logger.LogWarning("Variant not found for VariantId: {VariantId}", item.VariantId);
                    continue;
                }

                if (variant.Product == null)
                {
                    _logger.LogWarning("Product not found for VariantId: {VariantId}", item.VariantId);
                    continue;
                }

                var itemPrice = (variant.Product.SalePrice ?? variant.Product.Price) + variant.PriceAdjustment;
                subtotal += itemPrice * item.Quantity;
            }

            var shipping = subtotal >= 100 ? 0 : 10; 
            var total = subtotal + shipping;

            decimal discount = 0;
            if (!string.IsNullOrEmpty(couponCode))
            {
                var couponResult = await _couponService.ApplyCouponToOrderAsync(Guid.Empty, couponCode); 
                if (couponResult?.Success == true)
                {
                    discount = couponResult.DiscountApplied;
                    total -= discount;
                }
            }

            var response = new
            {
                Subtotal = $"EGP {subtotal:N2}",
                Delivery = shipping == 0 ? "Free" : $"EGP {shipping:N2}",
                Message = shipping == 0 ? "You unlocked Free Shipping!" : "",
                Discount = discount > 0 ? $"EGP {discount:N2}" : null,
                Total = $"EGP {total:N2}"
            };

            return OperationResult<object>.Success(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating formatted order summary");
            return OperationResult<object>.Fail(ex.Message);
        }
    }

}