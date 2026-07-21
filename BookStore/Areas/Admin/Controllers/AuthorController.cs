using BookStore.Services.Implementaion;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BookStore.Areas.Admin.Controllers
{
    [Authorize(Roles = "Admin")]
    [Area("Admin")]
    public class AuthorController : Controller
    {
        private readonly AuthorService _authorService;
        private readonly ILogger<AuthorController> _logger;

        public AuthorController(AuthorService authorService, ILogger<AuthorController> logger)
        {
            _authorService = authorService;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            var authors = await _authorService.GetAllAuthorsAsync();
            return View(authors);
        }

        [HttpPost]
        public async Task<IActionResult> Add(string authorName, IFormFile? imageFile)
        {
            await _authorService.AddAuthorAsync(authorName, imageFile);
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            if (!await _authorService.DeleteAuthorAsync(id))
            {
                _logger.LogWarning("Delete author {AuthorId} blocked: author has associated books", id);
                TempData["Error"] = "Cannot delete author because they have books associated with them.";
            }

            return RedirectToAction("Index");
        }
    }
}
