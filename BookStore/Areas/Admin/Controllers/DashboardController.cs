using BookStore.Services.Implementaion;
using BookStore.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BookStore.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class DashboardController : Controller
    {
        private readonly DashboardService _dashboardService;

        public DashboardController(DashboardService dashboardService)
        {
            _dashboardService = dashboardService;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                var viewModel = await _dashboardService.GetDashboardDataAsync();
                return View(viewModel);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Error loading dashboard metrics: " + ex.Message);
                return View(new AdminDashboardVM());
            }
        }
    }
}
