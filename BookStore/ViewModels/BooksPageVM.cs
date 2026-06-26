namespace BookStore.ViewModels
{
    public class BooksPageVM
    {
        public List<BookVM> Books { get; set; } = [];

        public int CurrentPage { get; set; }

        public int TotalPages { get; set; }

        public string? SearchTerm { get; set; }

    }
}
