using System.ComponentModel.DataAnnotations;
using Adidas.Application.Contracts.ServicesContracts.Operation;
using Adidas.DTOs.Operation.PaymentDTOs;
using Adidas.DTOs.Operation.PaymentDTOs.PaypalDtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Adidas.ClientAPI.Controllers.Payment
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize] // Require authentication
    public class PaymentController : ControllerBase
    {
        private readonly IPaymentService _paymentService;
        private readonly IPayPalService _payPalService;
        private readonly ILogger<PaymentController> _logger;

        public PaymentController(
            IPaymentService paymentService,
            IPayPalService payPalService,
            ILogger<PaymentController> logger)
        {
            _paymentService = paymentService;
            _payPalService = payPalService;
            _logger = logger;
        }

        /// <summary>
        /// Get payment by ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetPayment(Guid id)
        {
            var result = await _paymentService.GetPaymentByIdAsync(id);

            if (result.IsSuccess)
                return Ok(result);

            return BadRequest(result);
        }

        /// <summary>
        /// Create a new PayPal payment and redirect to PayPal
        /// </summary>
        [HttpPost("paypal/create")]
        public async Task<IActionResult> CreatePayPalPayment([FromBody] PayPalCreatePaymentDto createDto)
        {
            try
            {
                _logger.LogInformation("Creating PayPal payment for user {UserId}", User?.Identity?.Name);

                if (!ModelState.IsValid)
                {
                    var errors = ModelState
                        .Where(x => x.Value.Errors.Count > 0)
                        .Select(x => new { Field = x.Key, Errors = x.Value.Errors.Select(e => e.ErrorMessage) })
                        .ToList();

                    _logger.LogWarning("Invalid model state for PayPal payment creation: {Errors}",
                        string.Join(", ", errors.Select(e => $"{e.Field}: {string.Join(", ", e.Errors)}")));

                    return BadRequest(new
                    {
                        IsSuccess = false,
                        Message = "Validation failed",
                        Errors = errors
                    });
                }

                // Log the incoming request for debugging
                _logger.LogInformation("PayPal payment request: Amount={Amount}, Currency={Currency}, Items={ItemCount}",
                    createDto.Amount, createDto.Currency);

                var result = await _payPalService.CreatePaymentAsync(createDto);

                if (result.IsSuccess)
                {
                    _logger.LogInformation("PayPal payment created successfully with ID {PaymentId}", result.Data?.PaymentId);

                    // Return the approval URL for frontend to redirect
                    return Ok(new
                    {
                        IsSuccess = true,
                        Data = result.Data,
                        Message = "PayPal payment created successfully. Redirect user to ApprovalUrl."
                    });
                }

                _logger.LogWarning("PayPal payment creation failed: {Message}", result.ErrorMessage);
                return BadRequest(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating PayPal payment for user {UserId}", User?.Identity?.Name);
                return StatusCode(500, new
                {
                    IsSuccess = false,
                    Message = "Internal server error occurred while creating payment",
                    Error = ex.Message // Include error details for debugging (remove in production)
                });
            }
        }

        /// <summary>
        /// Execute PayPal payment after user approval
        /// </summary>
        [HttpPost("paypal/execute")]
        public async Task<IActionResult> ExecutePayPalPayment([FromBody] PayPalExecutePaymentDto executeDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                _logger.LogInformation("Executing PayPal payment {PaymentId} for payer {PayerId}",
                    executeDto.PaymentId, executeDto.PayerId);

                var result = await _payPalService.ExecutePaymentAsync(executeDto.PaymentId, executeDto.PayerId);

                if (result.IsSuccess)
                {
                    _logger.LogInformation("PayPal payment executed successfully: {PaymentId}", executeDto.PaymentId);

                    return Ok(new
                    {
                        IsSuccess = true,
                        Data = result.Data,
                        Message = "Payment executed successfully"
                    });
                }

                _logger.LogWarning("PayPal payment execution failed: {Message}", result.ErrorMessage);
                return BadRequest(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error executing PayPal payment {PaymentId}", executeDto.PaymentId);
                return StatusCode(500, new
                {
                    IsSuccess = false,
                    Message = "Internal server error occurred while executing payment",
                    Error = ex.Message
                });
            }
        }

        /// <summary>
        /// Handle PayPal success return (GET endpoint for browser redirect)
        /// </summary>
        [HttpGet("paypal/success")]
        [AllowAnonymous] // Allow anonymous access for PayPal redirects
        public async Task<IActionResult> PayPalSuccess([FromQuery] string paymentId, [FromQuery] string PayerID)
        {
            try
            {
                if (string.IsNullOrEmpty(paymentId) || string.IsNullOrEmpty(PayerID))
                {
                    _logger.LogWarning("PayPal success callback missing parameters: paymentId={PaymentId}, PayerID={PayerID}",
                        paymentId, PayerID);
                    return Redirect($"{GetFrontendUrl()}/payment/failed?error=missing_parameters");
                }

                _logger.LogInformation("PayPal success callback: PaymentId={PaymentId}, PayerID={PayerID}",
                    paymentId, PayerID);

                var result = await _payPalService.ExecutePaymentAsync(paymentId, PayerID);

                if (result.IsSuccess)
                {
                    _logger.LogInformation("PayPal payment executed successfully via callback: {PaymentId}", paymentId);

                    // Redirect to success page with payment details
                    return Redirect($"{GetFrontendUrl()}/payment/success?paymentId={result.Data.Id}&transactionId={result.Data.TransactionId}");
                }

                _logger.LogWarning("PayPal payment execution failed via callback: {Message}", result.ErrorMessage);
                return Redirect($"{GetFrontendUrl()}/payment/failed?error=execution_failed&message={Uri.EscapeDataString(result.ErrorMessage)}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in PayPal success callback for payment {PaymentId}", paymentId);
                return Redirect($"{GetFrontendUrl()}/payment/failed?error=server_error");
            }
        }

        /// <summary>
        /// Handle PayPal cancel return (GET endpoint for browser redirect)
        /// </summary>
        [HttpGet("paypal/cancel")]
        [AllowAnonymous] // Allow anonymous access for PayPal redirects
        public IActionResult PayPalCancel([FromQuery] string token)
        {
            _logger.LogInformation("PayPal payment cancelled. Token: {Token}", token);
            return Redirect($"{GetFrontendUrl()}/payment/cancelled?token={token}");
        }

        /// <summary>
        /// Get payment details by PayPal payment ID
        /// </summary>
        [HttpGet("paypal/{paymentId}")]
        public async Task<IActionResult> GetPayPalPaymentDetails(string paymentId)
        {
            try
            {
                var result = await _payPalService.GetPaymentDetailsAsync(paymentId);

                if (result.IsSuccess)
                    return Ok(result);

                return BadRequest(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting PayPal payment details for {PaymentId}", paymentId);
                return StatusCode(500, "Internal server error occurred while retrieving payment details");
            }
        }

        /// <summary>
        /// Refund a PayPal payment
        /// </summary>
        [HttpPost("paypal/refund")]
        public async Task<IActionResult> RefundPayPalPayment([FromBody] PayPalRefundDto refundDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var result = await _payPalService.RefundPaymentAsync(refundDto.TransactionId, refundDto.Amount);

                if (result.IsSuccess)
                {
                    return Ok(new
                    {
                        IsSuccess = true,
                        Data = result.Data,
                        Message = "Refund processed successfully"
                    });
                }

                return BadRequest(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing PayPal refund for transaction {TransactionId}", refundDto.TransactionId);
                return StatusCode(500, "Internal server error occurred while processing refund");
            }
        }

        /// <summary>
        /// Get all pending payments
        /// </summary>
        [HttpGet("pending")]
        public async Task<IActionResult> GetPendingPayments()
        {
            try
            {
                var result = await _paymentService.GetPendingPaymentsAsync();

                if (result.IsSuccess)
                    return Ok(result);

                return BadRequest(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving pending payments");
                return StatusCode(500, "Internal server error occurred while retrieving pending payments");
            }
        }

        /// <summary>
        /// Process a regular payment (non-PayPal)
        /// </summary>
        [HttpPost("process")]
        public async Task<IActionResult> ProcessPayment([FromBody] PaymentCreateDto paymentDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var result = await _paymentService.ProcessPaymentAsync(paymentDto);

                if (result.IsSuccess)
                {
                    return Ok(result);
                }

                return BadRequest(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing payment");
                return StatusCode(500, "Internal server error occurred while processing payment");
            }
        }

        /// <summary>
        /// Handle PayPal webhooks for payment notifications
        /// </summary>
        [HttpPost("paypal/webhook")]
        [AllowAnonymous] // PayPal webhooks don't use authentication
        public async Task<IActionResult> PayPalWebhook()
        {
            try
            {
                using var reader = new StreamReader(Request.Body);
                var webhookPayload = await reader.ReadToEndAsync();

                // Log the webhook for debugging in sandbox
                _logger.LogInformation("PayPal Webhook received: {Payload}", webhookPayload);

                // In production, you should verify the webhook signature
                // For sandbox testing, we'll just acknowledge receipt

                // You can process different webhook events here
                // Common events: PAYMENT.CAPTURE.COMPLETED, PAYMENT.CAPTURE.DENIED, etc.

                return Ok(new { status = "success" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing PayPal webhook");
                return StatusCode(500, "Webhook processing failed");
            }
        }

        private string GetFrontendUrl()
        {
            // Get from configuration or environment variable in production
            return Environment.GetEnvironmentVariable("FRONTEND_URL") ?? "http://localhost:4200";
        }
    }

    // Additional DTO for refunds with better validation
    public class PayPalRefundDto
    {
        [Required(ErrorMessage = "Transaction ID is required")]
        [StringLength(50, ErrorMessage = "Transaction ID cannot exceed 50 characters")]
        public string TransactionId { get; set; } = string.Empty;

        [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than 0")]
        public decimal? Amount { get; set; }
    }
}