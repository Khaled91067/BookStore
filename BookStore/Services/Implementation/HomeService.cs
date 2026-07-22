using BookStore.Data;
using BookStore.Models;
using BookStore.ViewModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace BookStore.Services.Implementaion
{
    public class HomeService
    {
        private const string HomeDataCacheKey = "Home_Data";

        private readonly ApplicationDbContext _context;
        private readonly ILogger<HomeService> _logger;
        private readonly IMemoryCache _cache;

        public HomeService(ApplicationDbContext context, ILogger<HomeService> logger, IMemoryCache cache)
        {
            _context = context;
            _logger = logger;
            _cache = cache;
        }

        public async Task<HomeVM> GetHomeDataAsync()
        {
            return (await _cache.GetOrCreateAsync(HomeDataCacheKey, async entry =>
            {
                entry.SlidingExpiration = TimeSpan.FromMinutes(5);

                var featuredBooks = await _context.Books
                    .Include(b => b.Category)
                    .Include(b => b.BookAuthors)
                    .ThenInclude(ba => ba.Author)
                    .OrderByDescending(b => b.BookId) // newest 4 books
                    .Take(4)
                    .ToListAsync();

                var categories = await _context.Categories
                    .Include(c => c.Books)
                    .ToListAsync();

                var featuredAuthors = await _context.Authors
                    .Include(a => a.BookAuthors)
                    .Take(4)
                    .ToListAsync();

                _logger.LogDebug("Home page data loaded from database: {BookCount} books, {CategoryCount} categories, {AuthorCount} authors",
                    featuredBooks.Count, categories.Count, featuredAuthors.Count);

                return new HomeVM
                {
                    FeaturedBooks = featuredBooks,
                    Categories = categories,
                    FeaturedAuthors = featuredAuthors
                };
            }))!;
        }
    }
}
