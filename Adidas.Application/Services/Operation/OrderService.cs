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
using Adidas.Models.Tracker;
using ClosedXML.Excel;
using DocumentFormat.OpenXml.Office.CustomUI;
using iTextSharp.text;
using iTextSharp.text.pdf;
using Mapster;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Models.Feature;
using Models.People;
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
    private readonly ICouponRepository _couponRepository;
    private readonly ILogger<OrderService> _logger;
    private readonly AdidasDbContext _context;

    public OrderService(
        ICouponRepository couponRepository,
        IOrderRepository orderRepository,
        IOrderItemRepository orderItemRepository,
        IShoppingCartRepository cartRepository,
        IProductVariantRepository variantRepository,
        IInventoryService inventoryService,
        INotificationService notificationService,
        ICouponService couponService,
        ILogger<OrderService> logger, AdidasDbContext context) : base(orderRepository, logger)
    {
        _orderRepository = orderRepository;
        _cartRepository = cartRepository;
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
            ShippingAddress = FormatAddress(order.ShippingAddress),
            BillingAddress = FormatAddress(order.BillingAddress),
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
    private OrderDetailDto MapToOrderDetailDto(Order order)
    {
        if (order == null) return null;

        return new OrderDetailDto
        {
            // Base properties from OrderDto
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
            ShippingAddress = FormatAddress(order.ShippingAddress),
            BillingAddress = FormatAddress(order.BillingAddress),
            Notes = order.Notes,
            UserId = order.UserId,
            UserName = order.User?.UserName,
            UserEmail = order.User?.Email,


            // Collections from base OrderDto
            Payments = order.Payments?.Select(MapToPaymentDto).ToList() ?? new List<PaymentDto>(),
            OrderCoupons = order.OrderCoupons?.Select(MapToOrderCouponDto).ToList() ?? new List<OrderCouponDto>(),

            // Additional detailed property - OrderItems
            OrderItems = order.OrderItems?.Select(item => new OrderItemDto
            {
                Id = item.Id,
                VariantId = item.VariantId,
                ProductName = item.Variant?.Product?.Name ?? "Unknown Product",
                Sku = item.Variant?.Sku ?? "",
                Size = item.Variant?.Size ?? "",
                Color = item.Variant?.Color ?? "",
                ImageUrl = item.Variant?.ImageUrl ?? "",
                UnitPrice = item.UnitPrice,
                Quantity = item.Quantity,
                TotalPrice = item.TotalPrice,
                VariantDetails = $"Size: {item.Variant?.Size}, Color: {item.Variant?.Color}"
            }).ToList() ?? new List<OrderItemDto>()
        };
    }
    private List<OrderDetailDto> MapToOrderDetailDto(IEnumerable<Order> orders)
    {
        if (orders == null) return new List<OrderDetailDto>();

        return orders.Select(MapToOrderDetailDto).ToList();
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
        if (payment == null) return null;

        return new PaymentDto
        {
            Id = payment.Id,
            PaymentMethod = payment.PaymentMethod,
            PaymentStatus = payment.PaymentStatus,
            Amount = payment.Amount,
            TransactionId = payment.TransactionId,
            GatewayResponse = payment.GatewayResponse,
            ProcessedAt = payment.ProcessedAt,
            OrderId = payment.OrderId,
            IsActive = payment.IsActive,
            UpdatedAt = payment.UpdatedAt,
        };
    }

    private OrderCouponDto MapToOrderCouponDto(OrderCoupon orderCoupon)
    {
        // Implementation depends on your OrderCoupon entity structure
        // Return a basic mapping for now
        return new OrderCouponDto();
    }

    private Dictionary<string, object> DeserializeAddress(string addressJson)
    {
        if (string.IsNullOrWhiteSpace(addressJson))
            return new Dictionary<string, object>();

        if (addressJson.TrimStart().StartsWith("{") || addressJson.TrimStart().StartsWith("["))
        {
            try
            {
                return JsonSerializer.Deserialize<Dictionary<string, object>>(addressJson)
                       ?? new Dictionary<string, object>();
            }
            catch
            {
                return new Dictionary<string, object>
            {
                { "RawAddress", addressJson }
            };
            }
        }

        return new Dictionary<string, object>
    {
        { "RawAddress", addressJson }
    };
    }
    private string FormatAddress(string addressJson)
    {
        try
        {
            if (string.IsNullOrEmpty(addressJson))
                return "Address not available";

            if (addressJson.TrimStart().StartsWith("{") || addressJson.TrimStart().StartsWith("["))
            {
                var addressDict = JsonSerializer.Deserialize<Dictionary<string, object>>(addressJson);
                var parts = new List<string>();

                if (addressDict.TryGetValue("name", out var name)) parts.Add(name?.ToString());
                if (addressDict.TryGetValue("street", out var street)) parts.Add(street?.ToString());
                if (addressDict.TryGetValue("city", out var city)) parts.Add(city?.ToString());
                if (addressDict.TryGetValue("country", out var country)) parts.Add(country?.ToString());
                if (addressDict.TryGetValue("phone", out var phone)) parts.Add($"Phone: {phone}");
                if (addressDict.TryGetValue("email", out var email)) parts.Add($"Email: {email}");

                return string.Join("\n", parts.Where(p => !string.IsNullOrEmpty(p)));
            }

            return addressJson;
        }
        catch
        {
            return addressJson;
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

    private PagedResultDto<OrderDetailDto> MapToPagedOrderDto(PagedResultDto<Order> pagedOrders)
    {
        return new PagedResultDto<OrderDetailDto>
        {
            Items = pagedOrders.Items.Select(MapToOrderDetailDto).ToList(),
            TotalCount = pagedOrders.TotalCount,
            PageNumber = pagedOrders.PageNumber,
            PageSize = pagedOrders.PageSize
        };
    }


    #endregion

    #region New Required Methods

    public async Task<OperationResult<OrderDetailDto>> GetOrderByIdAsync(Guid id)
    {
        try
        {
            var order = await _orderRepository.GetOrderByOrderIdAsync(id);
            if (order == null)
            {
                return OperationResult<OrderDetailDto>.Fail($"Order with id {id} was not found.");
            }
            var orderDto = MapToOrderDetailDto(order); // New mapping method
            return OperationResult<OrderDetailDto>.Success(orderDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting order by id {OrderId}", id);
            return OperationResult<OrderDetailDto>.Fail(ex.Message);
        }
    }

    public async Task<OperationResult<OrderDto>> CreateOrderFromCartAsync(CreateOrderDTO createOrderDto)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            // Ensure guest user exists if this is a guest order
            if (createOrderDto.IsGuestUser)
            {
                var guestUserId = await EnsureGuestUserExistsAsync(createOrderDto.UserId, createOrderDto.GuestEmail);
                createOrderDto.UserId = guestUserId; // Ensure we use the confirmed guest user ID
            }

            IEnumerable<ShoppingCart> cartItems;

            // Handle cart items (guest vs registered user)
            if (createOrderDto.IsGuestUser)
            {
                if (createOrderDto.CartItems == null || !createOrderDto.CartItems.Any())
                    return OperationResult<OrderDto>.Fail("Guest cart is empty, cannot create order.");

                cartItems = createOrderDto.CartItems.Select(cartItemDto => new ShoppingCart
                {
                    VariantId = cartItemDto.VariantId,
                    Quantity = cartItemDto.Quantity,
                    UserId = createOrderDto.UserId,
                }).ToList();

                _logger.LogInformation("Processing guest cart with {Count} items for guest user {GuestUserId}",
                    cartItems.Count(), createOrderDto.UserId);
            }
            else
            {
                if (createOrderDto.CartItems != null && createOrderDto.CartItems.Any())
                {
                    cartItems = createOrderDto.CartItems.Select(cartItemDto => new ShoppingCart
                    {
                        VariantId = cartItemDto.VariantId,
                        Quantity = cartItemDto.Quantity,
                        UserId = createOrderDto.UserId
                    }).ToList();

                    _logger.LogInformation("Processing registered user cart from DTO with {Count} items for user {UserId}",
                        cartItems.Count(), createOrderDto.UserId);
                }
                else
                {
                    cartItems = await _cartRepository.GetCartItemsByUserIdAsync(createOrderDto.UserId);
                    if (cartItems == null || !cartItems.Any())
                        return OperationResult<OrderDto>.Fail("Cart is empty, cannot create order.");

                    _logger.LogInformation("Processing registered user cart from database with {Count} items for user {UserId}",
                        cartItems.Count(), createOrderDto.UserId);
                }
            }

            // Generate order ID and number
            var orderId = Guid.NewGuid();
            var orderNumberResult = await GenerateOrderNumberAsync();
            if (!orderNumberResult.IsSuccess)
                return OperationResult<OrderDto>.Fail("Failed to generate order number");

            // Calculate amounts and reserve stock
            decimal subtotal = 0;
            var orderItems = new List<OrderItem>();
            var stockReservations = new List<(Guid variantId, int quantity)>();

            foreach (var cartItem in cartItems)
            {
                var variant = await _context.ProductVariants
                    .Include(v => v.Product)
                    .FirstOrDefaultAsync(v => v.Id == cartItem.VariantId);

                if (variant?.Product == null)
                {
                    _logger.LogError("Variant or Product not found for VariantId: {VariantId}", cartItem.VariantId);
                    continue;
                }

                if (variant.StockQuantity < cartItem.Quantity)
                {
                    await transaction.RollbackAsync();
                    return OperationResult<OrderDto>.Fail(
                        $"Insufficient stock for product variant {cartItem.VariantId}. Available: {variant.StockQuantity}, Requested: {cartItem.Quantity}");
                }

                var oldQuantity = variant.StockQuantity;
                variant.StockQuantity -= cartItem.Quantity;
                _context.ProductVariants.Update(variant);

                stockReservations.Add((cartItem.VariantId, cartItem.Quantity));

                var userIdentifier = createOrderDto.IsGuestUser ? "GUEST" : createOrderDto.UserId;
                await LogInventoryChangeAsync(cartItem.VariantId, oldQuantity, variant.StockQuantity,
                    "RESERVE", userIdentifier,
                    $"Reserved {cartItem.Quantity} units for order {orderNumberResult.Data}");

                var basePrice = variant.Product.SalePrice ?? variant.Product.Price;
                var unitPrice = basePrice + variant.PriceAdjustment;
                var totalPrice = unitPrice * cartItem.Quantity;

                subtotal += totalPrice;

                orderItems.Add(new OrderItem
                {
                    Id = Guid.NewGuid(),
                    OrderId = orderId,
                    VariantId = cartItem.VariantId,
                    Quantity = cartItem.Quantity,
                    UnitPrice = unitPrice,
                    TotalPrice = totalPrice,
                    ProductName = variant.Product.Name,
                    VariantDetails = $"Size: {variant.Size}, Color: {variant.Color}",
                    CreatedAt = DateTime.UtcNow,
                    IsActive = true
                });
            }

            if (orderItems.Count == 0)
            {
                await transaction.RollbackAsync();
                return OperationResult<OrderDto>.Fail("No valid items found in cart to create order.");
            }

            if (subtotal <= 0)
            {
                await transaction.RollbackAsync();
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
                    var coupon = await _couponRepository.GetByCodeAsync(createOrderDto.CouponCode);
                    if (coupon != null && !coupon.IsDeleted)
                    {
                        if (coupon.UsageLimit == 0 || coupon.UsedCount < coupon.UsageLimit)
                        {
                            if (subtotal >= coupon.MinimumAmount)
                            {
                                discountAmount = await CalculateDiscountAmountAsync(coupon, subtotal);
                                coupon.UsedCount++;
                                _context.Coupons.Update(coupon);

                                _logger.LogInformation("Coupon applied: {Code}, Discount: {Discount}", coupon.Code, discountAmount);
                            }
                            else
                            {
                                _logger.LogInformation("Coupon {Code} not applied - order subtotal {Subtotal} below minimum {MinAmount}",
                                    coupon.Code, subtotal, coupon.MinimumAmount);
                            }
                        }
                        else
                        {
                            _logger.LogInformation("Coupon {Code} not applied - usage limit reached", coupon.Code);
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to apply coupon {CouponCode}", createOrderDto.CouponCode);
                }
            }

            var totalAmount = subtotal - discountAmount + taxAmount + shippingAmount;

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

            var result = await _orderRepository.AddAsync(order);

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            if (!createOrderDto.IsGuestUser)
            {
                try
                {
                    await _cartRepository.ClearCartAsync(createOrderDto.UserId);
                }
                catch (Exception clearCartEx)
                {
                    _logger.LogWarning(clearCartEx, "Failed to clear cart for user {UserId} after order creation", createOrderDto.UserId);
                }
            }

            await SendOrderConfirmationNotification(order, createOrderDto);

            result.State = EntityState.Detached;
            var orderDto = MapToOrderDto(order);

            return OperationResult<OrderDto>.Success(orderDto);
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex, "Error creating order from cart for user {UserId}, IsGuest: {IsGuest}",
                createOrderDto.UserId, createOrderDto.IsGuestUser);
            return OperationResult<OrderDto>.Fail(ex.Message);
        }
    }

    private async Task<decimal> CalculateDiscountAmountAsync(Coupon coupon, decimal orderAmount)
    {
        return coupon.DiscountType switch
        {
            DiscountType.Percentage => orderAmount * (coupon.DiscountValue / 100m),
            DiscountType.FixedAmount => coupon.DiscountValue, // Deduct fixed amount directly
            DiscountType.Amount => Math.Min(coupon.DiscountValue, orderAmount),
            _ => 0
        };
    }

    // Helper method for inventory logging
    private async Task<string> EnsureGuestUserExistsAsync(string guestUserId, string? guestEmail)
    {
        // Check if guest user already exists
        var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Id == guestUserId);
        if (existingUser != null)
        {
            return guestUserId;
        }

        // Create a temporary guest user record with security measures
        var guestUser = new User
        {
            Id = guestUserId,
            UserName = $"guest_{guestUserId}",
            Email = guestEmail ?? $"guest_{guestUserId}@temp.local",
            EmailConfirmed = false,
            PhoneNumberConfirmed = false,
            TwoFactorEnabled = false,
            LockoutEnabled = true, // Enable lockout for security
            LockoutEnd = DateTimeOffset.MaxValue, // Permanently locked out from login
            AccessFailedCount = int.MaxValue, // Max failed attempts to prevent login
            SecurityStamp = Guid.NewGuid().ToString(),
            ConcurrencyStamp = Guid.NewGuid().ToString(),
            NormalizedUserName = $"GUEST_{guestUserId}".ToUpper(),
            NormalizedEmail = (guestEmail ?? $"guest_{guestUserId}@temp.local").ToUpper(),
            PasswordHash = null, // No password - cannot login

            // Properties from your User model
            CreatedAt = DateTime.UtcNow,
            IsActive = true, // Inactive for login purposes but can place orders
            IsDeleted = false,
            Role = UserRole.Customer, // Guest users are customers
            PreferredLanguage = "english", // Default language

            // Guest-specific naming for clarity
            FirstName = "Guest",
            LastName = "User"
        };

        try
        {
            await _context.Users.AddAsync(guestUser);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Created secure guest user {GuestUserId} with email {Email}",
                guestUserId, guestEmail ?? "none");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create guest user {GuestUserId}", guestUserId);
            throw;
        }

        return guestUserId;
    }
    private async Task LogInventoryChangeAsync(Guid variantId, int oldQuantity, int newQuantity, string changeType, string userId, string? reason = null)
    {
        try
        {
            // Handle different user ID scenarios
            string? validUserId = null;

            if (!string.IsNullOrEmpty(userId) && userId != "GUEST")
            {
                // Check if the user actually exists in the database
                var userExists = await _context.Users.AnyAsync(u => u.Id == userId);
                if (userExists)
                {
                    validUserId = userId;
                }
            }

            var log = new InventoryLog
            {
                Id = Guid.NewGuid(),
                VariantId = variantId,
                PreviousStock = oldQuantity,
                NewStock = newQuantity,
                QuantityChange = newQuantity - oldQuantity,
                ChangeType = changeType,
                Reason = reason ?? (userId == "GUEST" ? "Guest user order" : "Order processing"),
                AddedById = validUserId, // This will be null for guest users or invalid user IDs
                CreatedAt = DateTime.UtcNow
            };

            await _context.InventoryLogs.AddAsync(log);

            _logger.LogInformation("Logged inventory change for variant {VariantId}: {OldQuantity} -> {NewQuantity} (User: {UserId})",
                variantId, oldQuantity, newQuantity, userId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to log inventory change for variant {VariantId}", variantId);
            // Don't throw - inventory logging shouldn't break order creation
        }
    }

    public async Task<OperationResult<PagedResultDto<OrderDetailDto>>> GetOrderHistoryAsync(string userId, int page = 1, int pageSize = 10, OrderStatus? status = null)
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
            return OperationResult<PagedResultDto<OrderDetailDto>>.Success(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting order history for user {UserId}", userId);
            return OperationResult<PagedResultDto<OrderDetailDto>>.Fail(ex.Message);
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

    // Add these methods to your OrderService class:
    public async Task<OperationResult<object>> GetGuestCheckoutSummaryAsync(string guestUserId, List<GuestCartItemsDto> cartItems, string? couponCode = null)
    {
        try
        {
            if (cartItems == null || !cartItems.Any())
            {
                return OperationResult<object>.Fail("Cart is empty");
            }

            decimal subtotal = 0;

            foreach (var item in cartItems)
            {
                // Get variant to validate stock and pricing
                var variant = await _variantRepository.GetByIdAsync(item.VariantId);

                if (variant?.Product == null)
                {
                    _logger.LogWarning("Product or variant not found for guest cart item: {VariantId}", item.VariantId);
                    continue;
                }

                // Check stock availability
                if (variant.StockQuantity < item.Quantity)
                {
                    return OperationResult<object>.Fail($"Insufficient stock for {variant.Product.Name}. Available: {variant.StockQuantity}, Requested: {item.Quantity}");
                }

                // Calculate price using actual product data (don't just trust frontend)
                var actualPrice = (variant.Product.SalePrice ?? variant.Product.Price) + variant.PriceAdjustment;
                subtotal += actualPrice * item.Quantity;
            }

            var shipping = subtotal >= 100 ? 0 : 10;
            var total = subtotal + shipping;

            decimal discount = 0;
            if (!string.IsNullOrEmpty(couponCode))
            {
                try
                {
                    var discountedTotal = await _couponService.CalculateCouponAmountAsync(couponCode, subtotal);
                    discount = subtotal - discountedTotal;
                    total -= discount;
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to apply coupon {CouponCode} for guest", couponCode);
                    // Continue without discount if coupon fails
                }
            }

            var response = new
            {
                Subtotal = $"EGP {subtotal:N2}",
                Delivery = shipping == 0 ? "Free" : $"EGP {shipping:N2}",
                Message = shipping == 0 ? "You unlocked Free Shipping!" : "",
                Discount = discount > 0 ? $"EGP {discount:N2}" : null,
                Total = $"EGP {total:N2}",
                ItemCount = cartItems.Count,
                GuestUserId = guestUserId
            };

            return OperationResult<object>.Success(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating guest checkout summary");
            return OperationResult<object>.Fail(ex.Message);
        }
    }

    public async Task<BillingSummaryDto> GetGuestBillingSummaryAsync(string guestUserId, List<GuestCartItemsDto> cartItems, string? promoCode = null)
    {
        try
        {
            if (cartItems == null || !cartItems.Any())
            {
                return new BillingSummaryDto
                {
                    Subtotal = 0,
                    Shipping = 0,
                    ShippingText = "Free",
                    Discount = 0,
                    Total = 0
                };
            }

            decimal itemsTotal = 0;

            foreach (var item in cartItems)
            {
                // Get actual variant data to ensure pricing accuracy
                var variant = await _variantRepository.GetByIdAsync(item.VariantId);

                if (variant?.Product == null)
                {
                    _logger.LogWarning("Variant or Product not found for guest billing - VariantId: {VariantId}", item.VariantId);
                    continue;
                }

                // Use actual database pricing, not frontend values
                var price = (variant.Product.SalePrice ?? variant.Product.Price) + variant.PriceAdjustment;
                itemsTotal += price * item.Quantity;
            }

            decimal shipping = itemsTotal >= 100 ? 0 : 10;
            string shippingText = shipping == 0 ? "Free" : $"EGP {shipping:N2}";

            decimal discount = 0;

            if (!string.IsNullOrEmpty(promoCode))
            {
                try
                {
                    var discountedTotal = await _couponService.CalculateCouponAmountAsync(promoCode, itemsTotal);
                    discount = itemsTotal - discountedTotal;
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to apply promo code {PromoCode} for guest billing", promoCode);
                    // Continue without discount if promo code fails
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
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating guest billing summary");
            throw; // Let the controller handle the exception
        }
    }


    #endregion

    #region Helper Methods for Tracking

    // Helper method for processing registered user cart items
    private async Task<OrderItem?> ProcessRegisteredUserCartItem(ShoppingCart cartItem, Guid orderId, string orderNumber, string userId, List<(Guid variantId, int quantity)> stockReservations)
    {
        _logger.LogInformation("Processing cart item - VariantId: {VariantId}, Quantity: {Quantity}",
            cartItem.VariantId, cartItem.Quantity);

        var variant = await _context.ProductVariants
            .Include(v => v.Product)
            .FirstOrDefaultAsync(v => v.Id == cartItem.VariantId);

        if (variant?.Product == null)
        {
            _logger.LogError("Variant or Product not found for VariantId: {VariantId}", cartItem.VariantId);
            return null;
        }

        // Check and reserve stock
        if (variant.StockQuantity < cartItem.Quantity)
        {
            throw new InvalidOperationException($"Insufficient stock for product variant {cartItem.VariantId}. Available: {variant.StockQuantity}, Requested: {cartItem.Quantity}");
        }

        // Reserve stock
        var oldQuantity = variant.StockQuantity;
        variant.StockQuantity -= cartItem.Quantity;
        _context.ProductVariants.Update(variant);
        stockReservations.Add((cartItem.VariantId, cartItem.Quantity));

        // Log inventory change
        await LogInventoryChangeAsync(cartItem.VariantId, oldQuantity, variant.StockQuantity,
            "RESERVE", userId, $"Reserved {cartItem.Quantity} units for order {orderNumber}");

        // Calculate pricing
        var basePrice = variant.Product.SalePrice ?? variant.Product.Price;
        var unitPrice = basePrice + variant.PriceAdjustment;
        var totalPrice = unitPrice * cartItem.Quantity;

        return new OrderItem
        {
            Id = Guid.NewGuid(),
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
    }

    // Helper method for sending notifications
    private async Task SendOrderConfirmationNotification(Order order, CreateOrderDTO createOrderDto)
    {
        try
        {
            if (createOrderDto.IsGuestUser && !string.IsNullOrEmpty(createOrderDto.GuestEmail))
            {
                // Send guest order confirmation to provided email
                // await _notificationService.SendGuestOrderConfirmationAsync(order.Id, createOrderDto.GuestEmail);
            }
            else if (!createOrderDto.IsGuestUser)
            {
                await _notificationService.SendOrderConfirmationAsync(order.Id);
            }
        }
        catch (Exception notificationEx)
        {
            _logger.LogWarning(notificationEx, "Failed to send order confirmation for order {OrderId}", order.Id);
        }
    }
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

    public async Task<OperationResult<PagedResultDto<OrderDetailDto>>> GetPagedOrdersAsync(int pageNumber, int pageSize,
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
        return OperationResult<PagedResultDto<OrderDetailDto>>.Success(result);
    }

    public async Task<OperationResult<IEnumerable<OrderDetailDto>>> GetAllOrdersByUserIdAsync(string userId)
    {
        try
        {
            var orders = await _orderRepository.GetOrdersByUserIdAsync(userId);
            var orderDtos = MapToOrderDetailDto(orders);
            return OperationResult<IEnumerable<OrderDetailDto>>.Success(orderDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting orders for user {UserId}", userId);
            return OperationResult<IEnumerable<OrderDetailDto>>.Fail(ex.Message);
        }
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
    public async Task<OperationResult<bool>> UpdateOrderAmountAsync(Guid orderId, decimal newAmount)
    {
        try
        {
            var order = await _orderRepository.GetByIdAsync(orderId);
            if (order == null)
            {
                return OperationResult<bool>.Fail($"Order with id {orderId} was not found.");
            }
            order.TotalAmount = newAmount;
            var result = await _orderRepository.UpdateAsync(order);
            await _orderRepository.SaveChangesAsync();
            result.State = EntityState.Detached;
            return OperationResult<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating order amount");
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
                var existingOrder = await _orderRepository.GetOrderByNumberAsync(orderNumber);

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
        return subtotal * 0.05m; // 8% tax
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
    public class OrderDetailDto : OrderDto
    {
        public List<OrderItemDto> OrderItems { get; set; } = new();
    }
    public class OrderItemDto
    {
        public Guid Id { get; set; }
        public Guid VariantId { get; set; }
        public string ProductName { get; set; }
        public string Sku { get; set; }
        public string Size { get; set; }
        public string Color { get; set; }
        public string ImageUrl { get; set; }
        public decimal UnitPrice { get; set; }
        public int Quantity { get; set; }
        public decimal TotalPrice { get; set; }
        public string VariantDetails { get; set; }
    }
    #endregion

}