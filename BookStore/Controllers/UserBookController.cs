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
        public async Task<IActionResult> Index(string? search)
        {
            var books = await _bookService.GetAllBooksAsync(search);
            return View(books);
        }



    }
}
