using BookStore.Models;

namespace BookStore.ViewModels
{
    public class OrderVM
    {
        public int OrderId { get; set; }    


        public DateTime OrderDate { get; set; }
        public List<OrderItemVM> OrderItems { get; set; } = new List<OrderItemVM>();

        public string CustomerName { get; set; } = string.Empty;

        public IEnumerable<BookVM> Books { get; set; } = Enumerable.Empty<BookVM>();

        public string TransactionId { get; set; } = string.Empty;
        public PaymentStatus PaymentStatus { get; set; }
        public PaymentMethod PaymentMethod { get; set; }

        public decimal TotalAmount { get; set; }
    }
}
