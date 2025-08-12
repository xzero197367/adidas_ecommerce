using System.Security.Claims;
using Adidas.AdminDashboardMVC.Models.Order;
using Adidas.Application.Contracts.ServicesContracts.Operation;
using Adidas.DTOs.Operation.OrderDTOs;
using Adidas.DTOs.Operation.OrderDTOs.Create;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Adidas.AdminDashboardMVC.Controllers.Orders
{
    [Authorize(Policy = "EmployeeOrAdmin")]

    public class OrdersController : Controller
    {
        private IOrderService orderService;

        public OrdersController(IOrderService orderService)
        {
            this.orderService = orderService;
        }

        public async Task<IActionResult> Index(OrderFilterDto? filter = null, int pageNumber = 1, int pageSize = 10)
        {
            var orderPages = await orderService.GetPagedOrdersAsync(pageNumber, pageSize, filter);

            if (orderPages.IsSuccess == false)
            {
                return NotFound();
            }

            var data = new OrderModel()
            {
                Filter = filter,
                OrderPaged = orderPages.Data
            };
            return View(data);
        }

        public async Task<IActionResult> Delete(Guid id)
        {
            await orderService.DeleteAsync(id);
            return RedirectToAction(nameof(Index));
        }


        public async Task<IActionResult> Details(Guid id)
        {
            var order = await orderService.GetByIdAsync(id);
            return View(order.Data);
        }

        #region Create Order Actions

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CreateOrder(OrderCreateDto dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier); // Identity's user ID
            dto.UserId = userId;
            var result = await orderService.CreateAsync(dto);


            if (result.IsSuccess == false)
            {
                ModelState.AddModelError("", result.ErrorMessage);
                return View("Create", dto);
            }

            return RedirectToAction(nameof(Index));
        }

        #endregion


        #region Edit Order Actions

        public async Task<IActionResult> Edit(Guid id)
        {
            var order = await orderService.GetByIdAsync(id);
            if (order.IsSuccess == false)
            {
                return NotFound();
            }

            return View(order.Data);
        }

        [HttpPost]
        public async Task<IActionResult> EditOrder(OrderUpdateDto dto)
        {
            var result = await orderService.UpdateAsync(dto);

            if (result.IsSuccess == false)
            {
                ModelState.AddModelError("", result.ErrorMessage);
                return View("Edit", dto);
            }

            return RedirectToAction(nameof(Index));
        }

        #endregion
    }
}