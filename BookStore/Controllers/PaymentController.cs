using BookStore.Data;
using BookStore.DTOs.Paymob;
using BookStore.Models;
using BookStore.Services.Implementaion;
using BookStore.ViewModels;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.Json;

namespace BookStore.Controllers
{
    public class PaymentController : Controller
    {
        private readonly PaymobService _paymobService;
        private readonly IConfiguration _configuration;
        private readonly ApplicationDbContext _context;

        public PaymentController(IConfiguration configuration,PaymobService paymobService,ApplicationDbContext context)
        {
            _configuration = configuration;
            _paymobService = paymobService;
            _context = context;
        }

        public async Task<IActionResult> PaymentResult(int? orderId)
        {
            if (orderId == null)
                return RedirectToAction("ViewOrders");

            var vm = await _paymobService.GetPaymentResultAsync(orderId.Value);

            if (vm == null)
                return RedirectToAction("ViewOrders");

            return View(vm);
        }

      
        [HttpPost]
        public async Task<IActionResult> CreateIntention(int orderId)
        {
            var checkoutUrl = await _paymobService.CreateIntentionAsync(orderId);

            return Redirect(checkoutUrl);
        }

        [HttpPost]
        public async Task<IActionResult> Webhook()
        {
            using var reader = new StreamReader(Request.Body);

            var body = await reader.ReadToEndAsync();

            var webhook = JsonSerializer.Deserialize<PaymobWebHookDto>(body);

            if (webhook is null)
                return BadRequest();

            await _paymobService.ProcessWebhookAsync(webhook);

            return Ok();
        }


    }
}
