using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Adidas.Application.Contracts.ServicesContracts.People;
using Adidas.DTOs.People.Customer_DTOs;
using Models.People;
using Microsoft.AspNetCore.Identity;
using Adidas.AdminDashboardMVC.Controllers.System;

namespace Adidas.AdminDashboardMVC.Controllers.Customers
{
    [Authorize(Policy = "AdminOnly")]
    public class CustomersController : Controller
    {
        private readonly ICustomerService _customerService;
        private readonly UserManager<User> _userManager;
        private readonly ILogger<CustomersController> _logger;

        public CustomersController(ICustomerService customerService, UserManager<User> userManager, ILogger<CustomersController> logger)
        {
            _customerService = customerService;
            _userManager = userManager;
            _logger = logger;

        }

        public async Task<IActionResult> Index(CustomerFilterDto filter)
        {
            if (filter.Page <= 0) filter.Page = 1;
            if (filter.PageSize <= 0) filter.PageSize = 10;

            var result = await _customerService.GetCustomersAsync(filter);

            // Get statistics for the cards
            var totalCustomersFilter = new CustomerFilterDto { PageSize = int.MaxValue };
            var allCustomersResult = await _customerService.GetCustomersAsync(totalCustomersFilter);

            // Access the Data property to get the actual PagedResultDto
            var totalCustomers = allCustomersResult.Data.TotalCount;
            var activeCustomers = allCustomersResult.Data.Items.Count(c => c.Status == "Active");
            var suspendedCustomers = allCustomersResult.Data.Items.Count(c => c.Status == "Suspended");

            ViewBag.CurrentFilter = filter;
            ViewBag.TotalCustomers = totalCustomers;
            ViewBag.ActiveCustomers = activeCustomers;
            ViewBag.SuspendedCustomers = suspendedCustomers;

            return View(result.Data);
        }

        //[HttpGet]
        //public async Task<IActionResult> GetCustomersData(CustomerFilterDto filter)
        //{
        //    if (filter.Page <= 0) filter.Page = 1;
        //    if (filter.PageSize <= 0) filter.PageSize = 10;

        //    var result = await _customerService.GetCustomersAsync(filter);
        //    return PartialView("_CustomerListPartial", result.Data);
        //}
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
            {
                return Json(new { success = false, message = "Invalid customer ID." });
            }

            try
            {
                var result = await _customerService.ToggleCustomerStatusAsync(id);

                if (result.IsSuccess)
                {
                    // Get the updated customer to return the current status
                    var customerResult = await _customerService.GetCustomerByIdAsync(id);

                    if (customerResult.IsSuccess)
                    {
                        return Json(new
                        {
                            success = true,
                            isActive = customerResult.Data.IsActive,
                            message = "Customer status updated successfully."
                        });
                    }
                    else
                    {
                        // Fallback - assume the toggle worked but we couldn't get updated data
                        return Json(new
                        {
                            success = true,
                            message = "Customer status updated successfully."
                        });
                    }
                }

                return Json(new { success = false, message = result });
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error toggling customer status for ID: {CustomerId}", id);
                return Json(new { success = false, message = "An error occurred while updating customer status." });
            }
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

            return File(csvData.Data, "text/csv", $"customers_export_{DateTime.Now:yyyyMMdd_HHmmss}.csv");
        }

        //AJAX endpoint for filtered data
       [HttpGet]
        public async Task<IActionResult> GetCustomersData(CustomerFilterDto filter)
        {
            var result = await _customerService.GetCustomersAsync(filter);
            return PartialView("_CustomerListPartial", result.Data);
        }
    }
}