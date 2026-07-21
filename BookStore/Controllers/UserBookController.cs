using BookStore.Services.Implementaion;
using Microsoft.AspNetCore.Mvc;

namespace BookStore.Controllers
{
    public class UserBookController : Controller
    {
        private readonly BookService _bookService;

        public UserBookController(BookService bookService)
        {
            _bookService = bookService;
        }

        [HttpGet]
        public async Task<IActionResult> Index(string? search, int page = 1)
        {
            var books = await _bookService.GetAllBooksAsync(search, page);
            return View(books);
        }

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var book = await _bookService.GetBookDetailsAsync(id);

            if (book == null)
                return NotFound();

            return View(book);
        }
    }
}
