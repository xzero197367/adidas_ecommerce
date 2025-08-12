using System.Text;
using Adidas.Application.Contracts.ServicesContracts.Operation;
using Adidas.DTOs.Common_DTOs;
using Adidas.DTOs.Operation.PaymentDTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Adidas.AdminDashboardMVC.Controllers.Operation.Payment;
[Authorize(Policy = "EmployeeOrAdmin")]

public class PaymentController : Controller
{
    private readonly IPaymentService _paymentService;
    private readonly IPaymentReportService _paymentReportService;
    private readonly ILogger<PaymentController> _logger;

    public PaymentController(
        IPaymentService paymentService,
        IPaymentReportService paymentReportService,
        ILogger<PaymentController> logger)
    {
        _paymentService = paymentService;
        _paymentReportService = paymentReportService;
        _logger = logger;
    }

    // GET: Payment
    public async Task<IActionResult> Index(PaymentFilterDto filter)
    {
        try
        {
            ViewBag.Filter = filter;
            ViewBag.PaymentMethods = await GetPaymentMethodsAsync();
            ViewBag.PaymentStatuses = new List<string> { "Pending", "Completed", "Failed", "Refunded" };

            var payments = await GetFilteredPaymentsAsync(filter);
            return View(payments);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading payments index");
            TempData["ErrorMessage"] = "Error loading payments. Please try again.";
            return View(new PagedResultDto<PaymentDto> { Items = new List<PaymentDto>() });
        }
    }

    // GET: Payment/Details/5
    public async Task<IActionResult> Details(Guid id)
    {
        try
        {
            var result = await _paymentService.GetPaymentByIdAsync(id);
            if (!result.IsSuccess || result.Data == null)
            {
                TempData["ErrorMessage"] = "Payment not found.";
                return RedirectToAction(nameof(Index));
            }

            return View(result.Data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading payment details for {Id}", id);
            TempData["ErrorMessage"] = "Error loading payment details.";
            return RedirectToAction(nameof(Index));
        }
    }

    // GET: Payment/Create
    public async Task<IActionResult> Create(Guid? orderId)
    {
        try
        {
            ViewBag.PaymentMethods = await GetPaymentMethodsAsync();

            var model = new PaymentCreateDto();
            
            if (orderId.HasValue)
            {
                model.OrderId = orderId.Value;
            }

            return View(model);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading create payment form");
            TempData["ErrorMessage"] = "Error loading form. Please try again.";
            return RedirectToAction(nameof(Index));
        }
    }

    // POST: Payment/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(PaymentCreateDto model)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                ViewBag.PaymentMethods = await GetPaymentMethodsAsync();
                return View(model);
            }

            var result = await _paymentService.ProcessPaymentAsync(model);
            if (result.IsSuccess)
            {
                TempData["SuccessMessage"] = "Payment processed successfully.";
                return RedirectToAction(nameof(Details), new { id = result.Data.Id });
            }

