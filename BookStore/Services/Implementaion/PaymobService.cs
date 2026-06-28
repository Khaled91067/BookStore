using BookStore.Data;
using BookStore.DTOs.Paymob;
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
        public PaymobService(HttpClient httpClient,IConfiguration configuration,ApplicationDbContext context)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _context = context;
        }

        public async Task<OrderVM> GetPaymentResultAsync(int orderId)
        {
            var payment = await _context.Payments
                .Where(p => p.OrderId == orderId)
                .OrderByDescending(p => p.PaymentId)
                .FirstOrDefaultAsync();

            if (payment == null)
                return null;

            var vm = new OrderVM
            {
                OrderId = orderId,
                PaymentStatus = payment.PaymentStatus,
                TransactionId = payment.TransactionId
            };
            return vm;
        }

        public async Task<string> CreateIntentionAsync(int orderId)
        {
            var order = await _context.Orders
                .Include(o => o.User)
                .Include(o => o.OrderItems)
                .Include(o => o.Payments)
                .FirstOrDefaultAsync(o => o.OrderId == orderId);

            if (order == null)
                throw new Exception("Order not found");


            var payment = order.Payments
                 .OrderByDescending(p => p.PaymentId)
                 .FirstOrDefault();

            if (payment == null)
                throw new Exception("Payment not found");

            var request = new HttpRequestMessage(HttpMethod.Post, "https://accept.paymob.com/v1/intention/");
            var secretKey = _configuration["Paymob:SecretKey"];

            request.Headers.Add("Authorization", $"Token {secretKey}");

            var totalPrice = order.OrderItems
                .Sum(i => i.Price * i.Quantity);

            var body = new CreateIntentionRequestDto
            {
                Amount = (int)(totalPrice * 100),
                Currency = "EGP",
                PaymentMethods = new[] { 5733305 },
                BillingData = new BillingDataDto
                {
                    FirstName = order.User.FirstName,
                    LastName = order.User.LastName,
                    Email = order.User.Email,
                    PhoneNumber = order.User.PhoneNumber,
                    Address = order.User.Address
                },
                NotificationUrl = "https://backup-ambition-certified.ngrok-free.dev/Payment/Webhook",
                RedirectionUrl = $"https://backup-ambition-certified.ngrok-free.dev/Payment/PaymentResult?orderId={order.OrderId}"
            };

            var json = JsonSerializer.Serialize(body);

            var content = new StringContent(json, Encoding.UTF8, "application/json");

            request.Content = content;
            var response = await _httpClient.SendAsync(request);

            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadAsStringAsync();

            var publicKey = _configuration["Paymob:PublicKey"];

            var intentionResponse = JsonSerializer.Deserialize<CreateIntentionResponseDto>(result);

            var clientSecret = intentionResponse.ClientSecret;

            var paymobOrderId = intentionResponse.IntentionOrderId;

            var checkoutUrl = $"https://accept.paymob.com/unifiedcheckout/?publicKey={publicKey}&clientSecret={clientSecret}";

            payment.ProviderReference = paymobOrderId.ToString();

            await _context.SaveChangesAsync();
            return checkoutUrl;
        }

        public async Task ProcessWebhookAsync(PaymobWebHookDto webhook)
        {
            var success = webhook.Obj.Success;
            var paymobOrderId = webhook.Obj.Order.Id;
            var transactionId = webhook.Obj.Id;

            var payment = await _context.Payments
                .FirstOrDefaultAsync(p =>p.ProviderReference == paymobOrderId.ToString());

            if (payment == null)
                return ;

            if (success)
            {
                payment.PaymentStatus = PaymentStatus.Succeeded;
                payment.TransactionId = transactionId.ToString();
                payment.PaymentMethod = PaymentMethod.Paymob;
            }
            else
            {
                payment.PaymentStatus = PaymentStatus.Failed;
            }

            await _context.SaveChangesAsync();

        }

    }
}
