using Adidas.DTOs.Common_DTOs;
using Adidas.DTOs.CommonDTOs;
using Adidas.DTOs.Operation.OrderDTOs;
using Adidas.DTOs.Operation.OrderDTOs.Create;

namespace Adidas.Application.Contracts.ServicesContracts.Operation
{
    public interface IOrderService : IGenericService<Order, OrderDto, OrderCreateDto, OrderUpdateDto>
    {
        Task<OperationResult<PagedResultDto<OrderDto>>>
            GetPagedOrdersAsync(int pageNumber, int pageSize,
                 OrderFilterDto? filter=null);
        Task<OperationResult<IEnumerable<OrderDto>>> GetOrdersByUserIdAsync(string userId);
        Task<OperationResult<OrderDto>> GetOrderByOrderNumberAsync(string orderNumber);
        Task<OperationResult<IEnumerable<OrderDto>>> GetOrdersByStatusAsync(OrderStatus status);
        // Task<OperationResult<OrderDto>> CreateOrderFromCartAsync(string userId, CreateOrderFromCartDto orderDto);
        Task<OperationResult<bool>> UpdateOrderStatusAsync(Guid orderId, OrderStatus newStatus);
        Task<OperationResult<OrderDto>> GetOrderWithItemsAsync(Guid orderId);
        Task<OperationResult<decimal>> CalculateOrderTotalAsync(string userId, string? discountCode = null);
        Task<OperationResult<bool>> CancelOrderAsync(Guid orderId, string reason);

        Task<OperationResult<OrderSummaryDto>> GetOrderSummaryAsync(DateTime? startDate = null,
            DateTime? endDate = null);
        Task<IEnumerable<OrderDto>> GetRecentOrdersAsync(int count);
    }
}