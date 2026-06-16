using System.ComponentModel.DataAnnotations;

namespace BookStore.Models
{
    public class BookAuthor
    {
        [Range(1, int.MaxValue, ErrorMessage = "Book is required.")]
        public int BookId { get; set; }
        public Book? Book { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Author is required.")]
        public int AuthorId { get; set; }
        public Author? Author { get; set; }
    }
}
