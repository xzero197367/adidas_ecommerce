using Adidas.DTOs.Common_DTOs;
using Adidas.DTOs.CommonDTOs;
using Adidas.DTOs.Operation.OrderDTOs;
using Adidas.DTOs.Operation.OrderDTOs.Create;
using static Adidas.Application.Services.Operation.OrderService;

namespace Adidas.Application.Contracts.ServicesContracts.Operation
{
    public interface IOrderService : IGenericService<Order, OrderDto, OrderCreateDto, OrderUpdateDto>
    {
        // Existing methods
        Task<OperationResult<PagedResultDto<OrderDto>>> GetPagedOrdersAsync(int pageNumber, int pageSize, OrderFilterDto? filter = null);
        Task<OperationResult<IEnumerable<OrderDto>>> GetOrdersByUserIdAsync(string userId);
        Task<OperationResult<OrderDto>> GetOrderByUserIdAsync(string userId);
        Task<OperationResult<OrderDto>> GetOrderByOrderNumberAsync(string orderNumber);
        Task<OperationResult<IEnumerable<OrderDto>>> GetOrdersByStatusAsync(OrderStatus status);
        Task<OperationResult<bool>> UpdateOrderStatusAsync(Guid orderId, OrderStatus newStatus);
        Task<OperationResult<OrderDto>> GetOrderWithItemsAsync(Guid orderId);
        Task<OperationResult<decimal>> CalculateOrderTotalAsync(string userId, string? discountCode = null);
        Task<OperationResult<bool>> CancelOrderAsync(Guid orderId, string reason);
        Task<OperationResult<OrderSummaryDto>> GetOrderSummaryAsync(DateTime? startDate = null, DateTime? endDate = null);
        Task<byte[]> ExportToExcelAsync(DateTime? startDate = null, DateTime? endDate = null);
        Task<byte[]> ExportToPdfAsync(DateTime? startDate = null, DateTime? endDate = null);
        Task<OperationResult<object>> GetFormattedOrderSummaryAsync(string userId, string? couponCode = null);
        Task<BillingSummaryDto> GetBillingSummaryAsync(string userId, string? promoCode = null);
        Task<OperationResult<object>> PlaceOrder(CreateOrderDTO orderCreateDTO);

        // New required methods
        Task<OperationResult<OrderDetailDto>> GetOrderByIdAsync(Guid id);        Task<OperationResult<OrderDto>> CreateOrderFromCartAsync(CreateOrderDTO createOrderDto);
        Task<OperationResult<PagedResultDto<OrderDto>>> GetOrderHistoryAsync(string userId, int page = 1, int pageSize = 10, OrderStatus? status = null);
        Task<OperationResult<object>> GetOrderTrackingAsync(Guid id);

        Task<OperationResult<object>> GetGuestCheckoutSummaryAsync(string guestUserId, List<GuestCartItemsDto> cartItems, string? couponCode = null);
        Task<BillingSummaryDto> GetGuestBillingSummaryAsync(string guestUserId, List<GuestCartItemsDto> cartItems, string? promoCode = null);

    }
}