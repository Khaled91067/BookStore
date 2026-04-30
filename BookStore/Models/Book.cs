using System.ComponentModel.DataAnnotations;

namespace BookStore.Models
{
    public class Book
    {
        public int BookId { get; set; }

        [Required]
        public string Title { get; set; } = string.Empty;

        public string? ISBN { get; set; }

        public decimal Price { get; set; }
        public string? ImageUrl { get; set; } 

        public int CategoryId { get; set; }
        public Category? Category { get; set; }
        public int StockQuantity { get; set; }

        public int PublisherId { get; set; }
        
        public Publisher? Publisher { get; set; }

        public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
        public ICollection<BookAuthor> BookAuthors { get; set; } = new List<BookAuthor>();
    }
}
