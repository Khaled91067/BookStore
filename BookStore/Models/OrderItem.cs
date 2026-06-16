using System.ComponentModel.DataAnnotations;

namespace BookStore.Models
{
    public class OrderItem
    {
        public int OrderItemId { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Order is required.")]
        public int OrderId { get; set; }
        public Order? Order { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Book is required.")]
        public int BookId { get; set; }
        public Book? Book { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be at least 1.")]
        public int Quantity { get; set; }

        [Range(typeof(decimal), "0.01", "999999999.99", ErrorMessage = "Price must be greater than 0.")]
        public decimal Price { get; set; }
    }
}
