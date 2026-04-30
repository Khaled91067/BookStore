using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace BookStore.Models
{

    public class Author
    {
        public int AuthorId { get; set; }

        [Required]
        public string Name { get; set; } = string.Empty;

        public ICollection<BookAuthor> BookAuthors { get; set; } = new List<BookAuthor>();
    }

    

    

    

    
}
