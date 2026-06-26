namespace BookStore.ViewModels
{
    public class BookDetailsVM
    {
        public int BookId { get; set; }

        public string Title { get; set; } = string.Empty;

        public string? Description { get; set; }

        public decimal Price { get; set; }

        public int StockQuantity { get; set; }

        public string? ImageUrl { get; set; }

        public string CategoryName { get; set; } = string.Empty;

        public string PublisherName { get; set; } = string.Empty;

        public List<string> Authors { get; set; } = [];
    }
}
