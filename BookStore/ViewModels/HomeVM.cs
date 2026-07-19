using BookStore.Models;
using System.Collections.Generic;

namespace BookStore.ViewModels
{
    public class HomeVM
    {
        public List<Book> FeaturedBooks { get; set; } = new();
        public List<Category> Categories { get; set; } = new();
        public List<Author> FeaturedAuthors { get; set; } = new();
    }
}
