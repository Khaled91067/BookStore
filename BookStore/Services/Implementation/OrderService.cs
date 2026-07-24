using BookStore.Data;
using BookStore.Models;
using BookStore.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace BookStore.Services.Implementaion
{
    public class OrderService
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<OrderService> _logger;

        public OrderService(ApplicationDbContext context, UserManager<ApplicationUser> userManager, ILogger<OrderService> logger)
        {
            _context = context;
            _userManager = userManager;
            _logger = logger;
        }

        public async Task<bool> AddToCart(int bookId, List<OrderItemVM> cart, int quantity = 1)
        {
            var book = await _context.Books.FindAsync(bookId);

            if (book == null)
            {
                _logger.LogWarning("AddToCart: Book not found {BookId}", bookId);
                return false;
            }

            var item = cart.FirstOrDefault(x => x.ProductId == bookId);

            if (item != null)
            {
                if (item.Quantity + quantity > book.StockQuantity)
                {
                    _logger.LogWarning("AddToCart blocked for book {BookId} '{Title}': stock limit {StockQuantity}", bookId, book.Title, book.StockQuantity);
                    return false;
                }

                item.Quantity += quantity;
            }
            else
            {
                if (quantity > book.StockQuantity)
                {
                    _logger.LogWarning("AddToCart blocked for book {BookId} '{Title}': stock limit {StockQuantity}", bookId, book.Title, book.StockQuantity);
                    return false;
                }

                cart.Add(new OrderItemVM
                {
                    ProductId = book.BookId,
                    ProductName = book.Title,
                    Price = book.Price,
                    Quantity = quantity
                });
            }

            _logger.LogDebug("Book {BookId} '{Title}' added/updated in cart with quantity {Quantity}", bookId, book.Title, quantity);
            return true;
        }

        public bool RemoveFromCart(int bookId, List<OrderItemVM> cart)
        {
            var item = cart.FirstOrDefault(x => x.ProductId == bookId);
            if (item == null)
            {
                _logger.LogWarning("RemoveFromCart: item {BookId} not found in cart", bookId);
                return false;
            }

            cart.Remove(item);
            _logger.LogDebug("Book {BookId} removed from cart", bookId);
            return true;
        }

        public async Task<int?> PlaceOrder(string userId, List<OrderItemVM> cart)
        {
            if (cart == null || !cart.Any())
            {
                _logger.LogWarning("PlaceOrder called with empty cart for user {UserId}", userId);
                return null;
            }

            _logger.LogInformation("Placing order for user {UserId}, {ItemCount} item(s)", userId, cart.Count);

            foreach (var item in cart)
            {
                var book = await _context.Books.FindAsync(item.ProductId);

                if (book is null)
                {
                    _logger.LogWarning("PlaceOrder aborted: book {BookId} not found", item.ProductId);
                    return null;
                }

                if (book.StockQuantity < item.Quantity)
                {
                    _logger.LogWarning("PlaceOrder rejected: insufficient stock for book {BookId} '{Title}' (requested {Requested}, available {Available})",
                        item.ProductId, book.Title, item.Quantity, book.StockQuantity);
                    return null;
                }

                book.StockQuantity -= item.Quantity;
            }

            Order order = new Order
            {
                OrderDate = DateTime.UtcNow,
                UserId = userId,
                OrderItems = new List<OrderItem>()
            };

            foreach (var item in cart)
            {
                order.OrderItems.Add(new OrderItem
                {
                    BookId = item.ProductId,
                    Quantity = item.Quantity,
                    Price = item.Price
                });
            }

            Payment payment = new Payment
            {
                Order = order,
                Amount = order.OrderItems.Sum(i => i.Price * i.Quantity),
                PaymentStatus = PaymentStatus.Pending,
                PaymentMethod = PaymentMethod.Cash
            };

            order.TotalAmount = order.OrderItems.Sum(i => i.Price * i.Quantity);

            _context.Orders.Add(order);
            _context.Payments.Add(payment);

            await _context.SaveChangesAsync();

            _logger.LogInformation("Order {OrderId} placed for user {UserId}, total {TotalAmount:C}", order.OrderId, userId, order.TotalAmount);
            return order.OrderId;
        }

        public async Task<CheckOutVM?> PrepareCheckOutVMAsync(int id, ApplicationUser user)
        {
            var order = await _context.Orders.Where(o => o.OrderId == id && o.UserId == user.Id)
                                             .Include(o => o.OrderItems)
                                             .ThenInclude(i => i.Book)
                                             .FirstOrDefaultAsync();
            if (order == null)
            {
                _logger.LogWarning("Checkout order {OrderId} not found or does not belong to user {UserId}", id, user.Id);
                return null;
            }

            CheckOutVM vm = new CheckOutVM
            {
                OrderId = order.OrderId,
                Address = user.Address ?? string.Empty,
                PhoneNumber = user.PhoneNumber ?? string.Empty,
                Email = user.Email ?? string.Empty,
                FirstName = user.FirstName ?? string.Empty,
                LastName = user.LastName ?? string.Empty,

                OrderItems = order.OrderItems.Select(i => new OrderItemVM
                {
                    ProductId = i.BookId,
                    ProductName = i.Book != null ? i.Book.Title : string.Empty,
                    ImageUrl = i.Book != null ? i.Book.ImageUrl : null,
                    Price = i.Price,
                    Quantity = i.Quantity
                }).ToList()
            };

            return vm;
        }

        public async Task<List<OrderVM>> GetUserOrdersAsync(string userId, string? searchTerm)
        {
            var query = _context.Orders
               .Where(o => o.UserId == userId);

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                query = query.Where(o =>
                    o.OrderItems.Any(i =>
                        i.Book != null && i.Book.Title.Contains(searchTerm)));
            }

            return await query
                .OrderByDescending(o => o.OrderDate)
                .Select(o => new OrderVM
                {
                    OrderId = o.OrderId,
                    OrderDate = o.OrderDate,
                    CustomerName = o.User != null ? (o.User.UserName ?? string.Empty) : string.Empty,
                    TotalAmount = o.OrderItems.Sum(i => i.Price * i.Quantity),
                    PaymentStatus = o.Payments.Where(p => p.OrderId == o.OrderId)
                                              .OrderByDescending(p => p.CreatedAt)
                                              .Select(p => p.PaymentStatus)
                                              .FirstOrDefault(),

                    OrderItems = o.OrderItems.Select(i => new OrderItemVM
                    {
                        ProductId = i.BookId,
                        ProductName = i.Book != null ? i.Book.Title : string.Empty,
                        ImageUrl = i.Book != null ? i.Book.ImageUrl : null,
                        Quantity = i.Quantity,
                        Price = i.Price
                    }).ToList()
                })
                .ToListAsync();
        }

        public async Task<bool> IsOrderPaidAsync(int orderId)
        {
            return await _context.Payments
                .AnyAsync(p => p.OrderId == orderId && p.PaymentStatus == PaymentStatus.Succeeded);
        }

        public async Task UpdatePaymentMethodAsync(int orderId, PaymentMethod method)
        {
            var payment = await _context.Payments
                .Where(p => p.OrderId == orderId)
                .OrderByDescending(p => p.PaymentId)
                .FirstOrDefaultAsync();

            if (payment != null)
            {
                payment.PaymentMethod = method;
                await _context.SaveChangesAsync();
            }
        }

        public async Task UpdateUserAddressAsync(ApplicationUser user, CheckOutVM vm)
        {
            if (user != null)
            {
                user.FirstName = vm.FirstName;
                user.LastName = vm.LastName;
                user.Address = vm.Address;
                user.PhoneNumber = vm.PhoneNumber;

                await _userManager.UpdateAsync(user);
            }
        }
    }
}
