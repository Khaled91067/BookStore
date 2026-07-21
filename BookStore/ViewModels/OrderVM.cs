using BookStore.Models;

namespace BookStore.ViewModels
{
    public class OrderVM
    {
        public int OrderId { get; set; }    


        public DateTime OrderDate { get; set; }
        public List<OrderItemVM> OrderItems { get; set; }

        public string CustomerName { get; set; }

        public IEnumerable<BookVM> Books { get; set; }

        public String TransactionId { get; set; }
        public PaymentStatus PaymentStatus { get; set; }
        public PaymentMethod PaymentMethod { get; set; }

        public decimal TotalAmount { get; set; }
    }
}
