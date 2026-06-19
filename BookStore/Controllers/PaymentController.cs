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

        public async Task<IActionResult> PaymentResult(int orderId)
        {

            if (orderId == null)
                return RedirectToAction("ViewOrders");

            var payment = await _context.Payments
                .Where(p => p.OrderId == orderId)
                .OrderByDescending(p => p.PaymentId)
                .FirstOrDefaultAsync();

            if (payment == null)
                return RedirectToAction("ViewOrders");

            var vm = new OrderVM
            {
                OrderId = orderId,
                PaymentStatus = payment.PaymentStatus,
                TransactionId = payment.TransactionId
            };

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

            using var json = JsonDocument.Parse(body);

            var success = json.RootElement
                .GetProperty("obj")
                .GetProperty("success")
                .GetBoolean();


            var paymobOrderId = json.RootElement
                .GetProperty("obj")
                .GetProperty("order")
                .GetProperty("id")
                .GetInt64();

            var transactionId = json.RootElement
                .GetProperty("obj")
                .GetProperty("id")
                .GetInt64();

            var payment = await _context.Payments
                .FirstOrDefaultAsync(p =>
                    p.ProviderReference == paymobOrderId.ToString());

            if (payment == null)
                return Ok();

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

            return Ok();
        }


    }
}
