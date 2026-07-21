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
    public class AuthorController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly ILogger<AuthorController> _logger;

        public AuthorController(ApplicationDbContext context, IWebHostEnvironment webHostEnvironment, ILogger<AuthorController> logger)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            var authors = await _context.Authors.ToListAsync();
            return View(authors);
        }

        [HttpPost]
        public async Task<IActionResult> Add(string authorName, IFormFile? imageFile)
        {
            if (!string.IsNullOrWhiteSpace(authorName))
            {
                var author = new Author
                {
                    Name = authorName
                };

                if (imageFile != null && imageFile.Length > 0)
                {
                    string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "images", "authors");
                    if (!Directory.Exists(uploadsFolder))
                        Directory.CreateDirectory(uploadsFolder);

                    string uniqueFileName = Guid.NewGuid().ToString() + "_" + imageFile.FileName;
                    string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await imageFile.CopyToAsync(fileStream);
                    }

                    author.ImageUrl = uniqueFileName;
                    _logger.LogDebug("Author image saved: {FilePath}", filePath);
                }

                _context.Authors.Add(author);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Author created: {AuthorId} '{AuthorName}'", author.AuthorId, authorName);
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var hasBooks = await _context.BookAuthors.AnyAsync(ba => ba.AuthorId == id);
            if (hasBooks)
            {
                _logger.LogWarning("Delete author {AuthorId} blocked: author has associated books", id);
                TempData["Error"] = "Cannot delete author because they have books associated with them.";
                return RedirectToAction("Index");
            }

            var author = await _context.Authors.FindAsync(id);

            if (author != null)
            {
                if (!string.IsNullOrEmpty(author.ImageUrl))
                {
                    var imagePath = author.ImageUrl.Contains("/")
                        ? Path.Combine(_webHostEnvironment.WebRootPath, author.ImageUrl.TrimStart('/'))
                        : Path.Combine(_webHostEnvironment.WebRootPath, "images", "authors", author.ImageUrl);
                    if (System.IO.File.Exists(imagePath))
                    {
                        System.IO.File.Delete(imagePath);
                    }
                }

                _context.Authors.Remove(author);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Author deleted: {AuthorId} '{AuthorName}'", id, author.Name);
            }

            return RedirectToAction("Index");
        }
    }
}