            TempData["ErrorMessage"] = result.ErrorMessage ?? "Error processing payment.";
            ViewBag.PaymentMethods = await GetPaymentMethodsAsync();
            return View(model);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating payment");
            TempData["ErrorMessage"] = "Error processing payment. Please try again.";
            ViewBag.PaymentMethods = await GetPaymentMethodsAsync();
            return View(model);
        }
    }

    // POST: Payment/Refund/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Refund(Guid id, decimal? refundAmount)
    {
        try
        {
            var result = await _paymentService.RefundPaymentAsync(id, refundAmount);
            if (result != null && result.IsSuccess)
            {
                TempData["SuccessMessage"] = "Payment refunded successfully.";
            }
            else
            {
                TempData["ErrorMessage"] = result?.ErrorMessage ?? "Error processing refund.";
            }

            return RedirectToAction(nameof(Details), new { id });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing refund for payment {Id}", id);
            TempData["ErrorMessage"] = "Error processing refund. Please try again.";
            return RedirectToAction(nameof(Details), new { id });
        }
    }

    // GET: Payment/Dashboard
    public async Task<IActionResult> Dashboard()
    {
        try
        {
            var endDate = DateTime.UtcNow;
            var startDate = endDate.AddDays(-30);

            var report = await _paymentReportService.GetPaymentReportAsync(startDate, endDate);
            
            ViewBag.StartDate = startDate;
            ViewBag.EndDate = endDate;

            return View(report);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading payment dashboard");
            TempData["ErrorMessage"] = "Error loading dashboard. Please try again.";
            return View(new PaymentReportDto());
        }
    }

    // GET: Payment/Reports
    public async Task<IActionResult> Reports(DateTime? startDate, DateTime? endDate)
    {
        try
        {
            var start = startDate ?? DateTime.UtcNow.AddDays(-30);
            var end = endDate ?? DateTime.UtcNow;

            var report = await _paymentReportService.GetPaymentReportAsync(start, end);
            
            ViewBag.StartDate = start;
            ViewBag.EndDate = end;

            return View(report);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating payment report");
            TempData["ErrorMessage"] = "Error generating report. Please try again.";
            return View(new PaymentReportDto());
        }
    }

    // GET: Payment/ExportReport
    public async Task<IActionResult> ExportReport(DateTime startDate, DateTime endDate, string format = "csv")
    {
        try
        {
            var report = await _paymentReportService.GetPaymentReportAsync(startDate, endDate);
            
            if (format.ToLower() == "csv")
            {
                var csv = GenerateCSVReport(report);
                var bytes = Encoding.UTF8.GetBytes(csv);
                return File(bytes, "text/csv", $"payment-report-{startDate:yyyy-MM-dd}-to-{endDate:yyyy-MM-dd}.csv");
            }

            // Add other export formats as needed (Excel, PDF, etc.)
            return BadRequest("Unsupported export format");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error exporting payment report");
            TempData["ErrorMessage"] = "Error exporting report. Please try again.";
            return RedirectToAction(nameof(Reports));
        }
    }

    // API endpoint for getting payment data for charts
    [HttpGet]
    public async Task<IActionResult> GetChartData(DateTime startDate, DateTime endDate, string chartType)
    {
        try
        {
            switch (chartType.ToLower())
            {
                case "daily":
                    var dailyStats = await _paymentReportService.GetDailyPaymentStatsAsync(startDate, endDate);
                    return Json(dailyStats);
                
                case "methods":
                    var methodStats = await _paymentReportService.GetPaymentMethodStatsAsync(startDate, endDate);
                    return Json(methodStats);
                
                default:
                    return BadRequest("Invalid chart type");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting chart data");
            return BadRequest("Error loading chart data");
        }
    }

    private async Task<PagedResultDto<PaymentDto>> GetFilteredPaymentsAsync(PaymentFilterDto filter)
    {
        // Implement filtering logic based on the filter parameters
        // This is a simplified version - you'll need to extend your repository to support complex filtering
        
        var payments = new PagedResultDto<PaymentDto>
        {
            Items = new List<PaymentDto>(),
            TotalCount = 0,
            PageNumber = filter.PageNumber,
            PageSize = filter.PageSize
        };

        return payments;
    }

    private async Task<List<string>> GetPaymentMethodsAsync()
    {
        // Return available payment methods
        return new List<string> { "Credit Card", "PayPal", "Stripe", "Bank Transfer", "Cash on Delivery" };
    }

    private string GenerateCSVReport(PaymentReportDto report)
    {
        var csv = new StringBuilder();
        csv.AppendLine("Payment Report");
        csv.AppendLine($"Total Revenue,{report.TotalRevenue:C}");
        csv.AppendLine($"Total Transactions,{report.TotalTransactions}");
        csv.AppendLine($"Success Rate,{report.SuccessRate:F2}%");
        csv.AppendLine();
        
        csv.AppendLine("Payment Method Statistics");
        csv.AppendLine("Method,Count,Amount,Percentage");
        foreach (var stat in report.PaymentMethodStats)
        {
            csv.AppendLine($"{stat.PaymentMethod},{stat.Count},{stat.Amount:C},{stat.Percentage:F2}%");
        }

        return csv.ToString();
    }
}