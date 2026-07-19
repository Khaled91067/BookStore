using BookStore.Data;
using BookStore.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace BookStore.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;

        public HomeController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var featuredBooks = await _context.Books
                .Include(b => b.Category)
                .Include(b => b.BookAuthors)
                .ThenInclude(ba => ba.Author)
                .OrderByDescending(b => b.BookId) // Display 6 newest books as featured
                .Take(4)
                .ToListAsync();

            var categories = await _context.Categories
                .Include(c => c.Books)
                .ToListAsync();

            var viewModel = new HomeVM
            {
                FeaturedBooks = featuredBooks,
                Categories = categories
            };

            return View(viewModel);
        }

        public IActionResult Error()
        {
            return View();
        }
    }
}
