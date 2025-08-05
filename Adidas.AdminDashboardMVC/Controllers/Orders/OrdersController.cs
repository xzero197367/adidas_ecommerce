using Adidas.Application.Contracts.ServicesContracts.Operation;
using Microsoft.AspNetCore.Mvc;

namespace Adidas.AdminDashboardMVC.Controllers.Orders
{
    public class OrdersController : Controller
    {
        private IOrderService orderService;
        public OrdersController(IOrderService orderService)
        {
            this.orderService = orderService;
        }
        public async Task<IActionResult> Index(int pageNumber = 1, int pageSize = 10)
        {
            var orderPages = await orderService.GetPagedAsync(pageNumber, pageSize);
            
            return View(orderPages);
        }
    }
}
