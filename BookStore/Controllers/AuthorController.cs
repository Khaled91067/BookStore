using BookStore.Data;
using BookStore.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace BookStore.Controllers
{
    public class AuthorController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AuthorController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var author = await _context.Authors
                .Include(a => a.BookAuthors)
                .ThenInclude(ba => ba.Book)
                .FirstOrDefaultAsync(a => a.AuthorId == id);

            if (author == null)
            {
                return NotFound();
            }

            var vm = new AuthorDetailsVM
            {
                AuthorId = author.AuthorId,
                Name = author.Name,
                ImageUrl = author.ImageUrl,
                Books = author.BookAuthors
                    .Where(ba => ba.Book != null)
                    .Select(ba => new BookVM
                    {
                        BookId = ba.Book.BookId,
                        Title = ba.Book.Title,
                        Price = ba.Book.Price,
                        StockQuantity = ba.Book.StockQuantity,
                        Description = ba.Book.Description,
                        ImageUrl = ba.Book.ImageUrl
                    })
                    .ToList()
            };

            return View(vm);
        }
    }
}
