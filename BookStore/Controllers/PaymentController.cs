using BookStore.Data;
using BookStore.Services.Implementaion;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography.X509Certificates;
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

        public IActionResult Success()
        {
            return View();
        }

        public IActionResult Failed()
        {
            return View();
        }

        public async Task<IActionResult> TestIntention()
        {
            var clientSecret =
                await _paymobService.CreateIntention();

            var publicKey =
                _configuration["Paymob:PublicKey"];

            var checkoutUrl =
                $"https://accept.paymob.com/unifiedcheckout/?" +
                $"publicKey={publicKey}" +
                $"&clientSecret={clientSecret}";

            return Redirect(checkoutUrl);
        }


        [HttpPost]
        public async Task<IActionResult> CreateIntention(int orderId)
        {
            var order = await _context.Orders
                .Include(o => o.User)
                .Include(o => o.OrderItems)
                .FirstOrDefaultAsync(o => o.OrderId == orderId);

            if (order == null)
                return NotFound();

            // Create Intent
            var client = new HttpClient();
            var request = new HttpRequestMessage(HttpMethod.Post, "https://accept.paymob.com/v1/intention/");
            var secretKey = _configuration["Paymob:SecretKey"];

            request.Headers.Add("Authorization", $"Token {secretKey}");
           
            var totalPrice = order.OrderItems
                .Sum(i => i.Price * i.Quantity);

            var body = new
            {
                amount = (int)(totalPrice * 100),
                currency = "EGP",
                payment_methods = new[] { 5733305 },
                billing_data = new
                {
                    first_name = order.User.FirstName,
                    last_name = order.User.LastName,
                    email = order.User.Email,
                    phone_number = "01092395887",
                    address = order.User.Address
                }
            };

            var json = JsonSerializer.Serialize(body);


            var content = new StringContent(json, null, "application/json");
            request.Content = content;
            var response = await client.SendAsync(request);

            response.EnsureSuccessStatusCode();







            var result = await response.Content.ReadAsStringAsync();

            var publicKey = _configuration["Paymob:PublicKey"];



            var json2 = JsonDocument.Parse(result);

            var clientSecret =
            json2.RootElement
            .GetProperty("client_secret")
            .GetString();

            var checkoutUrl = $"https://accept.paymob.com/unifiedcheckout/?publicKey={publicKey}&clientSecret={clientSecret}";

            return Redirect(checkoutUrl);

        }

       /* [HttpPost]
        public async Task<IActionResult> Webhook()
        {
            using var reader = new StreamReader(Request.Body);
            var body = await reader.ReadToEndAsync();
            var path = Path.Combine(
    Directory.GetCurrentDirectory(),
    "webhook.txt");

            System.IO.File.WriteAllText("webhook.txt", body);

            return Ok();
        }*/

        [HttpPost]
        public async Task<IActionResult> Webhook()
        {
            using var reader = new StreamReader(Request.Body);
            var body = await reader.ReadToEndAsync();

            var folder = @"C:\Temp";
            Directory.CreateDirectory(folder);

            var path = Path.Combine(folder, "webhook.txt");

            await System.IO.File.WriteAllTextAsync(path, body);

            return Ok();
        }



    }
}
