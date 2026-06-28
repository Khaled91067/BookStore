using Microsoft.AspNetCore.Mvc;

namespace BookStore.Controllers
{
    public class InfoController : Controller
    {
        public IActionResult About()
        {
            return View();
        }

        public IActionResult FAQs()
        {
            return View();
        }

       

    }
}
