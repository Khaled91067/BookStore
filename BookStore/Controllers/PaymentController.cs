using BookStore.Data;
using BookStore.DTOs.Paymob;
using BookStore.Models;
using BookStore.Services.Implementaion;
using BookStore.Services.Interfaces;
using BookStore.ViewModels;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using System.Text;
using System.Text.Json;

namespace BookStore.Controllers
{
    [EnableRateLimiting("StrictPolicy")]
    public class PaymentController : Controller
    {
        private readonly IPaymobService _paymobService;
        private readonly IConfiguration _configuration;
        private readonly ILogger<PaymentController> _logger;

        public PaymentController(
            IConfiguration configuration,
            IPaymobService paymobService,
            ILogger<PaymentController> logger)
        {
            _configuration = configuration;
            _paymobService = paymobService;
            _logger = logger;
        }

        public async Task<IActionResult> PaymentResult(int? orderId, string? success, string? id, string? pending)
        {
            if (orderId == null)
            {
                _logger.LogWarning("PaymentResult: no orderId provided");
                return RedirectToAction("ViewOrders", "Order");
            }

            if (!string.IsNullOrEmpty(success))
            {
                await _paymobService.UpdatePaymentStatusFromCallbackAsync(orderId.Value, success, id, pending);
            }

            var vm = await _paymobService.GetPaymentResultAsync(orderId.Value);

            if (vm == null)
            {
                _logger.LogWarning("PaymentResult: no payment found for order {OrderId}", orderId);
                return RedirectToAction("ViewOrders", "Order");
            }

            return View(vm);
        }

        [HttpPost]
        public async Task<IActionResult> Webhook()
        {
            using var reader = new StreamReader(Request.Body);
            var body = await reader.ReadToEndAsync();

            var webhook = JsonSerializer.Deserialize<PaymobWebHookDto>(body);

            if (webhook is null)
            {
                _logger.LogWarning("Webhook: received null or malformed payload");
                return BadRequest();
            }

            await _paymobService.ProcessWebhookAsync(webhook);

            return Ok();
        }
    }
}
