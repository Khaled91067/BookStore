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
            if (User.IsInRole("Admin"))
            {
                return RedirectToAction("Index", "Dashboard", new { area = "Admin" });
            }

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

            var featuredAuthors = await _context.Authors
                .Include(a => a.BookAuthors)
                .Take(4)
                .ToListAsync();

            var viewModel = new HomeVM
            {
                FeaturedBooks = featuredBooks,
                Categories = categories,
                FeaturedAuthors = featuredAuthors
            };

            return View(viewModel);
        }

        public IActionResult Error()
        {
            return View();
        }
    }
}
