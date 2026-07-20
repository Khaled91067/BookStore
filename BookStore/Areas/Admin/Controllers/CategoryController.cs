using BookStore.Data;
using BookStore.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using System.IO;

namespace BookStore.Areas.Admin.Controllers
{
    [Authorize(Roles = "Admin")]
    [Area("Admin")]

    public class CategoryController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public CategoryController(ApplicationDbContext context, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
        }

        public async Task<IActionResult> Index()
        {
            var categories = await _context.Categories.ToListAsync();

            return View(categories);
        }

        [HttpPost]
        public async Task<IActionResult> Add(string catName, IFormFile? imageFile)
        {
            if (!string.IsNullOrWhiteSpace(catName))
            {
                var category = new Category
                {
                    Name = catName
                };

                if (imageFile != null && imageFile.Length > 0)
                {
                    string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "images", "categories");
                    if (!Directory.Exists(uploadsFolder))
                        Directory.CreateDirectory(uploadsFolder);

                    string uniqueFileName = Guid.NewGuid().ToString() + "_" + imageFile.FileName;
                    string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await imageFile.CopyToAsync(fileStream);
                    }

                    category.ImageUrl = uniqueFileName;
                }

                _context.Categories.Add(category);

                await _context.SaveChangesAsync();
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var hasBooks = await _context.Books.AnyAsync(b => b.CategoryId == id);
            if (hasBooks)
            {
                TempData["Error"] = "Cannot delete category because it has books associated with it.";
                return RedirectToAction("Index");
            }

            var category = await _context.Categories.FindAsync(id);

            if (category != null)
            {
                if (!string.IsNullOrEmpty(category.ImageUrl))
                {
                    var imagePath = category.ImageUrl.Contains("/")
                        ? Path.Combine(_webHostEnvironment.WebRootPath, category.ImageUrl.TrimStart('/'))
                        : Path.Combine(_webHostEnvironment.WebRootPath, "images", "categories", category.ImageUrl);
                    if (System.IO.File.Exists(imagePath))
                    {
                        System.IO.File.Delete(imagePath);
                    }
                }

                _context.Categories.Remove(category);

                await _context.SaveChangesAsync();
            }

            return RedirectToAction("Index");
        }


    }
}
