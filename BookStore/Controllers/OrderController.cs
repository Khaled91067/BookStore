using BookStore.Data;
using BookStore.Models;
using BookStore.Services.Implementaion;
using BookStore.Services.Interfaces;
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
        private readonly IPaymobService _paymobService;
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly OrderService _orderService;

        public OrderController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, OrderService orderService, IPaymobService paymobService)
        {
            _context = context;
            _userManager = userManager;
            _orderService = orderService;
            _paymobService = paymobService;
        }
        public IActionResult Index()
        {
            return View();
        }

        [Authorize]
        public async Task<IActionResult> AddToCart(int bookId)
        {

            var cart = HttpContext.Session.Get<List<OrderItemVM>>("Cart") ?? new List<OrderItemVM>();

            if (!await _orderService.AddToCart(bookId, cart))
                return NotFound();

            HttpContext.Session.Set("Cart", cart);

            TempData["Success"] = "Book added to cart successfully.";

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
        public async Task<IActionResult> ViewOrders(string? searchTerm)
        {
           var user = await _userManager.GetUserAsync(User);

           if (user == null)
           return Unauthorized();

           var orders = await _orderService.GetUserOrdersAsync(user.Id, searchTerm);

           return View(orders);

        }


        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Checkout(int id)
        {
            var user = await _userManager.GetUserAsync(User);

            var vm = await _orderService.PrepareCheckOutVMAsync(id, user);

            if (vm == null)
                return NotFound();

            return View(vm);

        }


        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Checkout(CheckOutVM vm,int orderId)
        {
            var user = await _userManager.GetUserAsync(User);

            if (!ModelState.IsValid)
                return Content("Model Invalid");

            if (user != null)
            {
                user.FirstName = vm.FirstName;
                user.LastName = vm.LastName;
                user.Address = vm.Address;
                user.PhoneNumber = vm.PhoneNumber;

                await _userManager.UpdateAsync(user);
            }

            var checkoutUrl = await _paymobService.CreateIntentionAsync(orderId);

            return Redirect(checkoutUrl);

        }

    }
}
