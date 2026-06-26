using BookStore.Data;
using BookStore.Models;
using BookStore.Services.Implementaion;
using BookStore.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace BookStore.Controllers
{
    
    public class UserBookController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly BookService _bookService;

        public UserBookController(ApplicationDbContext context,BookService bookService)
        {
            _context = context;
            _bookService = bookService;
        }

        [HttpGet]
        public async Task<IActionResult> Index(string? search,int page=1)
        {
            var books = await _bookService.GetAllBooksAsync(search,page);
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
