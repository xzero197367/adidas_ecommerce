using Adidas.Application.Contracts.RepositoriesContracts.Feature;
using Adidas.Application.Contracts.RepositoriesContracts.Main;
using Adidas.Application.Contracts.RepositoriesContracts.Operation;
using Adidas.Application.Contracts.ServicesContracts.Feature;
using Adidas.Application.Contracts.ServicesContracts.Operation;
using Adidas.Application.Contracts.ServicesContracts.Static;
using Adidas.Application.Contracts.ServicesContracts.Tracker;
using Adidas.Context;
using Adidas.DTOs.Common_DTOs;
using Adidas.DTOs.CommonDTOs;
using Adidas.DTOs.Feature.OrderCouponDTOs;
using Adidas.DTOs.Operation.OrderDTOs;
using Adidas.DTOs.Operation.OrderDTOs.Create;
using Adidas.DTOs.Operation.PaymentDTOs;
using Adidas.DTOs.People.Address_DTOs;
using Adidas.Models.Feature;
using ClosedXML.Excel;
using DocumentFormat.OpenXml.Office.CustomUI;
using iTextSharp.text;
using iTextSharp.text.pdf;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace Adidas.Application.Services.Operation;

public class OrderService : GenericService<Order, OrderDto, OrderCreateDto, OrderUpdateDto>, IOrderService
{
    private readonly IOrderRepository _orderRepository;
    private readonly IOrderItemRepository _orderItemRepository;
    private readonly IShoppingCartRepository _cartRepository;
    private readonly IProductVariantRepository _variantRepository;
    private readonly IInventoryService _inventoryService;
    private readonly INotificationService _notificationService;
    private readonly ICouponService _couponService;
    private readonly ILogger<OrderService> _logger;
    private readonly AdidasDbContext _context;

    public OrderService(
        IOrderRepository orderRepository,
        IOrderItemRepository orderItemRepository,
        IShoppingCartRepository cartRepository,
        IProductVariantRepository variantRepository,
        IInventoryService inventoryService,
        INotificationService notificationService,
        ICouponService couponService,
        ILogger<OrderService> logger , AdidasDbContext context) : base(orderRepository, logger)
    {
        _orderRepository = orderRepository;
        _orderItemRepository = orderItemRepository;
        _cartRepository = cartRepository;
        _variantRepository = variantRepository;
        _inventoryService = inventoryService;
        _notificationService = notificationService;
        _couponService = couponService;
        _logger = logger;
        _context = context;
    }

    #region Manual Mapping Methods

    private OrderDto MapToOrderDto(Order order)
    {
        if (order == null) return null;

        return new OrderDto
        {
            Id = order.Id,
            OrderNumber = order.OrderNumber,
            OrderStatus = order.OrderStatus,
            Subtotal = order.Subtotal,
            TaxAmount = order.TaxAmount,
            ShippingAmount = order.ShippingAmount,
            DiscountAmount = order.DiscountAmount,
            TotalAmount = order.TotalAmount,
            Currency = order.Currency,
            OrderDate = order.OrderDate,
            ShippedDate = order.ShippedDate,
            DeliveredDate = order.DeliveredDate,
            ShippingAddress = DeserializeAddress(order.ShippingAddress),
            BillingAddress = DeserializeAddress(order.BillingAddress),
            Notes = order.Notes,
            UserId = order.UserId,
            UserName = order.User?.UserName,
            UserEmail = order.User?.Email,
            Payments = order.Payments?.Select(MapToPaymentDto).ToList() ?? new List<PaymentDto>(),
            OrderCoupons = order.OrderCoupons?.Select(MapToOrderCouponDto).ToList() ?? new List<OrderCouponDto>()
        };
    }

    private IEnumerable<OrderDto> MapToOrderDtoList(IEnumerable<Order> orders)
    {
        return orders?.Select(MapToOrderDto) ?? new List<OrderDto>();
    }

    private Order MapToOrder(OrderCreateDto orderCreateDto)
    {
        if (orderCreateDto == null) return null;

        return new Order
        {
            OrderNumber = orderCreateDto.OrderNumber,
            OrderStatus = orderCreateDto.OrderStatus,
            Subtotal = orderCreateDto.Subtotal,
            TaxAmount = orderCreateDto.TaxAmount,
            ShippingAmount = orderCreateDto.ShippingAmount,
            DiscountAmount = orderCreateDto.DiscountAmount,
            TotalAmount = orderCreateDto.TotalAmount,
            Currency = orderCreateDto.Currency,
            OrderDate = orderCreateDto.OrderDate,
            ShippedDate = orderCreateDto.ShippedDate,
            DeliveredDate = orderCreateDto.DeliveredDate,
            ShippingAddress = orderCreateDto.ShippingAddress,
            BillingAddress = orderCreateDto.BillingAddress,
            Notes = orderCreateDto.Notes,
            UserId = orderCreateDto.UserId,
            OrderItems = orderCreateDto.OrderItems?.Select(MapToOrderItem).ToList() ?? new List<OrderItem>()
        };
    }

