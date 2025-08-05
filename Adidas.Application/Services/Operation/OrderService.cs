using Adidas.Application.Contracts.RepositoriesContracts.Feature;
using Adidas.Application.Contracts.RepositoriesContracts.Main;
using Adidas.Application.Contracts.RepositoriesContracts.Operation;
using Adidas.Application.Contracts.RepositoriesContracts.People;
using Adidas.Application.Contracts.ServicesContracts.Static;
using Adidas.Application.Contracts.ServicesContracts.Tracker;
using Adidas.Application.Services;
using Adidas.DTOs.Operation.OrderDTOs.Calculation;
using Adidas.DTOs.Operation.OrderDTOs.Create;
using Adidas.DTOs.Operation.OrderDTOs.Query;
using Adidas.DTOs.Operation.OrderDTOs.Result;
using Adidas.DTOs.Operation.OrderDTOs.Update;
using Adidas.DTOs.Operation.PaymentDTOs.Result;
using Adidas.Models.Feature;
using AutoMapper;
using Microsoft.Extensions.Logging;
using Models.Feature;
using System.Transactions;

namespace Adidas.Application.Contracts.ServicesContracts.Operation.OrderItemServices
{
    public class OrderService : GenericService<Order, OrderDto, CreateOrderDto, UpdateOrderDto>, IOrderService
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IShoppingCartRepository _cartRepository;
        private readonly IProductVariantRepository _variantRepository;
        private readonly IInventoryService _inventoryService;
        private readonly INotificationService _notificationService;

        public OrderService(
            IOrderRepository orderRepository,
            IShoppingCartRepository cartRepository,
            IProductVariantRepository variantRepository,
            IInventoryService inventoryService,
            INotificationService notificationService,
            IMapper mapper,
            ILogger<OrderService> logger)
            : base(orderRepository, mapper, logger)
        {
            _orderRepository = orderRepository;
            _cartRepository = cartRepository;
            _variantRepository = variantRepository;
            _inventoryService = inventoryService;
            _notificationService = notificationService;
        }

        public async Task<IEnumerable<OrderDto>> GetOrdersByUserIdAsync(string userId)
        {
            var orders = await _orderRepository.GetOrdersByUserIdAsync(userId);
            return _mapper.Map<IEnumerable<OrderDto>>(orders);
        }

        public async Task<OrderDto?> GetOrderByOrderNumberAsync(string orderNumber)
        {
            var order = await _orderRepository.GetOrderByNumberAsync(orderNumber);
            return order == null ? null : _mapper.Map<OrderDto>(order);
        }

        public async Task<IEnumerable<OrderDto>> GetOrdersByStatusAsync(OrderStatus status)
        {
            var orders = await _orderRepository.GetOrdersByStatusAsync(status);
            return _mapper.Map<IEnumerable<OrderDto>>(orders);
        }

        public async Task<OrderDto> CreateOrderFromCartAsync(string userId, CreateOrderFromCartDto orderDto)
        {
            var cartItems = await _cartRepository.GetCartItemsByUserIdAsync(userId);
            if (!cartItems.Any())
                throw new InvalidOperationException("Cart is empty");

            // Reserve inventory
            foreach (var item in cartItems)
            {
                var reserved = await _inventoryService.ReserveStockAsync(item.VariantId, item.Quantity);
                if (!reserved)
                    throw new InvalidOperationException($"Insufficient stock for variant {item.VariantId}");
            }

            try
            {
                var order = new Order
                {
                    Id = Guid.NewGuid(),
                    UserId = userId,
                    OrderNumber = await GenerateOrderNumberAsync(),
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
                order.TotalAmount = order.Subtotal  + order.TaxAmount;

                // Apply discount if provided
                if (!string.IsNullOrEmpty(orderDto.DiscountCode))
                {
                    // Implementation would validate and apply discount
                    // order.DiscountAmount = await CalculateDiscountAsync(orderDto.DiscountCode, order.SubTotal);
                    // order.TotalAmount -= order.DiscountAmount;
                }

                var createdOrder = await _repository.AddAsync(order);

                // Clear cart
                await _cartRepository.ClearCartAsync(userId);

                await _notificationService.SendOrderConfirmationAsync(createdOrder.Entity.Id);

                return _mapper.Map<OrderDto>(createdOrder);
            }
            catch
            {
                // Release reserved inventory on failure
                foreach (var item in cartItems)
                {
                    await _inventoryService.ReleaseStockAsync(item.VariantId, item.Quantity);
                }
                throw;
            }
        }

        public async Task<bool> UpdateOrderStatusAsync(Guid orderId, OrderStatus newStatus)
        {
            var order = await _orderRepository.GetByIdAsync(orderId);
            if (order == null) return false;

            order.OrderStatus = newStatus;
            await _repository.UpdateAsync(order);

            await _notificationService.SendOrderStatusUpdateAsync(orderId);
            return true;
        }

        public async Task<OrderDto?> GetOrderWithItemsAsync(Guid orderId)
        {
            var order = await _orderRepository.GetOrderWithItemsAsync(orderId);
            return order == null ? null : _mapper.Map<OrderDto>(order);
        }

        public async Task<decimal> CalculateOrderTotalAsync(string userId, string? discountCode = null)
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

            return total;
        }

        public async Task<bool> CancelOrderAsync(Guid orderId, string reason)
        {
            var order = await _orderRepository.GetOrderWithItemsAsync(orderId);
            if (order == null || order.OrderStatus != OrderStatus.Pending)
                return false;

            // Release inventory
            foreach (var item in order.OrderItems)
            {
                await _inventoryService.ReleaseStockAsync(item.VariantId, item.Quantity);
            }

            order.OrderStatus = OrderStatus.Cancelled;
           
            await _repository.UpdateAsync(order);

            return true;
        }

        public async Task<OrderSummaryDto> GetOrderSummaryAsync(DateTime? startDate = null, DateTime? endDate = null)
        {
            var totalSales = await _orderRepository.GetTotalSalesAsync(startDate, endDate);
            var orders = await _orderRepository.GetOrdersByDateRangeAsync(
                startDate ?? DateTime.UtcNow.AddDays(-30),
                endDate ?? DateTime.UtcNow);

            return new OrderSummaryDto
            {
                TotalAmount = totalSales,
                ItemCount = orders.Count()
                 
            };
        }

        private async Task<string> GenerateOrderNumberAsync()
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

            return orderNumber;
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
}
