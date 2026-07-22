using BookStore.Data;
using BookStore.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace BookStore.Services.Implementaion
{
    public class CategoryService
    {
        private const string CategoriesCacheKey = "Categories_All";
        private const string HomeDataCacheKey = "Home_Data";

        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly ILogger<CategoryService> _logger;
        private readonly IMemoryCache _cache;

        public CategoryService(
            ApplicationDbContext context,
            IWebHostEnvironment webHostEnvironment,
            ILogger<CategoryService> logger,
            IMemoryCache cache)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
            _logger = logger;
            _cache = cache;
        }

        public async Task<List<Category>> GetAllCategoriesAsync()
        {
            return (await _cache.GetOrCreateAsync(CategoriesCacheKey, async entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(30);
                return await _context.Categories.ToListAsync();
            }))!;
        }

        public async Task AddCategoryAsync(string categoryName, IFormFile? imageFile)
        {
            if (string.IsNullOrWhiteSpace(categoryName))
                return;

            var category = new Category { Name = categoryName };

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
                _logger.LogDebug("Category image saved: {FilePath}", filePath);
            }

            _context.Categories.Add(category);
            await _context.SaveChangesAsync();

            _cache.Remove(CategoriesCacheKey);
            _cache.Remove(HomeDataCacheKey);

            _logger.LogInformation("Category created: {CategoryId} '{CategoryName}'", category.CategoryId, categoryName);
        }

        public async Task<bool> DeleteCategoryAsync(int id)
        {
            var hasBooks = await _context.Books.AnyAsync(b => b.CategoryId == id);
            if (hasBooks)
            {
                _logger.LogWarning("Delete category {CategoryId} blocked: category has associated books", id);
                return false;
            }

            var category = await _context.Categories.FindAsync(id);
            if (category == null)
            {
                _logger.LogWarning("DeleteCategory: category {CategoryId} not found", id);
                return true; // nothing to delete — not an error
            }

            if (!string.IsNullOrEmpty(category.ImageUrl))
            {
                var imagePath = category.ImageUrl.Contains("/")
                    ? Path.Combine(_webHostEnvironment.WebRootPath, category.ImageUrl.TrimStart('/'))
                    : Path.Combine(_webHostEnvironment.WebRootPath, "images", "categories", category.ImageUrl);

                if (System.IO.File.Exists(imagePath))
                    System.IO.File.Delete(imagePath);
            }

            _context.Categories.Remove(category);
            await _context.SaveChangesAsync();

            _cache.Remove(CategoriesCacheKey);
            _cache.Remove(HomeDataCacheKey);

            _logger.LogInformation("Category deleted: {CategoryId} '{CategoryName}'", id, category.Name);
            return true;
        }
    }
}
