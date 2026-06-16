using BookStore.Services.Implementaion;
using Microsoft.AspNetCore.Mvc;

namespace BookStore.Controllers
{
    public class PaymentController : Controller
    {
        private readonly PaymobService _paymobService;
        private readonly IConfiguration _configuration;

        public PaymentController(IConfiguration configuration,PaymobService paymobService)
        {
            _configuration = configuration;
            _paymobService = paymobService;
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




    }
}
