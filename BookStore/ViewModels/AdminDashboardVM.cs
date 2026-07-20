using System;
using System.Collections.Generic;

namespace BookStore.ViewModels
{
    public class AdminDashboardVM
    {
        public decimal TotalRevenue { get; set; }
        public int TotalOrders { get; set; }
        public int TotalCustomers { get; set; }
        public int TotalBooks { get; set; }
        
        public List<BookLowStockVM> LowStockBooks { get; set; } = new();
        public List<RecentOrderVM> RecentOrders { get; set; } = new();
        public List<TopSellingBookVM> TopSellingBooks { get; set; } = new();
        public List<DailySalesVM> SalesHistory { get; set; } = new();
        public List<CategoryShareVM> CategoryDistribution { get; set; } = new();
    }

    public class BookLowStockVM
    {
        public int BookId { get; set; }
        public string Title { get; set; } = string.Empty;
        public int StockQuantity { get; set; }
        public string? ImageUrl { get; set; }
    }

    public class RecentOrderVM
    {
        public int OrderId { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public decimal TotalAmount { get; set; }
        public DateTime OrderDate { get; set; }
        public string PaymentStatus { get; set; } = string.Empty;
        public string PaymentMethod { get; set; } = string.Empty;
    }

    public class TopSellingBookVM
    {
        public int BookId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? ImageUrl { get; set; }
        public int TotalSold { get; set; }
        public decimal TotalRevenue { get; set; }
    }

    public class DailySalesVM
    {
        public string DateString { get; set; } = string.Empty;
        public decimal Revenue { get; set; }
        public int OrderCount { get; set; }
    }

    public class CategoryShareVM
    {
        public string CategoryName { get; set; } = string.Empty;
        public int BookCount { get; set; }
    }
}
