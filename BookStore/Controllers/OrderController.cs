using BookStore.Data;
using BookStore.Models;
using BookStore.Services.Implementaion;
using BookStore.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using static NuGet.Packaging.PackagingConstants;

namespace BookStore.Controllers
{
    public class OrderController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly OrderService _orderService;

        public OrderController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, OrderService orderService)
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
            int? orderId = await _orderService.PlaceOrder(userId, cart);
            if (!orderId.HasValue)
                return RedirectToAction("Cart");

            HttpContext.Session.Remove("Cart");

            return RedirectToAction("Checkout", new { id = orderId });
        }


        [HttpGet]
        [Authorize]
        public async Task<IActionResult> ViewOrders()
        {
            var userId = _userManager.GetUserId(User);

            var userOrders = await _context.Orders.Where(o => o.UserId == userId)
                                                  .Include(o => o.OrderItems)
                                                  .ThenInclude(i => i.Book)
                                                  .ToListAsync();


            return View("ViewOrders", userOrders);
        }


        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Checkout(int id)
        {
            var order = await _context.Orders.Where(o => o.OrderId == id)
                                             .Include(o => o.OrderItems)
                                             .ThenInclude(i => i.Book)
                                             .FirstOrDefaultAsync();
            if (order == null)
                return NotFound();


           

           
            var user = await _userManager.GetUserAsync(User);

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


            /*var cultures = CultureInfo.GetCultures(CultureTypes.SpecificCultures);
            vm.Countries = cultures
               .Select(c => new RegionInfo(c.Name))
               .DistinctBy(r => r.EnglishName)
               .Select(r => new SelectListItem
               {
                   Value = r.EnglishName,
                   Text = r.EnglishName
               })
               .OrderBy(c => c.Text)
               .ToList();*/


            return View(vm);

        }


        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Checkout(CheckOutVM vm)
        {
            


            var user = await _userManager.GetUserAsync(User);

            /*if (!ModelState.IsValid)
                return Content("Model Invalid");*/

            if (user != null)
            {
                user.FirstName = vm.FirstName;
                user.LastName = vm.LastName;
                user.Address = vm.Address;
                user.PhoneNumber = vm.PhoneNumber;

                await _userManager.UpdateAsync(user);
            }

            return RedirectToAction("ViewOrders");



        }

    }
}
