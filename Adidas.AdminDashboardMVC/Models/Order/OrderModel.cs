using Adidas.DTOs.Common_DTOs;
using Adidas.DTOs.Operation.OrderDTOs;

namespace Adidas.AdminDashboardMVC.Models.Order;

public class OrderModel
{
    public PagedResultDto<OrderDto> OrderPaged { get; set; }
    public OrderFilterDto Filter { get; set; }
}