using BookStore.Data;
using Xunit;
using BookStore.Models;
using BookStore.Services.Implementaion;
using BookStore.ViewModels;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace BookStore.Tests.Services;

public class BookServiceTests : IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly Mock<IWebHostEnvironment> _envMock;
    private readonly BookService _service;

    public BookServiceTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _context = new ApplicationDbContext(options);
        _envMock = new Mock<IWebHostEnvironment>();
        _envMock.Setup(e => e.WebRootPath).Returns(Path.GetTempPath());

        _service = new BookService(_context, _envMock.Object, NullLogger<BookService>.Instance);
    }

    public void Dispose() => _context.Dispose();

    // ──────────────────────────────────────────────────────────────────
    // GetAllBooksAsync
    // ──────────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetAllBooksAsync_NoSearch_ReturnsAllBooks()
    {
        // Arrange
        _context.Books.AddRange(
            new Book { Title = "Clean Code", Price = 29.99m, StockQuantity = 5, Description = "", CategoryId = 1, PublisherId = 1 },
            new Book { Title = "The Pragmatic Programmer", Price = 35.00m, StockQuantity = 3, Description = "", CategoryId = 1, PublisherId = 1 }
        );
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.GetAllBooksAsync(null, 1);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Books.Count);
    }

    [Fact]
    public async Task GetAllBooksAsync_WithSearch_FiltersBooksByTitle()
    {
        // Arrange – seed Category and Publisher so navigation props aren't null
        // during the in-memory LINQ evaluation of the compound OR search query.
        _context.Categories.Add(new Category { CategoryId = 1, Name = "Programming" });
        _context.Publishers.Add(new Publisher { PublisherId = 1, Name = "O'Reilly" });
        _context.Books.AddRange(
            new Book { Title = "Clean Code", Price = 29.99m, StockQuantity = 5, Description = "", CategoryId = 1, PublisherId = 1 },
            new Book { Title = "The Pragmatic Programmer", Price = 35.00m, StockQuantity = 3, Description = "", CategoryId = 1, PublisherId = 1 }
        );
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.GetAllBooksAsync("Clean", 1);

        // Assert
        Assert.Single(result.Books);
        Assert.Equal("Clean Code", result.Books[0].Title);
    }

    [Fact]
    public async Task GetAllBooksAsync_ReturnsCorrectTotalPages()
    {
        // Arrange – add 9 books; page size is 8 so we expect 2 pages
        for (int i = 1; i <= 9; i++)
        {
            _context.Books.Add(new Book
            {
                Title = $"Book {i}",
                Price = 10m,
                StockQuantity = i,
                Description = "",
                CategoryId = 1,
                PublisherId = 1
            });
        }
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.GetAllBooksAsync(null, 1);

        // Assert
        Assert.Equal(2, result.TotalPages);
        Assert.Equal(1, result.CurrentPage);
    }

    // ──────────────────────────────────────────────────────────────────
    // DeleteAsync
    // ──────────────────────────────────────────────────────────────────

    [Fact]
    public async Task DeleteAsync_BookExists_ReturnsTrue()
    {
        // Arrange
        var book = new Book { Title = "To Delete", Price = 5m, StockQuantity = 1, Description = "", CategoryId = 1, PublisherId = 1 };
        _context.Books.Add(book);
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.DeleteAsync(book.BookId);

        // Assert
        Assert.True(result);
        Assert.Equal(0, await _context.Books.CountAsync());
    }

    [Fact]
    public async Task DeleteAsync_BookNotFound_ReturnsFalse()
    {
        // Act
        var result = await _service.DeleteAsync(9999);

        // Assert
        Assert.False(result);
    }

    // ──────────────────────────────────────────────────────────────────
    // SaveAsync
    // ──────────────────────────────────────────────────────────────────

    [Fact]
    public async Task SaveAsync_NewBook_CreatesBook()
    {
        // Arrange
        var vm = new BookVM
        {
            BookId = 0,
            Title = "New Book",
            Price = 19.99m,
            StockQuantity = 10,
            CategoryId = 1,
            PublisherId = 1,
            Description = "A great book",
            SelectedAuthorIds = new List<int>()
        };

        // Act
        var result = await _service.SaveAsync(vm);

        // Assert
        Assert.True(result);
        Assert.Equal(1, await _context.Books.CountAsync());
        Assert.Equal("New Book", (await _context.Books.FirstAsync()).Title);
    }

    [Fact]
    public async Task SaveAsync_ExistingBook_UpdatesBook()
    {
        // Arrange
        var book = new Book { Title = "Old Title", Price = 5m, StockQuantity = 2, Description = "", CategoryId = 1, PublisherId = 1 };
        _context.Books.Add(book);
        await _context.SaveChangesAsync();

        var vm = new BookVM
        {
            BookId = book.BookId,
            Title = "Updated Title",
            Price = 15m,
            StockQuantity = 5,
            CategoryId = 1,
            PublisherId = 1,
            Description = "Updated description",
            SelectedAuthorIds = new List<int>()
        };

        // Act
        var result = await _service.SaveAsync(vm);

        // Assert
        Assert.True(result);
        var updated = await _context.Books.FindAsync(book.BookId);
        Assert.Equal("Updated Title", updated!.Title);
        Assert.Equal(15m, updated.Price);
    }

    [Fact]
    public async Task SaveAsync_ExistingBook_BookNotFound_ReturnsFalse()
    {
        // Arrange
        var vm = new BookVM
        {
            BookId = 9999,   // doesn't exist
            Title = "Ghost",
            Price = 1m,
            StockQuantity = 1,
            CategoryId = 1,
            PublisherId = 1,
            Description = "",
            SelectedAuthorIds = new List<int>()
        };

        // Act
        var result = await _service.SaveAsync(vm);

        // Assert
        Assert.False(result);
    }
}
