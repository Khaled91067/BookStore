using BookStore.Data;
using BookStore.Models;
using BookStore.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace BookStore.Controllers
{
    
    public class UserBookController : Controller
    {
        private readonly ApplicationDbContext _context;
        
        public UserBookController(ApplicationDbContext context, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            
        }
        public async Task<IActionResult> Index()
        {
            var books = _context.Books
        .Select(b => new BookVM
        {
            BookId = b.BookId,
            Title = b.Title,
            Price = b.Price,
            StockQuantity = b.StockQuantity,
            ImageUrl = b.ImageUrl
        })
        .ToList();

            return View(books);
        }

        

    }
}
