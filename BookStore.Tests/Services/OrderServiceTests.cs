using BookStore.Data;
using Xunit;
using BookStore.Models;
using BookStore.Services.Implementaion;
using BookStore.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace BookStore.Tests.Services;

public class OrderServiceTests : IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly OrderService _service;

    public OrderServiceTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _context = new ApplicationDbContext(options);

        var userManagerMock = new Mock<UserManager<ApplicationUser>>(
            Mock.Of<IUserStore<ApplicationUser>>(),
            null!, null!, null!, null!, null!, null!, null!, null!);

        _service = new OrderService(_context, userManagerMock.Object, NullLogger<OrderService>.Instance);
    }

    public void Dispose() => _context.Dispose();

    // ──────────────────────────────────────────────────────────────────
    // AddToCart
    // ──────────────────────────────────────────────────────────────────

    [Fact]
    public async Task AddToCart_BookNotFound_ReturnsFalse()
    {
        // Arrange
        var cart = new List<OrderItemVM>();

        // Act
        var result = await _service.AddToCart(9999, cart);

        // Assert
        Assert.False(result);
        Assert.Empty(cart);
    }

    [Fact]
    public async Task AddToCart_NewItem_AddsToCart()
    {
        // Arrange
        var book = new Book { Title = "C# in Depth", Price = 39.99m, StockQuantity = 10, Description = "", CategoryId = 1, PublisherId = 1 };
        _context.Books.Add(book);
        await _context.SaveChangesAsync();

        var cart = new List<OrderItemVM>();

        // Act
        var result = await _service.AddToCart(book.BookId, cart);

        // Assert
        Assert.True(result);
        Assert.Single(cart);
        Assert.Equal(1, cart[0].Quantity);
        Assert.Equal(book.BookId, cart[0].ProductId);
    }

    [Fact]
    public async Task AddToCart_ExistingItem_IncrementsQuantity()
    {
        // Arrange
        var book = new Book { Title = "DDIA", Price = 49.99m, StockQuantity = 10, Description = "", CategoryId = 1, PublisherId = 1 };
        _context.Books.Add(book);
        await _context.SaveChangesAsync();

        var cart = new List<OrderItemVM>
        {
            new OrderItemVM { ProductId = book.BookId, Quantity = 2, Price = book.Price, ProductName = book.Title }
        };

        // Act
        var result = await _service.AddToCart(book.BookId, cart);

        // Assert
        Assert.True(result);
        Assert.Single(cart);
        Assert.Equal(3, cart[0].Quantity);
    }

    [Fact]
    public async Task AddToCart_ExceedsStock_ReturnsFalse()
    {
        // Arrange
        var book = new Book { Title = "Rare Book", Price = 99m, StockQuantity = 2, Description = "", CategoryId = 1, PublisherId = 1 };
        _context.Books.Add(book);
        await _context.SaveChangesAsync();

        // Cart already holds the max quantity
        var cart = new List<OrderItemVM>
        {
            new OrderItemVM { ProductId = book.BookId, Quantity = 2, Price = book.Price, ProductName = book.Title }
        };

        // Act
        var result = await _service.AddToCart(book.BookId, cart);

        // Assert
        Assert.False(result);
        Assert.Equal(2, cart[0].Quantity); // unchanged
    }

    [Fact]
    public async Task AddToCart_MultipleQuantity_AddsToCartWithCustomQuantity()
    {
        // Arrange
        var book = new Book { Title = "Custom Book", Price = 10m, StockQuantity = 5, Description = "", CategoryId = 1, PublisherId = 1 };
        _context.Books.Add(book);
        await _context.SaveChangesAsync();

        var cart = new List<OrderItemVM>();

        // Act
        var result = await _service.AddToCart(book.BookId, cart, 3);

        // Assert
        Assert.True(result);
        Assert.Single(cart);
        Assert.Equal(3, cart[0].Quantity);
    }

    // ──────────────────────────────────────────────────────────────────
    // PlaceOrder
    // ──────────────────────────────────────────────────────────────────

    [Fact]
    public async Task PlaceOrder_EmptyCart_ReturnsNull()
    {
        // Act
        var result = await _service.PlaceOrder("user-1", new List<OrderItemVM>());

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task PlaceOrder_ValidCart_CreatesOrderAndReturnsId()
    {
        // Arrange
        var book = new Book { Title = "Refactoring", Price = 45m, StockQuantity = 5, Description = "", CategoryId = 1, PublisherId = 1 };
        _context.Books.Add(book);
        await _context.SaveChangesAsync();

        var cart = new List<OrderItemVM>
        {
            new OrderItemVM { ProductId = book.BookId, Quantity = 2, Price = book.Price, ProductName = book.Title }
        };

        // Act
        var orderId = await _service.PlaceOrder("user-1", cart);

        // Assert
        Assert.NotNull(orderId);
        var order = await _context.Orders.Include(o => o.OrderItems).FirstOrDefaultAsync(o => o.OrderId == orderId);
        Assert.NotNull(order);
        Assert.Single(order!.OrderItems);
        Assert.Equal(90m, order.TotalAmount);

        // Stock should be decremented
        var updatedBook = await _context.Books.FindAsync(book.BookId);
        Assert.Equal(3, updatedBook!.StockQuantity);
    }

    [Fact]
    public async Task PlaceOrder_InsufficientStock_ReturnsNull()
    {
        // Arrange
        var book = new Book { Title = "Scarce Book", Price = 20m, StockQuantity = 1, Description = "", CategoryId = 1, PublisherId = 1 };
        _context.Books.Add(book);
        await _context.SaveChangesAsync();

        var cart = new List<OrderItemVM>
        {
            new OrderItemVM { ProductId = book.BookId, Quantity = 5, Price = book.Price, ProductName = book.Title }
        };

        // Act
        var result = await _service.PlaceOrder("user-1", cart);

        // Assert
        Assert.Null(result);
        // No order should have been persisted
        Assert.Equal(0, await _context.Orders.CountAsync());
    }
}
