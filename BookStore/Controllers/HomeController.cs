using BookStore.Services.Implementaion;
using Microsoft.AspNetCore.Mvc;

namespace BookStore.Controllers
{
    public class HomeController : Controller
    {
        private readonly HomeService _homeService;

        public HomeController(HomeService homeService)
        {
            _homeService = homeService;
        }

        public async Task<IActionResult> Index()
        {
            if (User.IsInRole("Admin"))
            {
                return RedirectToAction("Index", "Dashboard", new { area = "Admin" });
            }

            var viewModel = await _homeService.GetHomeDataAsync();
            return View(viewModel);
        }

        public IActionResult Error()
        {
            return View();
        }
    }
}
