using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BookStore.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class DashboardController : Controller
    {
        public IActionResult Index()
        {
            // Default to V2 (Premium Dashboard)
            return RedirectToAction(nameof(V2));
        }

        public IActionResult V1()
        {
            return View();
        }

        public IActionResult V2()
        {
            return View();
        }
    }
}
