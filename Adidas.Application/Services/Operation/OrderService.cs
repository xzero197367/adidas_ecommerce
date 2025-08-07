using Adidas.Application.Contracts.RepositoriesContracts.Feature;
using Adidas.Application.Contracts.RepositoriesContracts.Main;
using Adidas.Application.Contracts.RepositoriesContracts.Operation;
using Adidas.Application.Contracts.ServicesContracts.Operation;
using Adidas.Application.Contracts.ServicesContracts.Static;
using Adidas.Application.Contracts.ServicesContracts.Tracker;
using Adidas.DTOs.Operation.OrderDTOs.Create;
using Adidas.DTOs.Operation.OrderDTOs.Result;
using Adidas.DTOs.Operation.PaymentDTOs.Result;
using Microsoft.Extensions.Logging;
using Adidas.DTOs.CommonDTOs;
using Adidas.DTOs.Operation.OrderDTOs;
using Mapster;
using Microsoft.EntityFrameworkCore;

namespace Adidas.Application.Services.Operation;

public class OrderService : GenericService<Order, OrderDto, OrderCreateDto, OrderUpdateDto>, IOrderService
{
    private readonly IOrderRepository _orderRepository;
    private readonly IShoppingCartRepository _cartRepository;
    private readonly IProductVariantRepository _variantRepository;
    private readonly IInventoryService _inventoryService;
    private readonly INotificationService _notificationService;
    private readonly ILogger<OrderService> _logger;

    public OrderService(
        IOrderRepository orderRepository,
        IShoppingCartRepository cartRepository,
        IProductVariantRepository variantRepository,
        IInventoryService inventoryService,
        INotificationService notificationService,
        ILogger<OrderService> logger) : base(orderRepository, logger)
    {
        _orderRepository = orderRepository;
        _cartRepository = cartRepository;
        _variantRepository = variantRepository;
        _inventoryService = inventoryService;
        _notificationService = notificationService;
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

    public async Task<OperationResult<OrderDto>> CreateOrderFromCartAsync(string userId,
        CreateOrderFromCartDto orderDto)
    {
        var cartItems = await _cartRepository.GetCartItemsByUserIdAsync(userId);
        if (!cartItems.Any())
            return OperationResult<OrderDto>.Fail("Cart is empty");

        try
        {
            // Reserve inventory
            foreach (var item in cartItems)
            {
                var reserved = await _inventoryService.ReserveStockAsync(item.VariantId, item.Quantity);
                if (!reserved.Data)
                {
                    return OperationResult<OrderDto>.Fail("Failed to reserve inventory");
                }
            }

            var orderNumber = await GenerateOrderNumberAsync();
            if (!orderNumber.IsSuccess) return OperationResult<OrderDto>.Fail(orderNumber.ErrorMessage);
            var order = new Order
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                OrderNumber = orderNumber.Data,
                OrderDate = DateTime.UtcNow,
                OrderStatus = OrderStatus.Pending,
                ShippingAddress = orderDto.ShippingAddress,
                BillingAddress = orderDto.BillingAddress,
                TotalAmount = orderDto.TotalAmount,
                Currency = orderDto.Currency
            };

            // Create order items
            var orderItems = new List<OrderItem>();
            decimal totalAmount = 0;

            foreach (var cartItem in cartItems)
            {
                var variant = await _variantRepository.GetByIdAsync(cartItem.VariantId);
                var itemPrice = (variant.Product.SalePrice ?? variant.Product.Price) + variant.PriceAdjustment;

                orderItems.Add(new OrderItem
                {
                    Id = Guid.NewGuid(),
                    OrderId = order.Id,
                    ProductName = variant.Product.Name,
                    VariantId = cartItem.VariantId,
                    Quantity = cartItem.Quantity,
                    UnitPrice = itemPrice,
                    TotalPrice = itemPrice * cartItem.Quantity
                });

                totalAmount += itemPrice * cartItem.Quantity;
            }

            order.Subtotal = totalAmount;
            order.TaxAmount = CalculateTax(totalAmount);
            order.TotalAmount = order.Subtotal + order.TaxAmount;

            // Apply discount if provided
            if (!string.IsNullOrEmpty(orderDto.DiscountCode))
            {
                // Implementation would validate and apply discount
                // order.DiscountAmount = await CalculateDiscountAsync(orderDto.DiscountCode, order.SubTotal);
                // order.TotalAmount -= order.DiscountAmount;
            }

            var createdOrder = await _orderRepository.AddAsync(order);
            createdOrder.State = EntityState.Detached;

            // Clear cart
            await _cartRepository.ClearCartAsync(userId);

            await _notificationService.SendOrderConfirmationAsync(createdOrder.Entity.Id);

            return OperationResult<OrderDto>.Success(createdOrder.Entity.Adapt<OrderDto>());
        }
        catch (Exception ex)
        {
            // Release reserved inventory on failure
            foreach (var item in cartItems)
            {
                await _inventoryService.ReleaseStockAsync(item.VariantId, item.Quantity);
            }

            return OperationResult<OrderDto>.Fail(ex.Message);
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
            await _orderRepository.UpdateAsync(order);

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
            result.State = EntityState.Detached;
            await _orderRepository.SaveChangesAsync();

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
            var totalSales = await _orderRepository.GetTotalSalesAsync(startDate, endDate);
            var orders = await _orderRepository.GetOrdersByDateRangeAsync(
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
}