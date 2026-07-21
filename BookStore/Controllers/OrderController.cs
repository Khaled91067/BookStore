using BookStore.Data;
using BookStore.Models;
using BookStore.Services.Implementaion;
using BookStore.Services.Interfaces;
using BookStore.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace BookStore.Controllers
{
    public class OrderController : Controller
    {
        private readonly IPaymobService _paymobService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly OrderService _orderService;
        private readonly ILogger<OrderController> _logger;

        public OrderController(
            UserManager<ApplicationUser> userManager,
            OrderService orderService,
            IPaymobService paymobService,
            ILogger<OrderController> logger)
        {
            _userManager = userManager;
            _orderService = orderService;
            _paymobService = paymobService;
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }

        [Authorize]
        public async Task<IActionResult> AddToCart(int bookId, int quantity = 1)
        {
            var cart = HttpContext.Session.Get<List<OrderItemVM>>("Cart") ?? new List<OrderItemVM>();

            if (!await _orderService.AddToCart(bookId, cart, quantity))
            {
                _logger.LogWarning("AddToCart failed for book {BookId} (user {UserId})", bookId, _userManager.GetUserId(User));
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return Json(new { success = false, message = "Could not add item to cart. Out of stock or invalid book." });
                }
                TempData["Error"] = "Could not add item to cart. Out of stock or invalid book.";
                return RedirectToAction("Index", "UserBook");
            }

            HttpContext.Session.Set("Cart", cart);

            var totalCount = cart.Sum(x => x.Quantity);
            TempData["Success"] = "Book added to cart successfully.";

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return Json(new { success = true, message = "Book added to cart successfully.", cartCount = totalCount });
            }

            var referer = Request.Headers["Referer"].ToString();
            if (!string.IsNullOrEmpty(referer))
            {
                return Redirect(referer);
            }

            return RedirectToAction("Index", "UserBook");
        }

        public IActionResult Cart()
        {
            var cart = HttpContext.Session.Get<List<OrderItemVM>>("Cart") ?? new List<OrderItemVM>();
            return View(cart);
        }

        [Authorize]
        public IActionResult RemoveFromCart(int bookId)
        {
            var cart = HttpContext.Session.Get<List<OrderItemVM>>("Cart") ?? new List<OrderItemVM>();

            if (!_orderService.RemoveFromCart(bookId, cart))
            {
                _logger.LogWarning("RemoveFromCart failed for book {BookId} (user {UserId})", bookId, _userManager.GetUserId(User));
                TempData["Error"] = "Could not remove item from cart.";
                return RedirectToAction("Cart");
            }

            HttpContext.Session.Set("Cart", cart);
            TempData["Success"] = "Item removed from cart.";
            return RedirectToAction("Cart");
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> PlaceOrder()
        {
            var cart = HttpContext.Session.Get<List<OrderItemVM>>("Cart");
            if (cart == null || !cart.Any())
            {
                _logger.LogWarning("PlaceOrder: empty cart for user {UserId}", _userManager.GetUserId(User));
                TempData["Error"] = "Your cart is empty. Please add items to your cart before placing an order.";
                return RedirectToAction("Cart");
            }

            var userId = _userManager.GetUserId(User);
            int? orderId = await _orderService.PlaceOrder(userId, cart);

            if (!orderId.HasValue)
            {
                _logger.LogWarning("PlaceOrder failed for user {UserId}", userId);
                TempData["Error"] = "Could not place the order. Please try again.";
                return RedirectToAction("Cart");
            }

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
            if (await _orderService.IsOrderPaidAsync(id))
            {
                _logger.LogInformation("Checkout: order {OrderId} already paid — redirecting user {UserId}", id, _userManager.GetUserId(User));
                TempData["Success"] = "This order has already been paid successfully.";
                return RedirectToAction("ViewOrders");
            }

            var user = await _userManager.GetUserAsync(User);
            var vm = await _orderService.PrepareCheckOutVMAsync(id, user);

            if (vm == null)
            {
                _logger.LogWarning("Checkout GET: order {OrderId} not found for user {UserId}", id, user?.Id);
                return NotFound();
            }

            return View(vm);
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Checkout(CheckOutVM vm, int orderId)
        {
            if (await _orderService.IsOrderPaidAsync(orderId))
            {
                _logger.LogInformation("Checkout POST: order {OrderId} already paid — redirecting user {UserId}", orderId, _userManager.GetUserId(User));
                TempData["Success"] = "This order has already been paid successfully.";
                return RedirectToAction("ViewOrders");
            }

            var user = await _userManager.GetUserAsync(User);

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Checkout POST: invalid model for order {OrderId}, user {UserId}", orderId, user?.Id);
                return Content("Model Invalid");
            }

            if (user != null)
            {
                await _orderService.UpdateUserAddressAsync(user, vm);
            }

            await _orderService.UpdatePaymentMethodAsync(orderId, vm.PaymentMethod);

            if (vm.PaymentMethod == PaymentMethod.Cash)
            {
                return RedirectToAction("PaymentResult", "Payment", new { orderId = orderId });
            }
            else
            {
                var checkoutUrl = await _paymobService.CreateIntentionAsync(orderId);
                return Redirect(checkoutUrl);
            }
        }
    }
}