    private OrderItem MapToOrderItem(OrderItemCreateDto orderItemCreateDto)
    {
        if (orderItemCreateDto == null) return null;

        return new OrderItem
        {
            OrderId = Guid.NewGuid(), // This will be set later when the order is created
            VariantId = orderItemCreateDto.VariantId,
            Quantity = orderItemCreateDto.Quantity,
            UnitPrice = orderItemCreateDto.UnitPrice,
            TotalPrice = orderItemCreateDto.Quantity * orderItemCreateDto.UnitPrice,
            ProductName = "", // Will be filled from variant/product
            VariantDetails = "" // Will be filled from variant
        };
    }

    private PaymentDto MapToPaymentDto(Payment payment)
    {
        // Implementation depends on your Payment entity structure
        // Return a basic mapping for now
        return new PaymentDto();
    }

    private OrderCouponDto MapToOrderCouponDto(OrderCoupon orderCoupon)
    {
        // Implementation depends on your OrderCoupon entity structure
        // Return a basic mapping for now
        return new OrderCouponDto();
    }

    private Dictionary<string, object> DeserializeAddress(string addressJson)
    {
        try
        {
            return string.IsNullOrEmpty(addressJson)
                ? new Dictionary<string, object>()
                : JsonSerializer.Deserialize<Dictionary<string, object>>(addressJson);
        }
        catch
        {
            return new Dictionary<string, object>();
        }
    }

    private OrderSummaryDto MapToOrderSummaryDto(Order order, int itemCount = 0)
    {
        if (order == null) return null;

        return new OrderSummaryDto
        {
            Id = order.Id,
            OrderNumber = order.OrderNumber,
            TotalAmount = order.TotalAmount,
            ItemCount = itemCount > 0 ? itemCount : order.OrderItems?.Count ?? 0,
            Status = order.OrderStatus.ToString(),
            OrderDate = order.OrderDate
        };
    }

    private PagedResultDto<OrderDto> MapToPagedOrderDto(PagedResultDto<Order> pagedOrders)
    {
        return new PagedResultDto<OrderDto>
        {
            Items = MapToOrderDtoList(pagedOrders.Items),
            TotalCount = pagedOrders.TotalCount,
            PageNumber = pagedOrders.PageNumber,
            PageSize = pagedOrders.PageSize
        };
    }

    #endregion

    #region New Required Methods

