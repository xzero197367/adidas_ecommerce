using Adidas.Application.Contracts.RepositoriesContracts.Feature;
using Adidas.Application.Contracts.RepositoriesContracts.Main;
using Adidas.Application.Contracts.RepositoriesContracts.Operation;
using Adidas.Application.Contracts.RepositoriesContracts.People;
using Adidas.DTOs.Operation.OrderDTOs.Calculation;
using Adidas.DTOs.Operation.OrderDTOs.Create;
using Adidas.DTOs.Operation.OrderDTOs.Query;
using Adidas.DTOs.Operation.OrderDTOs.Result;
using Adidas.DTOs.Operation.OrderDTOs.Update;
using Adidas.Models.Feature;
using Adidas.Models.Operation;
using AutoMapper;
using Models.Feature;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;

namespace Adidas.Application.Contracts.ServicesContracts.Operation.OrderItemServices
{
    public class OrderService : IOrderService
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IOrderItemRepository _orderItemRepository;
        private readonly IProductVariantRepository _variantRepository;
        private readonly IUserRepository _userRepository;
        private readonly ICouponRepository _couponRepository;
        private readonly IOrderCouponRepository _orderCouponRepository;
        private readonly IMapper _mapper;

        public OrderService(IOrderRepository orderRepository,
            IOrderItemRepository orderItemRepository,
            IProductVariantRepository variantRepository,
            IUserRepository userRepository,
            ICouponRepository couponRepository,
            IOrderCouponRepository orderCouponRepository,
            IMapper mapper)
        {
            _orderRepository = orderRepository;
            _orderItemRepository = orderItemRepository;
            _variantRepository = variantRepository;
            _userRepository = userRepository;
            _couponRepository = couponRepository;
            _orderCouponRepository = orderCouponRepository;
            _mapper = mapper;
        }

        public async Task<OrderCalculationDto> CalculateOrderAsync(CreateOrderDto createOrderDto)
        {
            var calculation = new OrderCalculationDto();
            var items = new List<OrderItemCalculationDto>();
            decimal subtotal = 0;

            foreach (var itemDto in createOrderDto.OrderItems)
            {
                var variant = await _variantRepository.GetByIdAsync(itemDto.VariantId);
                if (variant == null) continue;

                var itemCalculation = new OrderItemCalculationDto
                {
                    VariantId = itemDto.VariantId,
                    ProductName = itemDto.ProductName,
                    VariantDetails = itemDto.VariantDetails,
                    Quantity = itemDto.Quantity,
                    UnitPrice = variant.PriceAdjustment,
                    TotalPrice = variant.PriceAdjustment * itemDto.Quantity,
                    IsAvailable = variant.StockQuantity >= itemDto.Quantity,
                    AvailableStock = variant.StockQuantity
                };

                items.Add(itemCalculation);
                subtotal += itemCalculation.TotalPrice;
            }

            calculation.Items = items;
            calculation.Subtotal = subtotal;
            calculation.TaxAmount = subtotal * 0.14m;
            calculation.ShippingAmount = subtotal > 500 ? 0 : 50;

            decimal totalDiscount = 0;
            var appliedCoupons = new List<AppliedCouponDto>();

            foreach (var code in createOrderDto.CouponCodes)
            {
                var coupon = await _couponRepository.GetByCodeAsync(code);

                if (coupon == null)
                {
                    appliedCoupons.Add(new AppliedCouponDto
                    {
                        CouponCode = code,
                        IsValid = false,
                        ErrorMessage = "Coupon not found"
                    });
                    continue;
                }

                if (!coupon.IsActive || coupon.ValidFrom > DateTime.UtcNow || coupon.ValidTo < DateTime.UtcNow)
                {
                    appliedCoupons.Add(new AppliedCouponDto
                    {
                        CouponCode = code,
                        IsValid = false,
                        ErrorMessage = "Coupon is not valid at this time"
                    });
                    continue;
                }

                if (coupon.UsageLimit > 0 && coupon.UsedCount >= coupon.UsageLimit)
                {
                    appliedCoupons.Add(new AppliedCouponDto
                    {
                        CouponCode = code,
                        IsValid = false,
                        ErrorMessage = "Coupon usage limit exceeded"
                    });
                    continue;
                }

                if (coupon.MinimumAmount > subtotal)
                {
                    appliedCoupons.Add(new AppliedCouponDto
                    {
                        CouponCode = code,
                        IsValid = false,
                        ErrorMessage = $"Minimum amount required is {coupon.MinimumAmount}"
                    });
                    continue;
                }

                decimal discountAmount = 0;

                if (coupon.DiscountType == DiscountType.Percentage)
                {
                    discountAmount = subtotal * (coupon.DiscountValue / 100m);
                }
                else if (coupon.DiscountType == DiscountType.FixedAmount)
                {
                    discountAmount = coupon.DiscountValue;
                }

                totalDiscount += discountAmount;

                appliedCoupons.Add(new AppliedCouponDto
                {
                    CouponCode = coupon.Code,
                    DiscountAmount = discountAmount,
                    DiscountType = coupon.DiscountType.ToString(),
                    IsValid = true
                });
            }

            calculation.AppliedCoupons = appliedCoupons;
            calculation.DiscountAmount = totalDiscount;
            calculation.TotalAmount = calculation.Subtotal + calculation.TaxAmount + calculation.ShippingAmount - calculation.DiscountAmount;

            return calculation;
        }

