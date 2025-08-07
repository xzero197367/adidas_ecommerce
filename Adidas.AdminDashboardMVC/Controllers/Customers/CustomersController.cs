using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Adidas.Application.Contracts.ServicesContracts.People;
using Adidas.DTOs.People.Customer_DTOs;

namespace Adidas.AdminDashboardMVC.Controllers.Customers
{
    [Authorize(Policy = "AdminOnly")]
    public class CustomersController : Controller
    {
        private readonly ICustomerService _customerService;

        public CustomersController(ICustomerService customerService)
        {
            _customerService = customerService;
        }

        public async Task<IActionResult> Index(CustomerFilterDto filter)
        {
            if (filter.Page <= 0) filter.Page = 1;
            if (filter.PageSize <= 0) filter.PageSize = 10;

            var result = await _customerService.GetCustomersAsync(filter);

            ViewBag.CurrentFilter = filter;
            return View(result.Data);
        }

        [HttpGet]
        public async Task<IActionResult> Details(string id)
        {
            if (string.IsNullOrEmpty(id))
                return NotFound();

            var customer = await _customerService.GetCustomerByIdAsync(id);
            if (customer.IsSuccess == false)
                return NotFound();

            return View(customer.Data);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(string id)
        {
            if (string.IsNullOrEmpty(id))
                return NotFound();

            var customer = await _customerService.GetCustomerByIdAsync(id);
            if (customer.IsSuccess == false)
                return NotFound();

            var updateDto = new CustomerUpdateDto
            {
                Phone = customer.Data.Phone,
                Gender = customer.Data.Gender,
                DateOfBirth = customer.Data.DateOfBirth,
                PreferredLanguage = customer.Data.PreferredLanguage
            };

            ViewBag.CustomerId = id;
            ViewBag.CustomerName = customer.Data.Name;
            ViewBag.CustomerEmail = customer.Data.Email;

            return View(updateDto);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, CustomerUpdateDto customerUpdateDto)
        {
            if (string.IsNullOrEmpty(id))
                return NotFound();

            if (!ModelState.IsValid)
            {
                var customer = await _customerService.GetCustomerByIdAsync(id);
                ViewBag.CustomerId = id;
                ViewBag.CustomerName = customer?.Data.Name;
                ViewBag.CustomerEmail = customer?.Data.Email;
                return View(customerUpdateDto);
            }

            var success = await _customerService.UpdateCustomerAsync(id, customerUpdateDto);

            if (success.IsSuccess)
            {
                TempData["Success"] = "Customer updated successfully.";
                return RedirectToAction(nameof(Details), new { id });
            }

            TempData["Error"] = "Failed to update customer.";
            return View(customerUpdateDto);
        }

        [HttpPost]
        public async Task<IActionResult> ToggleStatus(string id)
        {
            if (string.IsNullOrEmpty(id))
                return Json(new { success = false, message = "Invalid customer ID." });

            var success = await _customerService.ToggleCustomerStatusAsync(id);

            return Json(new
            {
                success = success,
                message = success.IsSuccess ? "Customer status updated successfully." : "Failed to update customer status."
            });
        }

        [HttpPost]
        public async Task<IActionResult> Delete(string id)
        {
            if (string.IsNullOrEmpty(id))
                return Json(new { success = false, message = "Invalid customer ID." });

            var success = await _customerService.DeleteCustomerAsync(id);

            return Json(new
            {
                success = success,
                message = success.IsSuccess ? "Customer deleted successfully." : "Failed to delete customer."
            });
        }

        [HttpGet]
        public async Task<IActionResult> Export(CustomerFilterDto filter)
        {
            var csvData = await _customerService.ExportCustomersAsync(filter);

            return File(csvData, "text/csv", $"customers_export_{DateTime.Now:yyyyMMdd_HHmmss}.csv");
        }

        // AJAX endpoint for filtered data
        [HttpGet]
        public async Task<IActionResult> GetCustomersData(CustomerFilterDto filter)
        {
            var result = await _customerService.GetCustomersAsync(filter);
            return PartialView("_CustomerListPartial", result.Data);
        }
    }
}