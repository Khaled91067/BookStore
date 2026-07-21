using BookStore.Data;
using BookStore.Models;
using BookStore.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace BookStore.Services.Implementaion
{
    public class DashboardService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<DashboardService> _logger;

        public DashboardService(ApplicationDbContext context, ILogger<DashboardService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<AdminDashboardVM> GetDashboardDataAsync()
        {
            var viewModel = new AdminDashboardVM();

            // 1. Total Revenue (sum of successful payments)
            viewModel.TotalRevenue = await _context.Payments
                .Where(p => p.PaymentStatus == PaymentStatus.Succeeded)
                .SumAsync(p => p.Amount);

            // 2. Total Orders
            viewModel.TotalOrders = await _context.Orders.CountAsync();

            // 3. Total Books
            viewModel.TotalBooks = await _context.Books.CountAsync();

            // 4. Total Customers
            viewModel.TotalCustomers = await _context.Users.CountAsync();

            // 5. Low Stock Books (stock <= 5)
            viewModel.LowStockBooks = await _context.Books
                .Where(b => b.StockQuantity <= 5)
                .OrderBy(b => b.StockQuantity)
                .Select(b => new BookLowStockVM
                {
                    BookId = b.BookId,
                    Title = b.Title,
                    StockQuantity = b.StockQuantity,
                    ImageUrl = b.ImageUrl
                })
                .Take(5)
                .ToListAsync();

            // 6. Recent Orders (last 5)
            viewModel.RecentOrders = await _context.Orders
                .OrderByDescending(o => o.OrderDate)
                .Take(5)
                .Select(o => new RecentOrderVM
                {
                    OrderId = o.OrderId,
                    CustomerName = o.User != null
                        ? (o.User.FirstName + " " + o.User.LastName).Trim()
                        : "Guest",
                    TotalAmount = o.TotalAmount,
                    OrderDate = o.OrderDate,
                    PaymentStatus = _context.Payments
                        .Where(p => p.OrderId == o.OrderId)
                        .OrderByDescending(p => p.CreatedAt)
                        .Select(p => p.PaymentStatus.ToString())
                        .FirstOrDefault() ?? "Pending",
                    PaymentMethod = _context.Payments
                        .Where(p => p.OrderId == o.OrderId)
                        .OrderByDescending(p => p.CreatedAt)
                        .Select(p => p.PaymentMethod.ToString())
                        .FirstOrDefault() ?? "Cash"
                })
                .ToListAsync();

            // Fallback: resolve missing customer names via a second query
            foreach (var order in viewModel.RecentOrders)
            {
                if (string.IsNullOrWhiteSpace(order.CustomerName) || order.CustomerName == "Guest")
                {
                    var fullOrder = await _context.Orders
                        .Include(o => o.User)
                        .FirstOrDefaultAsync(o => o.OrderId == order.OrderId);

                    if (fullOrder?.User != null && !string.IsNullOrWhiteSpace(fullOrder.User.UserName))
                        order.CustomerName = fullOrder.User.UserName;
                }
            }

            // 7. Top Selling Books
            viewModel.TopSellingBooks = await _context.OrderItems
                .GroupBy(oi => new { oi.BookId, oi.Book.Title, oi.Book.ImageUrl })
                .Select(g => new TopSellingBookVM
                {
                    BookId = g.Key.BookId,
                    Title = g.Key.Title,
                    ImageUrl = g.Key.ImageUrl,
                    TotalSold = g.Sum(oi => oi.Quantity),
                    TotalRevenue = g.Sum(oi => oi.Quantity * oi.Price)
                })
                .OrderByDescending(x => x.TotalSold)
                .Take(5)
                .ToListAsync();

            // 8. Sales History (past 7 days)
            var startDate = DateTime.UtcNow.Date.AddDays(-6);
            var ordersInPeriod = await _context.Orders
                .Where(o => o.OrderDate >= startDate)
                .Select(o => new { o.OrderDate, o.TotalAmount })
                .ToListAsync();

            for (int i = 0; i < 7; i++)
            {
                var date = startDate.AddDays(i);
                var dayOrders = ordersInPeriod.Where(o => o.OrderDate.Date == date.Date).ToList();
                viewModel.SalesHistory.Add(new DailySalesVM
                {
                    DateString = date.ToString("MMM dd"),
                    Revenue = dayOrders.Sum(o => o.TotalAmount),
                    OrderCount = dayOrders.Count
                });
            }

            // 9. Category Distribution
            viewModel.CategoryDistribution = await _context.Categories
                .Select(c => new CategoryShareVM
                {
                    CategoryName = c.Name,
                    BookCount = c.Books != null ? c.Books.Count() : 0
                })
                .Where(x => x.BookCount > 0)
                .ToListAsync();

            _logger.LogDebug("Dashboard data loaded successfully");
            return viewModel;
        }
    }
}
