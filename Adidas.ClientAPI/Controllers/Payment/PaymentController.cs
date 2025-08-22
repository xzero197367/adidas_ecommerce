using System.ComponentModel.DataAnnotations;
using Adidas.Application.Contracts.ServicesContracts.Operation;
using Adidas.Application.Contracts.ServicesContracts.Feature;
using Adidas.DTOs.Operation.PaymentDTOs;
using Adidas.DTOs.Operation.PaymentDTOs.PaypalDtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Adidas.Models.Operation;

namespace Adidas.ClientAPI.Controllers.Payment
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class PaymentController : ControllerBase
    {
        private readonly IPaymentService _paymentService;
        private readonly IPayPalService _payPalService;
        private readonly IOrderService _orderService;
        private readonly IShoppingCartService _cartService;
        private readonly ILogger<PaymentController> _logger;

        public PaymentController(
            IPaymentService paymentService,
            IPayPalService payPalService,
            IOrderService orderService,
            IShoppingCartService cartService,
            ILogger<PaymentController> logger)
        {
            _paymentService = paymentService;
            _payPalService = payPalService;
            _orderService = orderService;
            _cartService = cartService;
            _logger = logger;
        }

        /// <summary>
        /// Get payment by ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetPayment(Guid id)
        {
            try
            {
                var result = await _paymentService.GetPaymentByIdAsync(id);

                if (result.IsSuccess)
                    return Ok(new { success = true, data = result.Data });

                return BadRequest(new { success = false, message = result.ErrorMessage });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting payment {PaymentId}", id);
                return StatusCode(500, new { success = false, message = "Internal server error" });
            }
        }

        /// <summary>
        /// Create a new PayPal payment
        /// </summary>
        [HttpPost("paypal/create")]
        public async Task<IActionResult> CreatePayPalPayment([FromBody] PayPalCreatePaymentDto createDto)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                _logger.LogInformation("Creating PayPal payment for user {UserId}, Order {OrderId}", userId, createDto.OrderId);

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
                        success = false,
                        message = "Validation failed",
                        errors = errors
                    });
                }

                var result = await _payPalService.CreatePaymentAsync(createDto);

                if (result.IsSuccess)
                {
                    _logger.LogInformation("PayPal payment created successfully with ID {PaymentId} for Order {OrderId}",
                        result.Data?.PaymentId, createDto.OrderId);

                    return Ok(new
                    {
                        success = true,
                        data = result.Data,
                        message = "PayPal payment created successfully. Redirect user to ApprovalUrl."
                    });
                }

                _logger.LogWarning("PayPal payment creation failed for Order {OrderId}: {Message}", createDto.OrderId, result.ErrorMessage);
                return BadRequest(new { success = false, message = result.ErrorMessage });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating PayPal payment for Order {OrderId}", createDto.OrderId);
                return StatusCode(500, new
                {
                    success = false,
                    message = "Internal server error occurred while creating payment"
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
                    return BadRequest(new { success = false, message = "Invalid request data" });
                }

                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                _logger.LogInformation("Executing PayPal payment {PaymentId} for payer {PayerId}, User {UserId}",
                    executeDto.PaymentId, executeDto.PayerId, userId);

                var result = await _payPalService.ExecutePaymentAsync(executeDto.PaymentId, executeDto.PayerId);

                if (result.IsSuccess)
                {
                    _logger.LogInformation("PayPal payment executed successfully: {PaymentId}", executeDto.PaymentId);

                    // Complete the checkout process
                    await CompleteCheckoutProcess(result.Data.OrderId, result.Data.Id, userId);

                    return Ok(new
                    {
                        success = true,
                        data = result.Data,
                        message = "Payment executed successfully"
                    });
                }

                _logger.LogWarning("PayPal payment execution failed: {Message}", result.ErrorMessage);
                return BadRequest(new { success = false, message = result.ErrorMessage });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error executing PayPal payment {PaymentId}", executeDto.PaymentId);
                return StatusCode(500, new
                {
                    success = false,
                    message = "Internal server error occurred while executing payment"
                });
            }
        }

        /// <summary>
        /// Handle PayPal success return (GET endpoint for browser redirect)
        /// </summary>
        [HttpGet("paypal/success")]
        [AllowAnonymous]
        public async Task<IActionResult> PayPalSuccess([FromQuery] string paymentId, [FromQuery] string PayerID)
        {
            try
            {
                if (string.IsNullOrEmpty(paymentId) || string.IsNullOrEmpty(PayerID))
                {
                    _logger.LogWarning("PayPal success callback missing parameters: paymentId={PaymentId}, PayerID={PayerID}",
                        paymentId, PayerID);
                    return Redirect($"{GetFrontendUrl()}/checkout/failed?error=missing_parameters");
                }

                _logger.LogInformation("PayPal success callback: PaymentId={PaymentId}, PayerID={PayerID}",
                    paymentId, PayerID);

                var result = await _payPalService.ExecutePaymentAsync(paymentId, PayerID);

                if (result.IsSuccess)
                {
                    _logger.LogInformation("PayPal payment executed successfully via callback: {PaymentId}", paymentId);

                    // Complete the checkout process
                    await CompleteCheckoutProcess(result.Data.OrderId, result.Data.Id, null);

                    return Redirect($"{GetFrontendUrl()}/checkout/success?orderId={result.Data.OrderId}&paymentId={result.Data.Id}");
                }

                _logger.LogWarning("PayPal payment execution failed via callback: {Message}", result.ErrorMessage);

                // Payment failed - clean up the order
                if (result.Data?.OrderId != null)
                {
                    await CleanupFailedOrder(result.Data.OrderId);
                }

                return Redirect($"{GetFrontendUrl()}/checkout/failed?error=payment_failed&message={Uri.EscapeDataString(result.ErrorMessage)}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in PayPal success callback for payment {PaymentId}", paymentId);
                return Redirect($"{GetFrontendUrl()}/checkout/failed?error=server_error");
            }
        }

        /// <summary>
        /// Handle PayPal cancel return
        /// </summary>
        [HttpGet("paypal/cancel")]
        [AllowAnonymous]
        public async Task<IActionResult> PayPalCancel([FromQuery] string token, [FromQuery] string orderId)
        {
            try
            {
                _logger.LogInformation("PayPal payment cancelled. Token: {Token}, OrderId: {OrderId}", token, orderId);

                // Clean up the order if provided
                if (!string.IsNullOrEmpty(orderId) && Guid.TryParse(orderId, out var orderGuid))
                {
                    await CleanupFailedOrder(orderGuid);
                }

                return Redirect($"{GetFrontendUrl()}/checkout/cancelled?token={token}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error handling PayPal cancel callback");
                return Redirect($"{GetFrontendUrl()}/checkout/failed?error=cancel_error");
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
                    return BadRequest(new { success = false, message = "Invalid payment data" });
                }

                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                _logger.LogInformation("Processing payment for Order {OrderId}, User {UserId}", paymentDto.OrderId, userId);

                var result = await _paymentService.ProcessPaymentAsync(paymentDto);

                if (result.IsSuccess)
                {
                    _logger.LogInformation("Payment processed successfully for Order {OrderId}", paymentDto.OrderId);

                    // Complete the checkout process
                    await CompleteCheckoutProcess(paymentDto.OrderId, result.Data.Id, userId);

                    return Ok(new
                    {
                        success = true,
                        data = result.Data,
                        message = "Payment processed successfully"
                    });
                }

                _logger.LogWarning("Payment processing failed for Order {OrderId}: {Message}", paymentDto.OrderId, result.ErrorMessage);

                // Payment failed - clean up the order
                await CleanupFailedOrder(paymentDto.OrderId);

                return BadRequest(new { success = false, message = result.ErrorMessage });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing payment for Order {OrderId}", paymentDto.OrderId);

                // Clean up on exception
                if (paymentDto?.OrderId != null)
                {
                    await CleanupFailedOrder(paymentDto.OrderId);
                }

                return StatusCode(500, new { success = false, message = "Internal server error occurred while processing payment" });
            }
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
                    return Ok(new { success = true, data = result.Data });

                return BadRequest(new { success = false, message = result.ErrorMessage });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting PayPal payment details for {PaymentId}", paymentId);
                return StatusCode(500, new { success = false, message = "Internal server error occurred while retrieving payment details" });
            }
        }

        /// <summary>
        /// Refund a PayPal payment
        /// </summary>
        [HttpPost("paypal/refund")]
        public async Task<IActionResult> RefundPayPalPayment([FromBody] PaymentDto refundDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new { success = false, message = "Invalid refund data" });
                }

                var result = await _payPalService.RefundPaymentAsync(refundDto.TransactionId, refundDto.Amount);

                if (result.IsSuccess)
                {
                    return Ok(new
                    {
                        success = true,
                        data = result.Data,
                        message = "Refund processed successfully"
                    });
                }

                return BadRequest(new { success = false, message = result.ErrorMessage });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing PayPal refund for transaction {TransactionId}", refundDto.TransactionId);
                return StatusCode(500, new { success = false, message = "Internal server error occurred while processing refund" });
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
                    return Ok(new { success = true, data = result.Data });

                return BadRequest(new { success = false, message = result.ErrorMessage });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving pending payments");
                return StatusCode(500, new { success = false, message = "Internal server error occurred while retrieving pending payments" });
            }
        }

        /// <summary>
        /// Handle PayPal webhooks for payment notifications
        /// </summary>
        [HttpPost("paypal/webhook")]
        [AllowAnonymous]
        public async Task<IActionResult> PayPalWebhook()
        {
            try
            {
                using var reader = new StreamReader(Request.Body);
                var webhookPayload = await reader.ReadToEndAsync();

                _logger.LogInformation("PayPal Webhook received: {Payload}", webhookPayload);

                // Process different webhook events here
                // Common events: PAYMENT.CAPTURE.COMPLETED, PAYMENT.CAPTURE.DENIED, etc.

                return Ok(new { status = "success" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing PayPal webhook");
                return StatusCode(500, "Webhook processing failed");
            }
        }

        /// <summary>
        /// Complete the checkout process after successful payment
        /// </summary>
        private async Task CompleteCheckoutProcess(Guid orderId, Guid paymentId, string userId)
        {
            try
            {
                _logger.LogInformation("Completing checkout process for Order {OrderId}, Payment {PaymentId}", orderId, paymentId);

                // Update order status to Processing
                var updateResult = await _orderService.UpdateOrderStatusAsync(orderId, OrderStatus.Processing);
                if (!updateResult.IsSuccess)
                {
                    _logger.LogWarning("Failed to update order status for Order {OrderId}: {Message}", orderId, updateResult.ErrorMessage);
                }

                // Clear user's cart if userId is provided
                if (!string.IsNullOrEmpty(userId))
                {
                    var clearCartResult = await _cartService.ClearCartAsync(userId);
                    if (!clearCartResult.IsSuccess)
                    {
                        _logger.LogWarning("Failed to clear cart for user {UserId} after successful payment", userId);
                    }
                }

                _logger.LogInformation("Checkout process completed successfully for Order {OrderId}", orderId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error completing checkout process for Order {OrderId}", orderId);
                // Don't throw - payment was successful, this is just cleanup
            }
        }

        /// <summary>
        /// Clean up order when payment fails
        /// </summary>
        private async Task CleanupFailedOrder(Guid orderId)
        {
            try
            {
                _logger.LogInformation("Cleaning up failed order {OrderId}", orderId);

                var deleteResult = await _orderService.DeleteAsync(orderId);
                if (!deleteResult.IsSuccess)
                {
                    _logger.LogWarning("Failed to delete order {OrderId} after payment failure: {Message}", orderId, deleteResult.ErrorMessage);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cleaning up failed order {OrderId}", orderId);
            }
        }

        private string GetFrontendUrl()
        {
            return Environment.GetEnvironmentVariable("FRONTEND_URL") ?? "http://localhost:4200";
        }
    }
}