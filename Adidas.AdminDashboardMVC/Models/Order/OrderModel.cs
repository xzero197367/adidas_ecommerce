using Adidas.DTOs.Operation.OrderDTOs.Result;

namespace Adidas.AdminDashboardMVC.Models.Order;

public class OrderModel
{
    public List<OrderDto> Orders { get; set; }
    public int TotalCount { get; set; }
}