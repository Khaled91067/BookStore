using BookStore.Models;
using System.ComponentModel.DataAnnotations;

namespace BookStore.ViewModels
{
    public class BookVM
    {
        public int BookId { get; set; }

        [Required]
        public string Title { get; set; } = string.Empty;

        public string? ISBN { get; set; }
        public decimal Price { get; set; }
        public string? ImageUrl { get; set; }
        public int CategoryId { get; set; }
        public List<Category>? Categories { get; set; }

        public IFormFile? ImageFile { get; set; }
        public int StockQuantity { get; set; }
        public int PublisherId { get; set; }
        public List<Publisher>? Publishers { get; set; }
    }
}