    public async Task<OperationResult<OrderDto>> GetOrderByIdAsync(Guid id)
    {
        try
        {
            var order = await _orderRepository.GetByIdAsync(id,
                o => o.User,
                o => o.OrderItems,
                o => o.Payments,
                o => o.OrderCoupons);

            if (order == null)
            {
                return OperationResult<OrderDto>.Fail($"Order with id {id} was not found.");
            }

            var orderDto = MapToOrderDto(order);
            return OperationResult<OrderDto>.Success(orderDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting order by id {OrderId}", id);
            return OperationResult<OrderDto>.Fail(ex.Message);
        }
    }

    public async Task<OperationResult<OrderDto>> CreateOrderFromCartAsync(CreateOrderDTO createOrderDto)
    {
        try
        {
            var cartItems = await _cartRepository.GetCartItemsByUserIdAsync(createOrderDto.UserId);

            if (cartItems == null || !cartItems.Any())
            {
                return OperationResult<OrderDto>.Fail("Cart is empty, cannot create order.");
            }

            _logger.LogInformation("Processing cart with {Count} items for user {UserId}", cartItems.Count(), createOrderDto.UserId);

            // Generate order ID first
            var orderId = Guid.NewGuid();

            // Generate order number with better uniqueness
            var orderNumberResult = await GenerateOrderNumberAsync();
            if (!orderNumberResult.IsSuccess)
            {
                return OperationResult<OrderDto>.Fail("Failed to generate order number");
            }

            // Calculate amounts
            decimal subtotal = 0;
            var orderItems = new List<OrderItem>();

            foreach (var cartItem in cartItems)
            {
                _logger.LogInformation("Processing cart item - VariantId: {VariantId}, Quantity: {Quantity}",
                    cartItem.VariantId, cartItem.Quantity);

                // Get variant with product included - THIS IS CRITICAL
                var variant = await _variantRepository.GetByIdAsync(cartItem.VariantId);

                if (variant == null)
                {
                    _logger.LogError("Variant not found for VariantId: {VariantId}", cartItem.VariantId);
                    continue;
                }

                // Make sure Product is loaded - if not, load it explicitly
                if (variant.Product == null)
                {
                    _logger.LogWarning("Product not loaded for variant {VariantId}, loading explicitly", cartItem.VariantId);

                    // Try to get the variant with product included
                    variant = await _context.ProductVariants
                        .Include(v => v.Product)
                        .FirstOrDefaultAsync(v => v.Id == cartItem.VariantId);

                    if (variant?.Product == null)
                    {
                        _logger.LogError("Product still not found for VariantId: {VariantId}", cartItem.VariantId);
                        continue;
                    }
                }

                _logger.LogInformation("Product found - Name: {ProductName}, Price: {Price}, SalePrice: {SalePrice}, PriceAdjustment: {PriceAdjustment}",
                    variant.Product.Name, variant.Product.Price, variant.Product.SalePrice, variant.PriceAdjustment);

                // Check inventory
                try
                {
                    var hasStock = await _inventoryService.HasSufficientStockAsync(cartItem.VariantId, cartItem.Quantity);
                    if (!hasStock.IsSuccess)
                    {
                        return OperationResult<OrderDto>.Fail($"Insufficient stock for product variant {cartItem.VariantId}");
                    }
                }
                catch (Exception inventoryEx)
                {
                    _logger.LogWarning(inventoryEx, "Could not check inventory for variant {VariantId}, proceeding anyway", cartItem.VariantId);
                }

                // Calculate unit price - Use SalePrice if available, otherwise regular Price
                var basePrice = variant.Product.SalePrice ?? variant.Product.Price;
                var unitPrice = basePrice + variant.PriceAdjustment;
                var totalPrice = unitPrice * cartItem.Quantity;

                _logger.LogInformation("Price calculation - BasePrice: {BasePrice}, PriceAdjustment: {PriceAdjustment}, UnitPrice: {UnitPrice}, Quantity: {Quantity}, TotalPrice: {TotalPrice}",
                    basePrice, variant.PriceAdjustment, unitPrice, cartItem.Quantity, totalPrice);

                subtotal += totalPrice;

                var orderItem = new OrderItem
                {
                    Id = Guid.NewGuid(), // Add explicit ID
                    OrderId = orderId,
                    VariantId = cartItem.VariantId,
                    Quantity = cartItem.Quantity,
                    UnitPrice = unitPrice,
                    TotalPrice = totalPrice,
                    ProductName = variant.Product.Name,
                    VariantDetails = $"Size: {variant.Size}, Color: {variant.Color}",
                    CreatedAt = DateTime.UtcNow,
                    IsActive = true
                };

                orderItems.Add(orderItem);
            }

            _logger.LogInformation("Order calculation - Subtotal: {Subtotal}, Items count: {ItemsCount}", subtotal, orderItems.Count);

            if (orderItems.Count == 0)
            {
                return OperationResult<OrderDto>.Fail("No valid items found in cart to create order.");
            }

            if (subtotal <= 0)
            {
                return OperationResult<OrderDto>.Fail("Order subtotal is zero or negative. Please check product prices.");
            }

            var taxAmount = CalculateTax(subtotal);
            var shippingAmount = CalculateShippingCost(subtotal);

            // Apply discount if coupon code is provided
            decimal discountAmount = 0;
            if (!string.IsNullOrEmpty(createOrderDto.CouponCode))
            {
                try
                {
                    var discountedTotal = await _couponService.CalculateCouponAmountAsync(createOrderDto.CouponCode, subtotal);
                    discountAmount = subtotal - discountedTotal;
                    _logger.LogInformation("Discount applied - Original: {Original}, Discounted: {Discounted}, Discount: {Discount}",
                        subtotal, discountedTotal, discountAmount);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to apply coupon {CouponCode}", createOrderDto.CouponCode);
                }
            }

            var totalAmount = subtotal - discountAmount + taxAmount + shippingAmount;

            _logger.LogInformation("Final order totals - Subtotal: {Subtotal}, Tax: {Tax}, Shipping: {Shipping}, Discount: {Discount}, Total: {Total}",
                subtotal, taxAmount, shippingAmount, discountAmount, totalAmount);

            var order = new Order
            {
                Id = orderId,
                OrderNumber = orderNumberResult.Data,
                OrderStatus = OrderStatus.Pending,
                Subtotal = subtotal,
                TaxAmount = taxAmount,
                ShippingAmount = shippingAmount,
                DiscountAmount = discountAmount,
                TotalAmount = totalAmount,
                Currency = createOrderDto.Currency ?? "USD",
                OrderDate = DateTime.UtcNow,
                ShippingAddress = createOrderDto.ShippingAddress,
                BillingAddress = createOrderDto.BillingAddress,
                UserId = createOrderDto.UserId,
                OrderItems = orderItems,
                Notes = createOrderDto.Notes ?? string.Empty,
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };

            // Reserve inventory for each item
            foreach (var item in orderItems)
            {
                try
                {
                    await _inventoryService.ReserveStockAsync(item.VariantId, item.Quantity);
                }
                catch (Exception inventoryEx)
                {
                    _logger.LogWarning(inventoryEx, "Could not reserve stock for variant {VariantId}", item.VariantId);
                }
            }

            var result = await _orderRepository.AddAsync(order);
            await _orderRepository.SaveChangesAsync();

            _logger.LogInformation("Order created successfully - OrderId: {OrderId}, OrderNumber: {OrderNumber}, Total: {Total}",
                order.Id, order.OrderNumber, order.TotalAmount);

            // Clear cart after successful order creation
            await _cartRepository.ClearCartAsync(createOrderDto.UserId);

            // Send notification
            try
            {
                await _notificationService.SendOrderConfirmationAsync(order.Id);
            }
            catch (Exception notificationEx)
            {
                _logger.LogWarning(notificationEx, "Failed to send order confirmation for order {OrderId}", order.Id);
            }

            result.State = EntityState.Detached;
            var orderDto = MapToOrderDto(order);

            return OperationResult<OrderDto>.Success(orderDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating order from cart for user {UserId}", createOrderDto.UserId);
            return OperationResult<OrderDto>.Fail(ex.Message);
        }
    }

    public async Task<OperationResult<PagedResultDto<OrderDto>>> GetOrderHistoryAsync(string userId, int page = 1, int pageSize = 10, OrderStatus? status = null)
    {
        try
        {
            var pagedOrders = await _orderRepository.GetPagedAsync(page, pageSize, q =>
            {
                var query = q.Where(o => o.UserId == userId && !o.IsDeleted);

                if (status.HasValue)
                {
                    query = query.Where(o => o.OrderStatus == status.Value);
                }

                return query.Include(o => o.User)
                           .Include(o => o.OrderItems)
                           .Include(o => o.Payments)
                           .Include(o => o.OrderCoupons)
                           .OrderByDescending(o => o.OrderDate);
            });

            var result = MapToPagedOrderDto(pagedOrders);
            return OperationResult<PagedResultDto<OrderDto>>.Success(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting order history for user {UserId}", userId);
            return OperationResult<PagedResultDto<OrderDto>>.Fail(ex.Message);
        }
    }

    public async Task<OperationResult<object>> GetOrderTrackingAsync(Guid id)
    {
        try
        {
            var order = await _orderRepository.GetByIdAsync(id, o => o.User);

            if (order == null)
            {
                return OperationResult<object>.Fail($"Order with id {id} was not found.");
            }

            var trackingInfo = new
            {
                OrderId = order.Id,
                OrderNumber = order.OrderNumber,
                Status = order.OrderStatus.ToString(),
                OrderDate = order.OrderDate,
                ShippedDate = order.ShippedDate,
                DeliveredDate = order.DeliveredDate,
                EstimatedDelivery = CalculateEstimatedDelivery(order.OrderDate, order.OrderStatus),
                TrackingSteps = GetTrackingSteps(order.OrderStatus, order.OrderDate, order.ShippedDate, order.DeliveredDate),
                ShippingAddress = DeserializeAddress(order.ShippingAddress),
                TotalAmount = order.TotalAmount,
                Currency = order.Currency
            };

            return OperationResult<object>.Success(trackingInfo);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting order tracking for order {OrderId}", id);
            return OperationResult<object>.Fail(ex.Message);
        }
    }

    #endregion

    #region Helper Methods for Tracking

    private DateTime? CalculateEstimatedDelivery(DateTime orderDate, OrderStatus status)
    {
        return status switch
        {
            OrderStatus.Pending => orderDate.AddDays(7),
            OrderStatus.Processing => orderDate.AddDays(5),
            OrderStatus.Shipped => orderDate.AddDays(3),
            OrderStatus.Delivered => null,
            OrderStatus.Cancelled => null,
            _ => orderDate.AddDays(7)
        };
    }

    private List<object> GetTrackingSteps(OrderStatus currentStatus, DateTime orderDate, DateTime? shippedDate, DateTime? deliveredDate)
    {
        var steps = new List<object>
        {
            new { Step = "Order Placed", Status = "Completed", Date = orderDate, IsCompleted = true },
            new { Step = "Processing", Status = GetStepStatus(currentStatus, OrderStatus.Processing), Date = orderDate.AddDays(1), IsCompleted = (int)currentStatus >= (int)OrderStatus.Processing },
            new { Step = "Shipped", Status = GetStepStatus(currentStatus, OrderStatus.Shipped), Date = shippedDate, IsCompleted = (int)currentStatus >= (int)OrderStatus.Shipped },
            new { Step = "Delivered", Status = GetStepStatus(currentStatus, OrderStatus.Delivered), Date = deliveredDate, IsCompleted = currentStatus == OrderStatus.Delivered }
        };

        return steps;
    }

    private string GetStepStatus(OrderStatus currentStatus, OrderStatus stepStatus)
    {
        if (currentStatus == OrderStatus.Cancelled)
            return "Cancelled";

        if ((int)currentStatus >= (int)stepStatus)
            return "Completed";

        if ((int)currentStatus == (int)stepStatus - 1)
            return "In Progress";

        return "Pending";
    }

    #endregion

    #region Existing Methods (Updated with Manual Mapping)

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

        var result = MapToPagedOrderDto(pagedOrders);
        return OperationResult<PagedResultDto<OrderDto>>.Success(result);
    }

    public async Task<OperationResult<IEnumerable<OrderDto>>> GetOrdersByUserIdAsync(string userId)
    {
        try
        {
            var orders = await _orderRepository.GetOrdersByUserIdAsync(userId);
            var orderDtos = MapToOrderDtoList(orders);
            return OperationResult<IEnumerable<OrderDto>>.Success(orderDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting orders for user {UserId}", userId);
            return OperationResult<IEnumerable<OrderDto>>.Fail(ex.Message);
        }
    }

    public async Task<OperationResult<OrderDto>> GetOrderByUserIdAsync(string userId)
    {
        try
        {
            var orders = await _orderRepository.GetOrdersByUserIdAsync(userId);
            var order = orders.FirstOrDefault();

            if (order == null)
                return OperationResult<OrderDto>.Fail("No order found for this user.");

            var orderDto = MapToOrderDto(order);
            return OperationResult<OrderDto>.Success(orderDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting order for user {UserId}", userId);
            return OperationResult<OrderDto>.Fail(ex.Message);
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

            var orderDto = MapToOrderDto(order);
            return OperationResult<OrderDto>.Success(orderDto);
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
            var orderDtos = MapToOrderDtoList(orders);
            return OperationResult<IEnumerable<OrderDto>>.Success(orderDtos);
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

            // Update dates based on status
            switch (newStatus)
            {
                case OrderStatus.Shipped:
                    order.ShippedDate = DateTime.UtcNow;
                    break;
                case OrderStatus.Delivered:
                    order.DeliveredDate = DateTime.UtcNow;
                    break;
            }

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

            var orderDto = MapToOrderDto(order);
            return OperationResult<OrderDto>.Success(orderDto);
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
                return OperationResult<bool>.Fail($"Order with id {orderId} cannot be cancelled.");
            }

            // Release inventory
            foreach (var item in order.OrderItems)
            {
                try
                {
                    await _inventoryService.ReleaseStockAsync(item.VariantId, item.Quantity);
                }
                catch (Exception inventoryEx)
                {
                    _logger.LogWarning(inventoryEx, "Could not release stock for variant {VariantId}", item.VariantId);
                    // Continue without stock release if method doesn't exist
                }
            }

            order.OrderStatus = OrderStatus.Cancelled;
            order.Notes = string.IsNullOrEmpty(order.Notes) ? reason : $"{order.Notes}; Cancelled: {reason}";

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

            var summary = new OrderSummaryDto
            {
                TotalAmount = totalSales,
                ItemCount = orders.Count()
            };

            return OperationResult<OrderSummaryDto>.Success(summary);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting order summary");
            return OperationResult<OrderSummaryDto>.Fail(ex.Message);
        }
    }

    private async Task<OperationResult<string>> GenerateOrderNumberAsync()
    {
        const int maxRetries = 10;

        for (int attempt = 0; attempt < maxRetries; attempt++)
        {
            string orderNumber;

            if (attempt < 5)
            {
                // First 5 attempts: Use timestamp with milliseconds
                orderNumber = $"ADI{DateTime.UtcNow:yyyyMMddHHmmssfff}";
            }
            else
            {
                // Last 5 attempts: Use date + random suffix for better uniqueness
                var randomSuffix = new Random().Next(1000, 9999);
                orderNumber = $"ADI{DateTime.UtcNow:yyyyMMdd}{randomSuffix}";
            }

            try
            {
                // Check if this order number already exists in database
                var existingOrder = await _orderRepository.GetByOrderNumberAsync(orderNumber);

                if (existingOrder == null)
                {
                    return OperationResult<string>.Success(orderNumber);
                }

                // If duplicate found, log and try again
                _logger.LogWarning("Duplicate order number {OrderNumber} found on attempt {Attempt}", orderNumber, attempt + 1);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error checking order number uniqueness on attempt {Attempt}", attempt + 1);
            }

            // Wait a small random time before retry to reduce collision probability
            await Task.Delay(Random.Shared.Next(10, 50));
        }

        // Fallback: Use GUID-based number if all attempts failed
        var fallbackNumber = $"ADI{DateTime.UtcNow:yyyyMMdd}{Guid.NewGuid().ToString("N")[..8].ToUpper()}";

        _logger.LogWarning("All order number generation attempts failed, using fallback: {OrderNumber}", fallbackNumber);

        return OperationResult<string>.Success(fallbackNumber);
    }

    // Also add this method to your OrderRepository if it doesn't exist
   

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

    public async Task<OperationResult<object>> PlaceOrder(CreateOrderDTO orderCreateDTO)
    {
        var cartItems = await _cartRepository.GetCartItemsByUserIdAsync(orderCreateDTO.UserId);

        if (cartItems == null || !cartItems.Any())
        {
            return OperationResult<object>.Fail("Cart is empty, cannot place order.");
        }

        // Calculate amounts
        decimal subtotal = cartItems.Sum(item =>
        {
            var variant = item.Variant;
            var product = variant.Product;
            return item.Quantity * (product.Price + variant.PriceAdjustment);
        });

        decimal taxAmount = subtotal * 0.1m;  // Example 10% tax
        decimal shippingAmount = 20m;         // Example flat shipping fee
        decimal discountAmount = 0m;          // Apply coupon logic later
        decimal totalAmount = subtotal + taxAmount + shippingAmount - discountAmount;

        var order = new Order
        {
            BillingAddress = orderCreateDTO.BillingAddress,
            ShippingAddress = orderCreateDTO.ShippingAddress,
            Currency = orderCreateDTO.Currency,
            OrderStatus = OrderStatus.Pending,
            OrderNumber = Guid.NewGuid().ToString("N"),
            OrderDate = DateTime.UtcNow,
            UserId = orderCreateDTO.UserId,
            Notes = "",
            Subtotal = subtotal,
            TaxAmount = taxAmount,
            ShippingAmount = shippingAmount,
            DiscountAmount = discountAmount,
            TotalAmount = totalAmount
        };

        // Add OrderItems
        foreach (var cartItem in cartItems)
        {
            var variant = cartItem.Variant;
            var product = variant.Product;

            var variantDetails = $"Size: {variant.Size}, Color: {variant.Color}";

            order.OrderItems.Add(new OrderItem
            {
                OrderId = order.Id,
                Quantity = cartItem.Quantity,
                UnitPrice = product.Price + variant.PriceAdjustment,
                TotalPrice = cartItem.Quantity * (product.Price + variant.PriceAdjustment),
                ProductName = product.Name,
                VariantDetails = variantDetails,
                VariantId = variant.Id
            });
        }

        // Save Order
        await _orderRepository.AddAsync(order);

        // Clear the cart
        await _cartRepository.ClearCartAsync(orderCreateDTO.UserId);

        // Save changes (repository should call DbContext.SaveChangesAsync inside)
        await _orderRepository.SaveChangesAsync();

        return OperationResult<object>.Success(new { OrderId = order.Id, OrderNumber = order.OrderNumber });
    }

    #endregion
}