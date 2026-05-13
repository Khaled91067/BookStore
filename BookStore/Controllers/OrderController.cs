using BookStore.Data;
using BookStore.Models;
using BookStore.Services.Implementaion;
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
        private readonly OrderService _orderService;

        public OrderController(ApplicationDbContext context, UserManager<ApplicationUser> userManager , OrderService orderService)
        {
            _context = context;
            _userManager = userManager;
            _orderService = orderService;
        }
        public IActionResult Index()
        {
            return View();
        }


        public async Task<IActionResult> AddToCart(int bookId)
        {
            
            var cart = HttpContext.Session.Get<List<OrderItemVM>>("Cart") ?? new List<OrderItemVM>();

            if (!await _orderService.AddToCart(bookId, cart))
                return NotFound();

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
        public async Task<IActionResult> PlaceOrder()
        {
            var cart = HttpContext.Session.Get<List<OrderItemVM>>("Cart");
            var userId = _userManager.GetUserId(User);

            if (!await _orderService.PlaceOrder(userId, cart))
                return RedirectToAction("Cart");

            HttpContext.Session.Remove("Cart");

            return RedirectToAction("Index", "Book");
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
