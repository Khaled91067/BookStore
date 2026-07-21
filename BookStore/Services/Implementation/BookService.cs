using BookStore.Data;
using BookStore.Models;
using BookStore.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace BookStore.Services.Implementaion
{
    public class BookService
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly ILogger<BookService> _logger;

        public BookService(ApplicationDbContext context, IWebHostEnvironment webHostEnvironment, ILogger<BookService> logger)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
            _logger = logger;
        }

        public async Task<BooksPageVM> GetAllBooksAsync(string? search, int page)
        {
            const int pageSize = 8;

            var booksQuery = _context.Books.AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                _logger.LogDebug("Searching books with term: {SearchTerm}", search);
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

            var totalBooks = await booksQuery.CountAsync();
            booksQuery = booksQuery
                .Skip((page - 1) * pageSize)
                .Take(pageSize);

            var books = await booksQuery
                .Select(b => new BookVM
                {
                    BookId = b.BookId,
                    Title = b.Title,
                    Price = b.Price,
                    StockQuantity = b.StockQuantity,
                    Description = b.Description,
                    ImageUrl = b.ImageUrl
                })
                .ToListAsync();

            return new BooksPageVM
            {
                Books = books,
                SearchTerm = search,
                CurrentPage = page,
                TotalPages = (int)Math.Ceiling(totalBooks / (double)pageSize)
            };
        }

        public async Task<BookVM?> GetAddEdit(int id)
        {
            BookVM vm = new BookVM();

            await LoadDataAsync(vm);

            if (id != 0)
            {
                /*var book = _context.Books.Find(id);*/
                var book = await _context.Books
                    .Include(b => b.BookAuthors)
                    .FirstOrDefaultAsync(b => b.BookId == id);

                if (book == null)
                {
                    _logger.LogWarning("Book not found for edit: {BookId}", id);
                    return null;
                }

                vm.BookId = book.BookId;
                vm.Title = book.Title;
                vm.Price = book.Price;
                vm.Description = book.Description;
                vm.ImageUrl = book.ImageUrl;
                vm.CategoryId = book.CategoryId;
                vm.StockQuantity = book.StockQuantity;
                vm.PublisherId = book.PublisherId;

                vm.SelectedAuthorIds = book.BookAuthors
                    .Select(ba => ba.AuthorId)
                    .ToList();
            }
            /*ModelState.Clear();*/

            return vm;
        }

        public async Task<bool> SaveAsync(BookVM vm)
        {
            string? fileName = vm.ImageUrl;

            if (vm.ImageFile != null)
            {
                var uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "Images/Books");
                var newFileName = Guid.NewGuid().ToString() + "_" + vm.ImageFile.FileName;
                string path = Path.Combine(uploadsFolder, newFileName);

                using (var stream = new FileStream(path, FileMode.Create))
                {
                    vm.ImageFile.CopyTo(stream);
                }
                fileName = newFileName;
                _logger.LogDebug("Book image saved: {ImagePath}", path);
            }

            if (vm.BookId == 0)
            {
                var book = new Book
                {
                    Title = vm.Title,
                    Price = vm.Price,
                    ImageUrl = fileName,
                    CategoryId = vm.CategoryId,
                    Description = vm.Description,
                    StockQuantity = vm.StockQuantity,
                    PublisherId = vm.PublisherId
                };
                _context.Books.Add(book);

                await _context.SaveChangesAsync();

                foreach (var authorId in vm.SelectedAuthorIds)
                {
                    _context.BookAuthors.Add(new BookAuthor
                    {
                        BookId = book.BookId,
                        AuthorId = authorId
                    });
                }

                _logger.LogInformation("Book created: {BookId} '{Title}'", book.BookId, book.Title);
            }
            else
            {
                var book = _context.Books.Find(vm.BookId);

                if (book == null)
                {
                    _logger.LogWarning("Book not found for update: {BookId}", vm.BookId);
                    return false;
                }

                book.Title = vm.Title;
                book.Price = vm.Price;
                book.ImageUrl = fileName;
                book.Description = vm.Description;
                book.CategoryId = vm.CategoryId;
                book.StockQuantity = vm.StockQuantity;
                book.PublisherId = vm.PublisherId;

                var existingAuthors = _context.BookAuthors
                 .Where(ba => ba.BookId == book.BookId);

                _context.BookAuthors.RemoveRange(existingAuthors);

                foreach (var authorId in vm.SelectedAuthorIds)
                {
                    _context.BookAuthors.Add(new BookAuthor
                    {
                        BookId = book.BookId,
                        AuthorId = authorId
                    });
                }

                _logger.LogInformation("Book updated: {BookId} '{Title}'", book.BookId, book.Title);
            }

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var book = await _context.Books.FindAsync(id);

            if (book == null)
            {
                _logger.LogWarning("Book not found for deletion: {BookId}", id);
                return false;
            }

            if (!string.IsNullOrEmpty(book.ImageUrl))
            {
                var path = Path.Combine(_webHostEnvironment.WebRootPath, "images", book.ImageUrl);

                if (System.IO.File.Exists(path))
                    System.IO.File.Delete(path);
                else
                    _logger.LogWarning("Image file not found during book deletion: {ImagePath}", path);
            }

            _context.Books.Remove(book);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Book deleted: {BookId} '{Title}'", id, book.Title);
            return true;
        }

        public async Task<BookDetailsVM> GetBookDetailsAsync(int id)
        {
            // TODO: Resolve nullable reference warnings (CS8600, CS8602)
            var result = await _context.Books
                .Include(b => b.Category)
                .Include(b => b.Publisher)
                .Include(b => b.BookAuthors)
                .ThenInclude(ba => ba.Author)
                .Where(b => b.BookId == id)
                .Select(b => new BookDetailsVM
                {
                    BookId = b.BookId,
                    Title = b.Title,
                    Description = b.Description,
                    Price = b.Price,
                    ImageUrl = b.ImageUrl,
                    StockQuantity = b.StockQuantity,
                    CategoryName = b.Category.Name,
                    PublisherName = b.Publisher.Name,
                    Authors = b.BookAuthors
                                .Select(ba => new AuthorDto
                                {
                                    AuthorId = ba.AuthorId,
                                    Name = ba.Author.Name
                                })
                                .ToList()
                })
                .FirstOrDefaultAsync();

            if (result == null)
                _logger.LogWarning("Book details not found: {BookId}", id);

            return result;
        }

        public async Task LoadDataAsync(BookVM vm)
        {
            vm.Categories = _context.Categories.ToList();
            vm.Publishers = _context.Publishers.ToList();
            vm.Authors = await _context.Authors
                .Select(a => new SelectListItem
                {
                    Value = a.AuthorId.ToString(),
                    Text = a.Name
                })
                .ToListAsync();
        }
    }
}
