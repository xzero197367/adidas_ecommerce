// Fixed PayPalRestService.cs
using Adidas.Application.Contracts.RepositoriesContracts.Operation;
using Adidas.Application.Contracts.ServicesContracts.Operation;
using Adidas.DTOs.CommonDTOs;
using Adidas.DTOs.Operation.PaymentDTOs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Text;
using System.Text.Json;
using Mapster;
using Adidas.DTOs.Operation.PaymentDTOs.PaypalDtos;
using System.Linq;
using System.Text.Json.Serialization;

namespace Adidas.Application.Services.Operation
{
    public class PayPalRestService : IPayPalService
    {
        private readonly HttpClient _httpClient;
        private readonly IPaymentRepository _paymentRepository;
        private readonly IPaymentService _paymentService;
        private readonly ILogger<PayPalRestService> _logger;
        private readonly IConfiguration _configuration;
        private readonly string _baseUrl;
        private readonly string _clientId;
        private readonly string _clientSecret;

        public PayPalRestService(
            HttpClient httpClient,
            IConfiguration configuration,
            IPaymentRepository paymentRepository,
            IPaymentService paymentService,
            ILogger<PayPalRestService> logger)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _paymentRepository = paymentRepository;
            _paymentService = paymentService;
            _logger = logger;

            _clientId = _configuration["PayPal:ClientId"];
            _clientSecret = _configuration["PayPal:Secret"];
            var mode = _configuration["PayPal:Mode"];

            _baseUrl = mode?.ToLower() == "live"
                ? "https://api-m.paypal.com"
                : "https://api-m.sandbox.paypal.com";
        }

