using BookStore.Data;
using BookStore.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BookStore.Areas.Admin.Controllers
{
    [Authorize(Roles = "Admin")]
    [Area("Admin")]

    public class CategoryController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CategoryController(ApplicationDbContext context)
        {

            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var categories = await _context.Categories.ToListAsync();

            return View(categories);
        }

        [HttpPost]
        public async Task<IActionResult> Add(string catName)
        {
            if (!string.IsNullOrWhiteSpace(catName))
            {
                var category = new Category
                {
                    Name = catName
                };

                _context.Categories.Add(category);

                await _context.SaveChangesAsync();
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var category = await _context.Categories.FindAsync(id);

            if (category != null)
            {
                _context.Categories.Remove(category);

                await _context.SaveChangesAsync();
            }

            return RedirectToAction("Index");
        }


    }
}
