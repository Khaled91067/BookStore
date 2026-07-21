using BookStore.Data;
using BookStore.DTOs.Paymob;
using BookStore.Helpers;
using BookStore.Models;
using BookStore.Services.Interfaces;
using BookStore.ViewModels;
using Microsoft.EntityFrameworkCore;
using System.Text;
using System.Text.Json;

namespace BookStore.Services.Implementaion
{
    public class PaymobService : IPaymobService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly ApplicationDbContext _context;
        private readonly ILogger<PaymobService> _logger;

        public PaymobService(HttpClient httpClient, IConfiguration configuration, ApplicationDbContext context, ILogger<PaymobService> logger)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _context = context;
            _logger = logger;
        }

        public async Task<OrderVM> GetPaymentResultAsync(int orderId)
        {
            var payment = await _context.Payments
                .Where(p => p.OrderId == orderId)
                .OrderByDescending(p => p.PaymentId)
                .FirstOrDefaultAsync();

            if (payment == null)
            {
                _logger.LogWarning("GetPaymentResult: no payment found for order {OrderId}", orderId);
                return null;
            }

            var vm = new OrderVM
            {
                OrderId = orderId,
                PaymentStatus = payment.PaymentStatus,
                PaymentMethod = payment.PaymentMethod,
                TransactionId = payment.TransactionId
            };
            return vm;
        }

        public async Task<string> CreateIntentionAsync(int orderId)
        {
            _logger.LogInformation("Creating Paymob payment intention for order {OrderId}", orderId);

            var order = await _context.Orders
                .Include(o => o.User)
                .Include(o => o.OrderItems)
                .Include(o => o.Payments)
                .FirstOrDefaultAsync(o => o.OrderId == orderId);

            if (order == null)
            {
                _logger.LogError("CreateIntention failed: order {OrderId} not found", orderId);
                throw new Exception("Order not found");
            }

            var payment = order.Payments
                 .OrderByDescending(p => p.PaymentId)
                 .FirstOrDefault();

            if (payment == null)
            {
                _logger.LogError("CreateIntention failed: no payment record for order {OrderId}", orderId);
                throw new Exception("Payment not found");
            }

            var request = new HttpRequestMessage(HttpMethod.Post, "https://accept.paymob.com/v1/intention/");
            var secretKey = _configuration["Paymob:SecretKey"];

            request.Headers.Add("Authorization", $"Token {secretKey}");

            var totalPrice = order.OrderItems
                .Sum(i => i.Price * i.Quantity);

            var baseUrl = (_configuration["Paymob:BaseUrl"] ?? "https://bookstore.khaled303.dev").TrimEnd('/');

            var body = new CreateIntentionRequestDto
            {
                Amount = (int)(totalPrice * 100),
                Currency = "EGP",
                PaymentMethods = new[] { 5733305 },
                BillingData = new BillingDataDto
                {
                    FirstName = order.User.FirstName,
                    LastName = order.User.LastName,
                    Email = order.User.Email,       // not logged — masked elsewhere
                    PhoneNumber = order.User.PhoneNumber,
                    Address = order.User.Address
                },
                NotificationUrl = $"{baseUrl}/Payment/Webhook",
                RedirectionUrl = $"{baseUrl}/Payment/PaymentResult?orderId={order.OrderId}"
            };

            var json = JsonSerializer.Serialize(body);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            request.Content = content;

            var response = await _httpClient.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Paymob API call failed for order {OrderId}: HTTP {StatusCode}", orderId, (int)response.StatusCode);
                response.EnsureSuccessStatusCode(); // rethrows
            }

            var result = await response.Content.ReadAsStringAsync();
            var publicKey = _configuration["Paymob:PublicKey"];
            var intentionResponse = JsonSerializer.Deserialize<CreateIntentionResponseDto>(result);

            var clientSecret = intentionResponse.ClientSecret;
            var paymobOrderId = intentionResponse.IntentionOrderId;

            var checkoutUrl = $"https://accept.paymob.com/unifiedcheckout/?publicKey={publicKey}&clientSecret={clientSecret}";

            payment.ProviderReference = paymobOrderId.ToString();
            await _context.SaveChangesAsync();

            _logger.LogInformation("Paymob intention created for order {OrderId}, provider reference {ProviderReference}",
                orderId, LogMask.TransactionId(paymobOrderId.ToString()));

            return checkoutUrl;
        }

        public async Task ProcessWebhookAsync(PaymobWebHookDto webhook)
        {
            var success = webhook.Obj.Success;
            var paymobOrderId = webhook.Obj.Order.Id;
            var transactionId = webhook.Obj.Id;

            _logger.LogInformation("Paymob webhook received: paymobOrderId {PaymobOrderId}, success={Success}",
                LogMask.TransactionId(paymobOrderId.ToString()), success);

            var payment = await _context.Payments
                .FirstOrDefaultAsync(p => p.ProviderReference == paymobOrderId.ToString());

            if (payment == null)
            {
                _logger.LogWarning("Webhook: no payment found for provider reference {ProviderReference}",
                    LogMask.TransactionId(paymobOrderId.ToString()));
                return;
            }

            // Verify callback amount matches expected payment amount (in cents)
            var expectedCents = (long)Math.Round(payment.Amount * 100);
            if (webhook.Obj.AmountCents != expectedCents)
            {
                _logger.LogWarning("Webhook amount mismatch for order {OrderId}: expected {Expected} cents, received {Received} cents",
                    payment.OrderId, expectedCents, webhook.Obj.AmountCents);
                return;
            }

            if (success)
            {
                payment.PaymentStatus = PaymentStatus.Succeeded;
                payment.TransactionId = transactionId.ToString();
                payment.PaymentMethod = PaymentMethod.Paymob;

                _logger.LogInformation("Payment SUCCEEDED for order {OrderId}, transaction {TransactionId}",
                    payment.OrderId, LogMask.TransactionId(transactionId.ToString()));
            }
            else
            {
                payment.PaymentStatus = PaymentStatus.Failed;
                _logger.LogWarning("Payment FAILED for order {OrderId}, transaction {TransactionId}",
                    payment.OrderId, LogMask.TransactionId(transactionId.ToString()));
            }

            await _context.SaveChangesAsync();
        }

        public async Task<bool> UpdatePaymentStatusFromCallbackAsync(int orderId, string? success, string? id, string? pending)
        {
            if (string.IsNullOrEmpty(success))
                return false;

            bool isSuccess = string.Equals(success, "true", StringComparison.OrdinalIgnoreCase);
            bool isPending = string.Equals(pending, "true", StringComparison.OrdinalIgnoreCase);

            var payment = await _context.Payments
                .Where(p => p.OrderId == orderId)
                .OrderByDescending(p => p.PaymentId)
                .FirstOrDefaultAsync();

            if (payment != null && payment.PaymentStatus == PaymentStatus.Pending && payment.PaymentMethod != PaymentMethod.Cash)
            {
                if (isSuccess)
                {
                    payment.PaymentStatus = PaymentStatus.Succeeded;
                    payment.TransactionId = id;
                    payment.PaymentMethod = PaymentMethod.Paymob;
                    _logger.LogInformation("PaymentResult GET callback: Payment SUCCEEDED for order {OrderId}, transaction {TransactionId}", orderId, id);
                }
                else if (!isPending)
                {
                    payment.PaymentStatus = PaymentStatus.Failed;
                    _logger.LogWarning("PaymentResult GET callback: Payment FAILED for order {OrderId}", orderId);
                }
                await _context.SaveChangesAsync();
                return true;
            }
            return false;
        }
    }
}
