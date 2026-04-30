using BookStore.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Reflection.Emit;

namespace BookStore.Data
{
    public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : IdentityDbContext(options)
    {
        public DbSet<Book> Books { get; set; }
        public DbSet<Author> Authors { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Publisher> Publishers { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
        public DbSet<BookAuthor> BookAuthors { get; set; }


        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            // Configure many-to-many relationship between Book and Author
            builder.Entity<BookAuthor>()
                .HasKey(ba => new { ba.BookId, ba.AuthorId });

            builder.Entity<BookAuthor>()
                .HasOne(ba => ba.Book)
                .WithMany(b => b.BookAuthors)
                .HasForeignKey(ba => ba.BookId);

            builder.Entity<BookAuthor>()
                .HasOne(ba => ba.Author)
                .WithMany(a => a.BookAuthors)
                .HasForeignKey(ba => ba.AuthorId);

            builder.Entity<Category>().HasData(
        new Category { CategoryId = 1, Name = "Programming" },
        new Category { CategoryId = 2, Name = "Databases" },
        new Category { CategoryId = 3, Name = "Networking" }
    );

         builder.Entity<Publisher>().HasData(
                new Publisher { PublisherId = 1, Name = "Tech Books" },
                new Publisher { PublisherId = 2, Name = "Code Press" }
            );

            builder.Entity<Author>().HasData(
                new Author { AuthorId = 1, Name = "Ahmed Ali" },
                new Author { AuthorId = 2, Name = "Mohamed Hassan" },
                new Author { AuthorId = 3, Name = "Sara Ibrahim" }
            );

            builder.Entity<Book>().HasData(
                new Book
                {
                    BookId = 1,
                    Title = "C# Fundamentals",
                    Price = 150,
                    CategoryId = 1,
                    PublisherId = 1,
                    ImageUrl = "/images/csharp.jpg"
                },
                new Book
                {
                    BookId = 2,
                    Title = "SQL Mastery",
                    Price = 180,
                    CategoryId = 2,
                    PublisherId = 2,
                    ImageUrl = "/images/sql.jpg"
                },
                new Book
                {
                    BookId = 3,
                    Title = "Network Basics",
                    Price = 130,
                    CategoryId = 3,
                    PublisherId = 1,
                    ImageUrl = "/images/network.jpg"
                }
            );

            builder.Entity<BookAuthor>().HasData(
                new BookAuthor { BookId = 1, AuthorId = 1 },
                new BookAuthor { BookId = 1, AuthorId = 2 },
                new BookAuthor { BookId = 2, AuthorId = 2 },
                new BookAuthor { BookId = 3, AuthorId = 3 }
           
    );
            builder.Entity<Book>()
    .Property(b => b.Price)
    .HasPrecision(18, 2);

            builder.Entity<OrderItem>()
   .Property(b => b.Price)
   .HasPrecision(18, 2);


        }


    }
}
