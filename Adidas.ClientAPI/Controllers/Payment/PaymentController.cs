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
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var result = await _payPalService.CreatePaymentAsync(createDto);

                if (result.IsSuccess)
                {
                    // Return the approval URL for frontend to redirect
                    return Ok(new
                    {
                        Success = true,
                        Data = result.Data,
                        Message = "PayPal payment created successfully. Redirect user to ApprovalUrl."
                    });
                }

                return BadRequest(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating PayPal payment");
                return StatusCode(500, "Internal server error occurred while creating payment");
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

                var result = await _payPalService.ExecutePaymentAsync(executeDto.PaymentId, executeDto.PayerId);

                if (result.IsSuccess)
                {
                    return Ok(new
                    {
                        Success = true,
                        Data = result.Data,
                        Message = "Payment executed successfully"
                    });
                }

                return BadRequest(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error executing PayPal payment");
                return StatusCode(500, "Internal server error occurred while executing payment");
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
                    return Redirect($"{GetFrontendUrl()}/payment/failed?error=missing_parameters");
                }

                var result = await _payPalService.ExecutePaymentAsync(paymentId, PayerID);

                if (result.IsSuccess)
                {
                    // Redirect to success page with payment details
                    return Redirect($"{GetFrontendUrl()}/payment/success?paymentId={result.Data.Id}&transactionId={result.Data.TransactionId}");
                }

                return Redirect($"{GetFrontendUrl()}/payment/failed?error=execution_failed");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in PayPal success callback");
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
            var result = await _payPalService.GetPaymentDetailsAsync(paymentId);

            if (result.IsSuccess)
                return Ok(result);

            return BadRequest(result);
        }

        /// <summary>
        /// Refund a PayPal payment
        /// </summary>
        [HttpPost("paypal/refund")]
        public async Task<IActionResult> RefundPayPalPayment([FromBody] PayPalRefundDto refundDto)
        {
            try
            {
                var result = await _payPalService.RefundPaymentAsync(refundDto.TransactionId, refundDto.Amount);

                if (result.IsSuccess)
                {
                    return Ok(new
                    {
                        Success = true,
                        Data = result.Data,
                        Message = "Refund processed successfully"
                    });
                }

                return BadRequest(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing PayPal refund");
                return StatusCode(500, "Internal server error occurred while processing refund");
            }
        }

        /// <summary>
        /// Get all pending payments
        /// </summary>
        [HttpGet("pending")]
        public async Task<IActionResult> GetPendingPayments()
        {
            var result = await _paymentService.GetPendingPaymentsAsync();

            if (result.IsSuccess)
                return Ok(result);

            return BadRequest(result);
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
            // في الـ Sandbox هتخليها localhost:4200 (الـ Angular app بتاعك)
            // في الإنتاج حط لينك موقعك الحقيقي (مثلا https://myadidasclone.com)
            return "http://localhost:4200";
        }
    }

    // Additional DTO for refunds

    
    public class PayPalRefundDto
    {
        [Required]
        public string TransactionId { get; set; }

        [Range(0.01, double.MaxValue)]
        public decimal? Amount { get; set; }
    }
}