        public async Task<bool> CancelOrderAsync(Guid orderId, string reason = null)
        {
            var order = await _orderRepository.GetOrderWithItemsAsync(orderId);
            if (order == null || order.OrderStatus != OrderStatus.Pending)
            {
                return false; // Order not found or cannot be cancelled
            }
            if (order.OrderStatus == OrderStatus.Delivered || order.OrderStatus == OrderStatus.Cancelled)
            {
                return false;
            }
            using var transaction = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);
            order.OrderStatus = OrderStatus.Cancelled;
            order.Notes = reason ?? "Order cancelled by user";
            order.OrderDate = DateTime.UtcNow;
            await _orderRepository.UpdateAsync(order);
            foreach (var item in order.OrderItems)
            {
                var variant = await _variantRepository.GetByIdAsync(item.VariantId);
                if (variant != null)
                {
                    variant.StockQuantity += item.Quantity;
                    await _variantRepository.UpdateAsync(variant);
                }
            }

            transaction.Complete();
            return true;

        }
        public async Task<OrderDto> CreateOrderAsync(CreateOrderDto createOrderDto)
        {
            var user = await _userRepository.GetByIdAsync(createOrderDto.UserId);
            if (user == null)
                throw new ArgumentException("User not found");

            var calculation = await CalculateOrderAsync(createOrderDto);

            using var transaction = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);

            var order = new Order
            {
                Id = Guid.NewGuid(),
                OrderNumber = $"ORD-{Guid.NewGuid().ToString().Substring(0, 8).ToUpper()}",
                OrderStatus = OrderStatus.Pending,
                UserId = createOrderDto.UserId,
                Currency = createOrderDto.Currency,
                Subtotal = calculation.Subtotal,
                TaxAmount = calculation.TaxAmount,
                ShippingAmount = calculation.ShippingAmount,
                DiscountAmount = calculation.DiscountAmount,
                TotalAmount = calculation.TotalAmount,
                OrderDate = DateTime.UtcNow,
                ShippingAddress = createOrderDto.ShippingAddress,
                BillingAddress = createOrderDto.BillingAddress,
                Notes = createOrderDto.Notes
            };

            await _orderRepository.AddAsync(order);

            foreach (var itemDto in createOrderDto.OrderItems)
            {
                var variant = await _variantRepository.GetByIdAsync(itemDto.VariantId);
                if (variant == null || variant.StockQuantity < itemDto.Quantity)
                    throw new InvalidOperationException("Insufficient stock for variant.");

                var orderItem = new OrderItem
                {
                    Id = Guid.NewGuid(),
                    OrderId = order.Id,
                    VariantId = itemDto.VariantId,
                    Quantity = itemDto.Quantity,
                    UnitPrice = variant.PriceAdjustment,
                    TotalPrice = variant.PriceAdjustment * itemDto.Quantity,
                    ProductName = variant.Product.Name
                };

                await _orderItemRepository.AddAsync(orderItem);

                variant.StockQuantity -= itemDto.Quantity;
                await _variantRepository.UpdateAsync(variant);
            }

            foreach (var appliedCoupon in calculation.AppliedCoupons.Where(c => c.IsValid))
            {
                var coupon = await _couponRepository.GetByCodeAsync(appliedCoupon.CouponCode);
                if (coupon == null) continue;

                var orderCoupon = new OrderCoupon
                {
                    Id = Guid.NewGuid(),
                    OrderId = order.Id,
                    CouponId = coupon.Id,
                    DiscountApplied = appliedCoupon.DiscountAmount
                };

                await _orderCouponRepository.AddAsync(orderCoupon);

                coupon.UsedCount++;
                await _couponRepository.UpdateAsync(coupon);
            }

            transaction.Complete();

            return await GetOrderByIdAsync(order.Id);
        }


        public async Task<bool> DeleteOrderAsync(Guid orderId)
        {
            var order = await _orderRepository.GetByIdAsync(orderId);
            if (order is null)
                return false;

            if (order.OrderStatus is not OrderStatus.Cancelled and not OrderStatus.Pending)
                throw new InvalidOperationException("Only orders with status 'Cancelled' or 'Pending' can be deleted.");

            order.IsDeleted = true;
            await _orderRepository.UpdateAsync(order);

            return true;

        }

        public async Task<OrderDto> GetOrderByIdAsync(Guid orderId)
        {
            var order = await _orderRepository.GetOrderWithItemsAsync(orderId);
            if (order == null)
            {
                throw new ArgumentException("Order not found");
            }

            return _mapper.Map<OrderDto>(order);
        }

        public async Task<OrderDto> GetOrderByNumberAsync(string orderNumber)
        {
            var order = await _orderRepository.GetOrderByNumberAsync(orderNumber);
            if (order == null)
            {
                throw new ArgumentException("Order not found");
            }

            return _mapper.Map<OrderDto>(order);
        }

        public async Task<PagedOrderResultDto> GetOrdersAsync(OrderQueryDto query)
        {
            var orders = await _orderRepository.GetAllAsync();

            if (!string.IsNullOrEmpty(query.UserId))
            {
                orders = orders.Where(o => o.UserId == query.UserId);
            }

            if (query.Status.HasValue)
            {
                orders = orders.Where(o => o.OrderStatus == query.Status.Value);
            }

            if (query.StartDate.HasValue)
            {
                orders = orders.Where(o => o.OrderDate >= query.StartDate.Value);
            }

            if (query.EndDate.HasValue)
            {
                orders = orders.Where(o => o.OrderDate <= query.EndDate.Value);
            }

            if (!string.IsNullOrEmpty(query.OrderNumber))
            {
                orders = orders.Where(o => o.OrderNumber.Contains(query.OrderNumber));
            }

            orders = query.SortDescending
                ? orders.OrderByDescending(o => o.OrderDate)
                : orders.OrderBy(o => o.OrderDate);

            var totalCount = orders.Count();
            var pagedOrders = orders
                .Skip((query.PageNumber - 1) * query.PageSize)
                .Take(query.PageSize)
                .ToList();

            var orderSummaries = _mapper.Map<List<OrderSummaryDto>>(pagedOrders);

            return new PagedOrderResultDto
            {
                Orders = orderSummaries,
                TotalCount = totalCount,
                PageNumber = query.PageNumber,
                PageSize = query.PageSize
            };
        }
        public async Task<List<OrderSummaryDto>> GetPendingOrdersAsync()
        {
            var orders = await _orderRepository.GetPendingOrdersAsync();
            return _mapper.Map<List<OrderSummaryDto>>(orders);
        }

        public async Task<decimal> GetTotalSalesAsync(DateTime? startDate = null, DateTime? endDate = null)
        {
            return await _orderRepository.GetTotalSalesAsync(startDate, endDate);
        }

        public async Task<PagedOrderResultDto> GetUserOrdersAsync(string userId, int pageNumber = 1, int pageSize = 10)
        {
            var (orders, totalCount) = await _orderRepository.GetUserOrderHistoryPagedAsync(userId, pageNumber, pageSize);
            var orderSummaries = _mapper.Map<List<OrderSummaryDto>>(orders);

            return new PagedOrderResultDto
            {
                Orders = orderSummaries,
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize
            };
        }


        public async Task<OrderDto> UpdateOrderAsync(Guid orderId, UpdateOrderDto updateOrderDto)
        {
            var order = await _orderRepository.GetByIdAsync(orderId);
            if (order == null)
            {
                throw new ArgumentException("Order not found");
            }

            // Update properties
            if (updateOrderDto.OrderStatus.HasValue)
            {
                order.OrderStatus = updateOrderDto.OrderStatus.Value;

                if (updateOrderDto.OrderStatus.Value == OrderStatus.Shipped && !order.ShippedDate.HasValue)
                {
                    order.ShippedDate = DateTime.UtcNow;
                }
                else if (updateOrderDto.OrderStatus.Value == OrderStatus.Delivered && !order.DeliveredDate.HasValue)
                {
                    order.DeliveredDate = DateTime.UtcNow;
                }
            }

            if (updateOrderDto.ShippingAddress != null)
                order.ShippingAddress = updateOrderDto.ShippingAddress;

            if (updateOrderDto.BillingAddress != null)
                order.BillingAddress = updateOrderDto.BillingAddress;

            if (!string.IsNullOrEmpty(updateOrderDto.Notes))
                order.Notes = updateOrderDto.Notes;

            if (updateOrderDto.ShippedDate.HasValue)
                order.ShippedDate = updateOrderDto.ShippedDate;

            if (updateOrderDto.DeliveredDate.HasValue)
                order.DeliveredDate = updateOrderDto.DeliveredDate;

            order.OrderDate = DateTime.UtcNow;

            await _orderRepository.UpdateAsync(order);

            return await GetOrderByIdAsync(orderId);
        }

        public async Task<OrderDto> UpdateOrderStatusAsync(Guid orderId, UpdateOrderStatusDto updateStatusDto)
        {
            var updateDto = new UpdateOrderDto
            {
                OrderStatus = updateStatusDto.OrderStatus,
                Notes = updateStatusDto.Notes
            };

            return await UpdateOrderAsync(orderId, updateDto);
        }
    }
}