        public async Task<OperationResult<PayPalPaymentDto>> CreatePaymentAsync(PayPalCreatePaymentDto createDto)
        {
            try
            {
                // Get access token
                var accessToken = await GetAccessTokenAsync();
                if (string.IsNullOrEmpty(accessToken))
                {
                    return OperationResult<PayPalPaymentDto>.Fail("Failed to get PayPal access token");
                }

                // Create order payload
                var orderPayload = new
                {
                    intent = "CAPTURE",
                    purchase_units = new[]
                    {
                        new
                        {
                            reference_id = createDto.OrderId.ToString(),
                            amount = new
                            {
                                currency_code = createDto.Currency,
                                value = createDto.Amount.ToString("F2")
                            },
                            description = createDto.Description ?? $"Order Payment - {createDto.OrderId}"
                        }
                    },
                    application_context = new
                    {
                        return_url = createDto.ReturnUrl,
                        cancel_url = createDto.CancelUrl,
                        brand_name = "Adidas Store",
                        landing_page = "BILLING",
                        user_action = "PAY_NOW",
                        shipping_preference = "NO_SHIPPING"
                    }
                };

                var json = JsonSerializer.Serialize(orderPayload, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                });

                _httpClient.DefaultRequestHeaders.Clear();
                _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {accessToken}");
                _httpClient.DefaultRequestHeaders.Add("PayPal-Request-Id", Guid.NewGuid().ToString());

                var response = await _httpClient.PostAsync(
                    $"{_baseUrl}/v2/checkout/orders",
                    new StringContent(json, Encoding.UTF8, "application/json")
                );

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    var orderResponse = JsonSerializer.Deserialize<PayPalOrderResponse>(responseContent);

                    if (orderResponse?.Status == "CREATED")
                    {
                        var approvalUrl = orderResponse.Links?.FirstOrDefault(l => l.Rel == "approve")?.Href;

                        // Create payment record in database - FIX: Create the Payment entity directly
                        var payment = new Payment
                        {
                            Id = Guid.NewGuid(),
                            OrderId = createDto.OrderId,
                            Amount = createDto.Amount,
                            PaymentMethod = "PayPal",
                            PaymentStatus = "PayPal_Created",
                            TransactionId = orderResponse.Id, // Store PayPal Order ID
                            CreatedAt = DateTime.UtcNow,
                            ProcessedAt = DateTime.UtcNow, // FIX: Add required ProcessedAt
                            IsActive = true,
                            GatewayResponse = $"PayPal Order Created: {orderResponse.Id}"
                        };

                        // Add payment directly to repository
                        var createdPayment = await _paymentRepository.AddAsync(payment);
                        await _paymentRepository.SaveChangesAsync();

                        var payPalPaymentDto = new PayPalPaymentDto
                        {
                            PaymentId = orderResponse.Id,
                            ApprovalUrl = approvalUrl,
                            Status = orderResponse.Status,
                            Amount = createDto.Amount,
                            Currency = createDto.Currency,
                            OrderId = createDto.OrderId
                        };

                        _logger.LogInformation("PayPal payment created successfully. Order ID: {OrderId}", orderResponse.Id);
                        return OperationResult<PayPalPaymentDto>.Success(payPalPaymentDto);
                    }
                }

                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError("PayPal order creation failed. Status: {StatusCode}, Response: {Response}",
                    response.StatusCode, errorContent);
                return OperationResult<PayPalPaymentDto>.Fail($"PayPal order creation failed: {response.StatusCode}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating PayPal payment");
                return OperationResult<PayPalPaymentDto>.Fail("Error creating PayPal payment");
            }
        }

        public async Task<OperationResult<PaymentDto>> ExecutePaymentAsync(string paymentId, string payerId)
        {
            try
            {
                var accessToken = await GetAccessTokenAsync();
                if (string.IsNullOrEmpty(accessToken))
                {
                    return OperationResult<PaymentDto>.Fail("Failed to get PayPal access token");
                }

                _httpClient.DefaultRequestHeaders.Clear();
                _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {accessToken}");

                var response = await _httpClient.PostAsync(
                    $"{_baseUrl}/v2/checkout/orders/{paymentId}/capture",
                    new StringContent("{}", Encoding.UTF8, "application/json")
                );

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    var captureResponse = JsonSerializer.Deserialize<PayPalCaptureResponse>(responseContent);

                    // Find our payment record - Use the approach that matches your repository
                    var paymentRecords = await _paymentRepository.FindAsync(p =>
                        p.Where(x => x.TransactionId == paymentId && !x.IsDeleted));

                    Payment paymentRecord;

                    // Try to cast to IEnumerable first (for collections)
                    if (paymentRecords is IEnumerable<Payment> collection)
                    {
                        paymentRecord = collection.FirstOrDefault();
                    }
                    else
                    {
                        // If it's a single Payment object
                        paymentRecord = paymentRecords as Payment;
                    }

                    if (paymentRecord == null)
                    {
                        _logger.LogError("Payment record not found for PayPal order: {PaymentId}", paymentId);
                        return OperationResult<PaymentDto>.Fail("Payment record not found");
                    }

                    if (captureResponse?.Status == "COMPLETED")
                    {
                        var captureId = captureResponse.PurchaseUnits?[0]?.Payments?.Captures?[0]?.Id;

                        paymentRecord.PaymentStatus = "Completed";
                        paymentRecord.TransactionId = captureId ?? paymentId;
                        paymentRecord.ProcessedAt = DateTime.UtcNow;
                        paymentRecord.UpdatedAt = DateTime.UtcNow;
                        paymentRecord.GatewayResponse = $"PayPal Capture Completed: {captureId}";

                        await _paymentRepository.UpdateAsync(paymentRecord);
                        await _paymentRepository.SaveChangesAsync();

                        _logger.LogInformation("PayPal payment executed successfully. Capture ID: {CaptureId}", captureId);
                        return OperationResult<PaymentDto>.Success(paymentRecord.Adapt<PaymentDto>());
                    }
                    else
                    {
                        paymentRecord.PaymentStatus = "Failed";
                        paymentRecord.ProcessedAt = DateTime.UtcNow;
                        paymentRecord.UpdatedAt = DateTime.UtcNow;
                        paymentRecord.GatewayResponse = $"PayPal Capture Failed. Status: {captureResponse?.Status}";

                        await _paymentRepository.UpdateAsync(paymentRecord);
                        await _paymentRepository.SaveChangesAsync();

                        return OperationResult<PaymentDto>.Fail($"PayPal payment capture failed: {captureResponse?.Status}");
                    }
                }

                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError("PayPal payment capture failed. Status: {StatusCode}, Response: {Response}",
                    response.StatusCode, errorContent);
                return OperationResult<PaymentDto>.Fail($"PayPal payment capture failed: {response.StatusCode}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error executing PayPal payment");
                return OperationResult<PaymentDto>.Fail("Error executing PayPal payment");
            }
        }

        public async Task<OperationResult<PaymentDto>> GetPaymentDetailsAsync(string paymentId)
        {
            try
            {
                var paymentRecords = await _paymentRepository.FindAsync(p =>
                    p.Where(x => x.TransactionId == paymentId && !x.IsDeleted));

                Payment paymentRecord;

                // Try to cast to IEnumerable first (for collections)
                if (paymentRecords is IEnumerable<Payment> collection)
                {
                    paymentRecord = collection.FirstOrDefault();
                }
                else
                {
                    // If it's a single Payment object
                    paymentRecord = paymentRecords as Payment;
                }

                if (paymentRecord != null)
                {
                    return OperationResult<PaymentDto>.Success(paymentRecord.Adapt<PaymentDto>());
                }

                return OperationResult<PaymentDto>.Fail("Payment record not found");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting PayPal payment details");
                return OperationResult<PaymentDto>.Fail("Error getting payment details");
            }
        }

        public async Task<OperationResult<PaymentDto>> RefundPaymentAsync(string transactionId, decimal? amount = null)
        {
            try
            {
                var accessToken = await GetAccessTokenAsync();
                if (string.IsNullOrEmpty(accessToken))
                {
                    return OperationResult<PaymentDto>.Fail("Failed to get PayPal access token");
                }

                object refundPayload = new { };
                if (amount.HasValue)
                {
                    refundPayload = new
                    {
                        amount = new
                        {
                            currency_code = "USD",
                            value = amount.Value.ToString("F2")
                        }
                    };
                }

                var json = JsonSerializer.Serialize(refundPayload);

                _httpClient.DefaultRequestHeaders.Clear();
                _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {accessToken}");

                var response = await _httpClient.PostAsync(
                    $"{_baseUrl}/v2/payments/captures/{transactionId}/refund",
                    new StringContent(json, Encoding.UTF8, "application/json")
                );

                if (response.IsSuccessStatusCode)
                {
                    // Find and update payment record
                    var paymentRecords = await _paymentRepository.FindAsync(p =>
                        p.Where(x => x.TransactionId == transactionId && !x.IsDeleted));

                    Payment paymentRecord;

                    // Try to cast to IEnumerable first (for collections)
                    if (paymentRecords is IEnumerable<Payment> collection)
                    {
                        paymentRecord = collection.FirstOrDefault();
                    }
                    else
                    {
                        // If it's a single Payment object
                        paymentRecord = paymentRecords as Payment;
                    }

                    if (paymentRecord != null)
                    {
                        paymentRecord.PaymentStatus = "Refunded";
                        paymentRecord.UpdatedAt = DateTime.UtcNow;
                        paymentRecord.GatewayResponse = "PayPal Refund Completed";

                        await _paymentRepository.UpdateAsync(paymentRecord);
                        await _paymentRepository.SaveChangesAsync();

                        return OperationResult<PaymentDto>.Success(paymentRecord.Adapt<PaymentDto>());
                    }
                }

                return OperationResult<PaymentDto>.Fail("Refund processing failed");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing PayPal refund");
                return OperationResult<PaymentDto>.Fail("Error processing refund");
            }
        }

        private async Task<string> GetAccessTokenAsync()
        {
            try
            {
                var authValue = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{_clientId}:{_clientSecret}"));

                _httpClient.DefaultRequestHeaders.Clear();
                _httpClient.DefaultRequestHeaders.Add("Authorization", $"Basic {authValue}");

                var requestBody = "grant_type=client_credentials";
                var response = await _httpClient.PostAsync(
                    $"{_baseUrl}/v1/oauth2/token",
                    new StringContent(requestBody, Encoding.UTF8, "application/x-www-form-urlencoded")
                );

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    var tokenResponse = JsonSerializer.Deserialize<PayPalTokenResponse>(responseContent);
                    return tokenResponse?.AccessToken;
                }

                _logger.LogError("Failed to get PayPal access token. Status: {StatusCode}", response.StatusCode);
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting PayPal access token");
                return null;
            }
        }
    }

}