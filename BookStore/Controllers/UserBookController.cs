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

        public UserBookController(ApplicationDbContext context)
        {
            _context = context;
        }
        [HttpGet]
        public async Task<IActionResult> Index(string? search)
        {
            var booksQuery = _context.Books

                .Include(b => b.Category)

                .Include(b => b.Publisher)

                .Include(b => b.BookAuthors)
                    .ThenInclude(ba => ba.Author)

                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                booksQuery = booksQuery.Where(b =>

                    b.Title.Contains(search) ||

                    (b.Category != null &&
                     b.Category.Name.Contains(search)) ||

                    (b.Publisher != null &&
                     b.Publisher.Name.Contains(search)) ||

                    b.BookAuthors.Any(ba =>
                        ba.Author != null &&
                        ba.Author.Name.Contains(search))
                );
            }

            var books = await booksQuery
                .Select(b => new BookVM
                {
                    BookId = b.BookId,
                    Title = b.Title,
                    Price = b.Price,
                    StockQuantity = b.StockQuantity,
                    ImageUrl = b.ImageUrl
                })
                .ToListAsync();

            return View(books);
        }



    }
}
