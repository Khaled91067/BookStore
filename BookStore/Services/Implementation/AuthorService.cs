using BookStore.Data;
using BookStore.Models;
using BookStore.ViewModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace BookStore.Services.Implementaion
{
    public class AuthorService
    {
        private const string AuthorsCacheKey = "Authors_All";
        private const string HomeDataCacheKey = "Home_Data";

        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly ILogger<AuthorService> _logger;
        private readonly IMemoryCache _cache;

        public AuthorService(
            ApplicationDbContext context,
            IWebHostEnvironment webHostEnvironment,
            ILogger<AuthorService> logger,
            IMemoryCache cache)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
            _logger = logger;
            _cache = cache;
        }

        public async Task<List<Author>> GetAllAuthorsAsync()
        {
            return (await _cache.GetOrCreateAsync(AuthorsCacheKey, async entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(30);
                return await _context.Authors.ToListAsync();
            }))!;
        }

        public async Task AddAuthorAsync(string authorName, IFormFile? imageFile)
        {
            if (string.IsNullOrWhiteSpace(authorName))
                return;

            var author = new Author { Name = authorName };

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

            _cache.Remove(AuthorsCacheKey);
            _cache.Remove(HomeDataCacheKey);

            _logger.LogInformation("Author created: {AuthorId} '{AuthorName}'", author.AuthorId, authorName);
        }

        public async Task<bool> DeleteAuthorAsync(int id)
        {
            var hasBooks = await _context.BookAuthors.AnyAsync(ba => ba.AuthorId == id);
            if (hasBooks)
            {
                _logger.LogWarning("Delete author {AuthorId} blocked: author has associated books", id);
                return false;
            }

            var author = await _context.Authors.FindAsync(id);
            if (author == null)
            {
                _logger.LogWarning("DeleteAuthor: author {AuthorId} not found", id);
                return true; // nothing to delete — not an error
            }

            if (!string.IsNullOrEmpty(author.ImageUrl))
            {
                var imagePath = author.ImageUrl.Contains("/")
                    ? Path.Combine(_webHostEnvironment.WebRootPath, author.ImageUrl.TrimStart('/'))
                    : Path.Combine(_webHostEnvironment.WebRootPath, "images", "authors", author.ImageUrl);

                if (System.IO.File.Exists(imagePath))
                    System.IO.File.Delete(imagePath);
            }

            _context.Authors.Remove(author);
            await _context.SaveChangesAsync();

            _cache.Remove(AuthorsCacheKey);
            _cache.Remove(HomeDataCacheKey);

            _logger.LogInformation("Author deleted: {AuthorId} '{AuthorName}'", id, author.Name);
            return true;
        }

        public async Task<AuthorDetailsVM?> GetAuthorDetailsAsync(int id)
        {
            var author = await _context.Authors
                .Include(a => a.BookAuthors)
                .ThenInclude(ba => ba.Book)
                .FirstOrDefaultAsync(a => a.AuthorId == id);

            if (author == null)
            {
                _logger.LogWarning("GetAuthorDetails: author {AuthorId} not found", id);
                return null;
            }

            return new AuthorDetailsVM
            {
                AuthorId = author.AuthorId,
                Name = author.Name,
                ImageUrl = author.ImageUrl,
                Books = author.BookAuthors
                    .Where(ba => ba.Book != null)
                    .Select(ba => new BookVM
                    {
                        BookId = ba.Book!.BookId,
                        Title = ba.Book.Title,
                        Price = ba.Book.Price,
                        StockQuantity = ba.Book.StockQuantity,
                        Description = ba.Book.Description,
                        ImageUrl = ba.Book.ImageUrl
                    })
                    .ToList()
            };
        }
    }
}
