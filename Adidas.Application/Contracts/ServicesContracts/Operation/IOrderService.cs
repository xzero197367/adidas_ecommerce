using Adidas.DTOs.Operation.OrderDTOs.Calculation;
using Adidas.DTOs.Operation.OrderDTOs.Create;
using Adidas.DTOs.Operation.OrderDTOs.Query;
using Adidas.DTOs.Operation.OrderDTOs.Result;
using Adidas.DTOs.Operation.OrderDTOs.Update;
using Adidas.DTOs.Operation.PaymentDTOs.Result;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Adidas.Application.Contracts.ServicesContracts.Operation
{
    public interface IOrderService : IGenericService<Order, OrderDto, CreateOrderDto, UpdateOrderDto>
    {
        Task<IEnumerable<OrderDto>> GetOrdersByUserIdAsync(string userId);
        Task<OrderDto?> GetOrderByOrderNumberAsync(string orderNumber);
        Task<IEnumerable<OrderDto>> GetOrdersByStatusAsync(OrderStatus status);
        Task<OrderDto> CreateOrderFromCartAsync(string userId, CreateOrderFromCartDto orderDto);
        Task<bool> UpdateOrderStatusAsync(Guid orderId, OrderStatus newStatus);
        Task<OrderDto?> GetOrderWithItemsAsync(Guid orderId);
        Task<decimal> CalculateOrderTotalAsync(string userId, string? discountCode = null);
        Task<bool> CancelOrderAsync(Guid orderId, string reason);
        Task<OrderSummaryDto> GetOrderSummaryAsync(DateTime? startDate = null, DateTime? endDate = null);
    }

}
