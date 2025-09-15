
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Adidas.Application.Contracts.ServicesContracts.Operation;
using Adidas.DTOs.Operation.OrderDTOs;
using Adidas.DTOs.Operation.OrderDTOs.Create;
using Adidas.Models.Operation;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Models.People;
using System.Text.Json;
using Mapster;
using Microsoft.Extensions.Caching.Memory;   // ✅ Added for caching

namespace Adidas.Web.Controllers
{
    [Authorize]
    public class OrdersController : Controller
    {
        private readonly IOrderService _orderService;
        private readonly UserManager<User> _userManager;
        private readonly IOrderEditService _orderEditService;
        private readonly IOrderFilterService _orderFilterService;
        private readonly IMemoryCache _cache;   // ✅ Add cache

        public OrdersController(
            IOrderService orderService,
        IOrderEditService orderEditService,
            UserManager<User> userManager,
        IOrderFilterService orderFilterService,
            IMemoryCache cache)   // ✅ Inject cache
        {
            _orderService = orderService;
            _orderEditService = orderEditService;
            _userManager = userManager;
            _orderFilterService = orderFilterService;
            _cache = cache;   // ✅ Assign cache
        }

        [Authorize]
        public async Task<IActionResult> Index(
            int pageNumber = 1, int pageSize = 10,
            string? orderNumber = null,
            OrderStatus? status = null,
            DateTime? startDate = null,
            DateTime? endDate = null,
            bool? isGuest = null)
        {
            var filter = new ExtendedOrderFilterDto
            {
                OrderNumber = string.IsNullOrWhiteSpace(orderNumber) ? null : orderNumber.Trim(),
                OrderStatus = status,
                StartDate = startDate,
                EndDate = endDate,
                IsGuest = isGuest
            };

            var result = await _orderFilterService.GetFilteredOrdersAsync(pageNumber, pageSize, filter);

            ViewBag.PageNumber = pageNumber;
            ViewBag.PageSize = pageSize;
            ViewBag.OrderNumber = orderNumber;
            ViewBag.OrderStatus = status;
            ViewBag.StartDate = startDate;
            ViewBag.EndDate = endDate;
            ViewBag.IsGuest = isGuest;

            return View(result);
        }

        public async Task<IActionResult> Details(Guid id)
        {
            var result = await _orderService.GetOrderWithItemsAsync(id);

            if (!result.IsSuccess)
            {
                TempData["Error"] = result.ErrorMessage;
                return RedirectToAction(nameof(Index));
            }

            var dto = result.Data.Adapt<OrderWithCreatorDto>();

            // ✅ Get cached last update info
            if (_cache.TryGetValue($"LastUpdate_{id}", out OrderLastUpdateDto updateInfo))
            {
                ViewBag.LastUpdatedBy = updateInfo.UpdatedBy;
                ViewBag.LastUpdatedAt = updateInfo.UpdatedAt.ToString("yyyy-MM-dd HH:mm");
            }

            return View(dto);
        }

