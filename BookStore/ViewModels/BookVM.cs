using BookStore.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace BookStore.ViewModels
{
    public class BookVM
    {
        public int BookId { get; set; }

        [Required]
        [StringLength(200, MinimumLength = 2, ErrorMessage = "Title must be between 2 and 200 characters.")]
        public string Title { get; set; } = string.Empty;

        [StringLength(20, ErrorMessage = "ISBN cannot exceed 20 characters.")]
        public string? ISBN { get; set; }

        [Range(typeof(decimal), "0.01", "999999999.99", ErrorMessage = "Price must be greater than 0.")]
        public decimal Price { get; set; }

        [StringLength(255, ErrorMessage = "Image URL cannot exceed 255 characters.")]
        public string? ImageUrl { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Category is required.")]
        public int CategoryId { get; set; }
        public List<Category>? Categories { get; set; }
        public List<int> SelectedAuthorIds { get; set; } = new();
        public List<SelectListItem> Authors { get; set; } = new();
        public IFormFile? ImageFile { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "Stock quantity cannot be negative.")]
        public int StockQuantity { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Publisher is required.")]
        public int PublisherId { get; set; }

        public string Description { get; set; } = string.Empty;
        public List<Publisher>? Publishers { get; set; }
    }
}
