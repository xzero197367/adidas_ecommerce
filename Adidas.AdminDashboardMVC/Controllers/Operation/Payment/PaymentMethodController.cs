using Adidas.DTOs.Operation.PaymentDTOs;
using Adidas.DTOs.Operation.PaymentDTOs.PaymentMethod;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Adidas.AdminDashboardMVC.Controllers.Operation.Payment;

[Authorize(Policy = "EmployeeOrAdmin")]

public class PaymentMethodController : Controller
{
    private readonly ILogger<PaymentMethodController> _logger;
    // Assume you have a payment method service

    public PaymentMethodController(ILogger<PaymentMethodController> logger)
    {
        _logger = logger;
    }

    public async Task<IActionResult> Index()
    {
        try
        {
            // Load payment methods from your service
            var paymentMethods = new List<PaymentMethodDto>();
            return View(paymentMethods);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading payment methods");
            TempData["ErrorMessage"] = "Error loading payment methods.";
            return View(new List<PaymentMethodDto>());
        }
    }

    [HttpGet]
    public IActionResult Create()
    {
        return View(new PaymentMethodCreateDto());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(PaymentMethodCreateDto model)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Create payment method logic here
            TempData["SuccessMessage"] = "Payment method created successfully.";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating payment method");
            TempData["ErrorMessage"] = "Error creating payment method.";
            return View(model);
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleStatus(Guid id)
    {
        try
        {
            // Toggle payment method status logic here
            TempData["SuccessMessage"] = "Payment method status updated.";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating payment method status");
            TempData["ErrorMessage"] = "Error updating payment method status.";
            return RedirectToAction(nameof(Index));
        }
    }
}