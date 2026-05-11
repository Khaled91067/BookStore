using BookStore.Data;
using BookStore.Models;
using BookStore.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static NuGet.Packaging.PackagingConstants;

namespace BookStore.Controllers
{
    public class OrderController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public OrderController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }


        public IActionResult AddToCart(int bookId)
        {
            var book = _context.Books.Find(bookId);

            if (book == null)
                return NotFound();

            var cart = HttpContext.Session.Get<List<OrderItemVM>>("Cart") ?? new List<OrderItemVM>();

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

            HttpContext.Session.Set("Cart", cart);

            return RedirectToAction("Index", "UserBook");
        }

        public IActionResult Cart()
        {
            var cart = HttpContext.Session.Get<List<OrderItemVM>>("Cart") ?? new List<OrderItemVM>();
            return View(cart);
        }





        [Authorize]
        [HttpPost]
        public IActionResult PlaceOrder()
        {
            var cart = HttpContext.Session
                .Get<List<OrderItemVM>>("Cart");

            if (cart == null || !cart.Any())
                return RedirectToAction("Cart");

            var userId = _userManager.GetUserId(User);

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
            _context.SaveChanges();

            HttpContext.Session.Remove("Cart");

            return RedirectToAction("Index", "Book");
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> ViewOrders()
        {
            var userId = _userManager.GetUserId(User);

            var userOrders = await _context.Orders.Where(o => o.UserId == userId)
                                                  .Include(o => o.OrderItems)
                                                  .ThenInclude(i=> i.Book)
                                                  .ToListAsync();


        return View("ViewOrders", userOrders);
        }
    }
}
