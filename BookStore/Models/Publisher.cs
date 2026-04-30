using System.ComponentModel.DataAnnotations;

namespace BookStore.Models
{
    public class Publisher
    {
        public int PublisherId { get; set; }

        [Required]
        public string Name { get; set; } = string.Empty;

        public ICollection<Book> Books { get; set; } = new List<Book>();
    }
}
