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

        public async Task<bool> PlaceOrder(string userId, List<OrderItemVM> cart)
        {
            if (cart == null || !cart.Any())
                return false;

            Order order = new Order
            {
                OrderDate = DateTime.Now,
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
            return true;

        }



    }
}
