using Adidas.DTOs.Operation.OrderDTOs.Calculation;
using Adidas.DTOs.Operation.OrderDTOs.Create;
using Adidas.DTOs.Operation.OrderDTOs.Query;
using Adidas.DTOs.Operation.OrderDTOs.Result;
using Adidas.DTOs.Operation.OrderDTOs.Update;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Adidas.Application.Contracts.ServicesContracts.Operation
{
    public interface IOrderService
    {
        Task<OrderDto> CreateOrderAsync(CreateOrderDto createOrderDto);
        Task<OrderDto> GetOrderByIdAsync(Guid orderId);
        Task<OrderDto> GetOrderByNumberAsync(string orderNumber);
        Task<PagedOrderResultDto> GetOrdersAsync(OrderQueryDto query);
        Task<PagedOrderResultDto> GetUserOrdersAsync(string userId, int pageNumber = 1, int pageSize = 10);
        Task<OrderDto> UpdateOrderAsync(Guid orderId, UpdateOrderDto updateOrderDto);
        Task<OrderDto> UpdateOrderStatusAsync(Guid orderId, UpdateOrderStatusDto updateStatusDto);
        Task<bool> CancelOrderAsync(Guid orderId, string reason = null);
        Task<OrderCalculationDto> CalculateOrderAsync(CreateOrderDto createOrderDto);
        Task<decimal> GetTotalSalesAsync(DateTime? startDate = null, DateTime? endDate = null);
        Task<List<OrderSummaryDto>> GetPendingOrdersAsync();
        Task<bool> DeleteOrderAsync(Guid orderId);
    }                      
 
}
