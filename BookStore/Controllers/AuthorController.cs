using BookStore.Services.Implementaion;
using Microsoft.AspNetCore.Mvc;

namespace BookStore.Controllers
{
    public class AuthorController : Controller
    {
        private readonly AuthorService _authorService;

        public AuthorController(AuthorService authorService)
        {
            _authorService = authorService;
        }

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var vm = await _authorService.GetAuthorDetailsAsync(id);

            if (vm == null)
                return NotFound();

            return View(vm);
        }
    }
}