        [Authorize(Roles = "Admin,Employee")]
        public IActionResult Create()
        {
            var model = new OrderCreateDto
            {
                OrderDate = DateTime.UtcNow,
                Currency = "USD",
                OrderStatus = OrderStatus.Pending
            };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Employee")]
        public async Task<IActionResult> Create(OrderCreateDto orderDto)
        {
            if (!ModelState.IsValid)
                return View(orderDto);

            try
            {
                var result = await _orderService.CreateAsync(orderDto);

                if (result.IsSuccess)
                {
                    TempData["Success"] = "Order created successfully!";
                    return RedirectToAction(nameof(Details), new { id = result.Data.Id });
                }

                TempData["Error"] = result.ErrorMessage;
                return View(orderDto);
            }
            catch
            {
                TempData["Error"] = "An error occurred while creating the order.";
                return View(orderDto);
            }
        }

        [Authorize(Roles = "Admin,Employee")]
        public async Task<IActionResult> Edit(Guid id)
        {
            var result = await _orderService.GetByIdAsync(id);

            if (!result.IsSuccess)
            {
                TempData["Error"] = result.ErrorMessage;
                return RedirectToAction(nameof(Index));
            }

            var updateDto = new OrderUpdateDto
            {
                Id = result.Data.Id,
                OrderNumber = result.Data.OrderNumber,
                OrderStatus = result.Data.OrderStatus,
                Subtotal = result.Data.Subtotal,
                TaxAmount = result.Data.TaxAmount,
                ShippingAmount = result.Data.ShippingAmount,
                DiscountAmount = result.Data.DiscountAmount,
                TotalAmount = result.Data.TotalAmount,
                Currency = result.Data.Currency,
                ShippingAddress = result.Data.ShippingAddress ?? string.Empty,
                BillingAddress = result.Data.BillingAddress ?? string.Empty,
                Notes = result.Data.Notes,
                UserId = result.Data.UserId
            };

            return View(updateDto);
        }

        // ✅ Updated Edit POST with caching
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Employee")]
        public async Task<IActionResult> Edit(Guid id, OrderUpdateDto orderDto)
        {
            if (id != orderDto.Id)
            {
                TempData["Error"] = "Invalid order ID.";
                return RedirectToAction(nameof(Index));
            }

            if (!ModelState.IsValid)
                return View(orderDto);

            var user = await _userManager.GetUserAsync(User);
            var displayName = user?.Email ?? user?.UserName ?? User.Identity?.Name ?? "Unknown";

            var result = await _orderEditService.EditWithTrackingAsync(orderDto, displayName);

            if (result.IsSuccess)
            {
                TempData["Success"] = "Order updated successfully!";

                // ✅ Store globally in memory cache
                var updateInfo = new OrderLastUpdateDto
                {
                    OrderId = id,
                    UpdatedBy = result.Data.UpdatedBy,
                    UpdatedAt = result.Data.UpdatedAt
                };

                _cache.Set($"LastUpdate_{id}", updateInfo, TimeSpan.FromHours(3));
                // ⏳ expires after 3 hours

                return RedirectToAction(nameof(Details), new { id });
            }

            TempData["Error"] = result.ErrorMessage;
            return View(orderDto);
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var result = await _orderService.GetByIdAsync(id);

            if (!result.IsSuccess)
            {
                TempData["Error"] = result.ErrorMessage;
                return RedirectToAction(nameof(Index));
            }

            return View(result.Data);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            try
            {
                var result = await _orderService.DeleteAsync(id);

                if (result.IsSuccess)
                    TempData["Success"] = "Order deleted successfully!";
                else
                    TempData["Error"] = result.ErrorMessage;

                return RedirectToAction(nameof(Index));
            }
            catch
            {
                TempData["Error"] = "An error occurred while deleting the order.";
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Employee")]
        public async Task<IActionResult> UpdateStatus(Guid id, OrderStatus status)
        {
            try
            {
                var result = await _orderService.UpdateOrderStatusAsync(id, status);

                if (result.IsSuccess)
                    TempData["Success"] = $"Order status updated to {status}!";
                else
                    TempData["Error"] = result.ErrorMessage;

                return RedirectToAction(nameof(Details), new { id });
            }
            catch
            {
                TempData["Error"] = "An error occurred while updating the order status.";
                return RedirectToAction(nameof(Details), new { id });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Employee")]
        public async Task<IActionResult> Cancel(Guid id, string reason)
        {
            try
            {
                var result = await _orderService.CancelOrderAsync(id, reason);

                if (result.IsSuccess)
                    TempData["Success"] = "Order cancelled successfully!";
                else
                    TempData["Error"] = result.ErrorMessage;

                return RedirectToAction(nameof(Details), new { id });
            }
            catch
            {
                TempData["Error"] = "An error occurred while cancelling the order.";
                return RedirectToAction(nameof(Details), new { id });
            }
        }

        public async Task<IActionResult> MyOrders()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return RedirectToAction("Login", "Account");

            var result = await _orderService.GetOrdersByUserIdAsync(user.Id);

            if (!result.IsSuccess)
            {
                TempData["Error"] = result.ErrorMessage;
                return View(new List<OrderDto>());
            }

            return View(result.Data);
        }

        [Authorize(Roles = "Admin,Employee")]
        public async Task<IActionResult> Summary(DateTime? startDate = null, DateTime? endDate = null)
        {
            var result = await _orderService.GetOrderSummaryAsync(startDate, endDate);

            if (!result.IsSuccess)
            {
                TempData["Error"] = result.ErrorMessage;
                return View();
            }

            ViewBag.StartDate = startDate?.ToString("yyyy-MM-dd");
            ViewBag.EndDate = endDate?.ToString("yyyy-MM-dd");

            return View(result.Data);
        }

        public async Task<FileResult> ExportToFile(string format, DateTime? startDate = null, DateTime? endDate = null)
        {
            if (format == "pdf")
            {
                var pdfBytes = await _orderService.ExportToPdfAsync(startDate, endDate);
                return File(pdfBytes, "application/pdf", "orders.pdf");
            }

            var excelBytes = await _orderService.ExportToExcelAsync(startDate, endDate);
            return File(excelBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "orders.xlsx");
        }

        public async Task<IActionResult> Search(string orderNumber)
        {
            if (string.IsNullOrEmpty(orderNumber))
                return RedirectToAction(nameof(Index));

            var result = await _orderService.GetOrderByOrderNumberAsync(orderNumber);

            if (!result.IsSuccess)
            {
                TempData["Error"] = result.ErrorMessage;
                return RedirectToAction(nameof(Index));
            }

            return RedirectToAction(nameof(Details), new { id = result.Data.Id });
        }

        [HttpGet("get-customers")]
        public async Task<IActionResult> GetCustomers(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? search = null,
            [FromQuery] string? role = null)
        {
            try
            {
                var query = _userManager.Users.Where(u => !u.IsDeleted && u.IsActive);

                if (!string.IsNullOrEmpty(search))
                {
                    query = query.Where(u =>
                        u.FirstName.Contains(search) ||
                        u.LastName.Contains(search) ||
                        u.Email.Contains(search) ||
                        u.UserName.Contains(search));
                }

                if (!string.IsNullOrEmpty(role) && Enum.TryParse<UserRole>(role, out var userRole))
                {
                    query = query.Where(u => u.Role == userRole);
                }

                var totalCount = await query.CountAsync();

                var users = await query
                    .OrderBy(u => u.FirstName)
                    .ThenBy(u => u.LastName)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Select(u => new
                    {
                        u.Id,
                        u.FirstName,
                        u.LastName,
                        u.Email,
                        u.UserName,
                        Role = u.Role.ToString(),
                        u.Phone,
                        u.IsActive
                    })
                    .ToListAsync();

                var result = new
                {
                    Items = users,
                    TotalCount = totalCount,
                    PageNumber = page,
                    PageSize = pageSize,
                    TotalPages = (int)Math.Ceiling((double)totalCount / pageSize)
                };

                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Error retrieving customers", error = ex.Message });
            }
        }
    }
}