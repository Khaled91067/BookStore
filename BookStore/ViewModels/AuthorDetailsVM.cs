using System.Collections.Generic;

namespace BookStore.ViewModels
{
    public class AuthorDetailsVM
    {
        public int AuthorId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? ImageUrl { get; set; }
        public List<BookVM> Books { get; set; } = new();
    }
}
