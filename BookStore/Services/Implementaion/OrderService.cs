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
        public OrderService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<bool> AddToCart(int bookId, List<OrderItemVM> cart)
        {

            var book = await _context.Books.FindAsync(bookId);

            if (book == null)
                return false;



            var item = cart.FirstOrDefault(x => x.ProductId == bookId);

            if (item != null)
            {
                if (item.Quantity >= book.StockQuantity)
                    return false;

                item.Quantity++;
            }

            else
            {
                cart.Add(new OrderItemVM
                {
                    ProductId = book.BookId,
                    ProductName = book.Title,
                    Price = book.Price,
                    Quantity = 1
                });
            }
            return true;
        }

        public async Task<int?> PlaceOrder(string userId, List<OrderItemVM> cart)
        {
            if (cart == null || !cart.Any())
                return null;

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

            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            Payment payment = new Payment
            {
                OrderId = order.OrderId,
                Amount = order.OrderItems.Sum(i => i.Price * i.Quantity),
                PaymentStatus = PaymentStatus.Pending,
                PaymentMethod = PaymentMethod.Cash
            };

            _context.Payments.Add(payment);

            await _context.SaveChangesAsync();

            return order.OrderId;

        }

        public async Task<CheckOutVM> PrepareCheckOutVMAsync(int id, ApplicationUser user)
        {
            var order = await _context.Orders.Where(o => o.OrderId == id)
                                             .Include(o => o.OrderItems)
                                             .ThenInclude(i => i.Book)
                                             .FirstOrDefaultAsync();
            if (order == null)
                return null;

            CheckOutVM vm = new CheckOutVM
            {
                OrderId = order.OrderId,
                Address = user?.Address,
                PhoneNumber = user?.PhoneNumber,
                Email = user?.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,


                OrderItems = order.OrderItems.Select(i => new OrderItemVM
                {

                    ProductId = i.BookId,
                    ProductName = i.Book.Title,
                    ImageUrl = i.Book.ImageUrl,
                    Price = i.Price,
                    Quantity = i.Quantity
                }).ToList()

            };

            return vm;

        }



    }
}
