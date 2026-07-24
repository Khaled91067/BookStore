using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using BookStore.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace BookStore.Data
{
    public static class DbInitializer
    {
        public static async Task SeedDataAsync(ApplicationDbContext context, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager, ILogger logger)
        {
            logger.LogInformation("Applying pending database migrations");
            await context.Database.MigrateAsync();
            logger.LogInformation("Database migration complete");

            var jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            string seedDataPath = Path.Combine(Directory.GetCurrentDirectory(), "Data", "SeedData");

            // 1. Seed Categories
            string categoriesPath = Path.Combine(seedDataPath, "categories.json");
            if (File.Exists(categoriesPath))
            {
                var categoriesJson = await File.ReadAllTextAsync(categoriesPath);
                var categories = JsonSerializer.Deserialize<List<Category>>(categoriesJson, jsonOptions);
                if (categories != null)
                {
                    if (!await context.Categories.AnyAsync())
                    {
                        await context.Categories.AddRangeAsync(categories);
                        await context.SaveChangesAsync();
                        logger.LogInformation("Seeded {Count} categories", categories.Count);
                    }
                    else
                    {
                        // On subsequent runs, only image URLs are updated so existing category
                        // relationships and references stay intact.
                        var existingCategories = await context.Categories.ToListAsync();
                        bool changed = false;
                        foreach (var cat in categories)
                        {
                            var existing = existingCategories.FirstOrDefault(c => c.Name == cat.Name);
                            if (existing != null && existing.ImageUrl != cat.ImageUrl && !string.IsNullOrEmpty(cat.ImageUrl))
                            {
                                existing.ImageUrl = cat.ImageUrl;
                                changed = true;
                            }
                        }
                        if (changed)
                        {
                            await context.SaveChangesAsync();
                            logger.LogInformation("Updated category images from seed data");
                        }
                        else
                        {
                            logger.LogDebug("Categories already seeded — skipping");
                        }
                    }
                }
            }
            else
            {
                logger.LogWarning("Seed file not found: {FilePath}", categoriesPath);
            }


            // 2. Seed Publishers
            if (!await context.Publishers.AnyAsync())
            {
                string publishersPath = Path.Combine(seedDataPath, "publishers.json");
                if (File.Exists(publishersPath))
                {
                    var publishersJson = await File.ReadAllTextAsync(publishersPath);
                    var publishers = JsonSerializer.Deserialize<List<Publisher>>(publishersJson, jsonOptions);
                    if (publishers != null)
                    {
                        await context.Publishers.AddRangeAsync(publishers);
                        await context.SaveChangesAsync();
                        logger.LogInformation("Seeded {Count} publishers", publishers.Count);
                    }
                }
                else
                {
                    logger.LogWarning("Seed file not found: {FilePath}", Path.Combine(seedDataPath, "publishers.json"));
                }
            }
            else
            {
                logger.LogDebug("Publishers already seeded — skipping");
            }

            // 3. Seed Authors
            string authorsPath = Path.Combine(seedDataPath, "authors.json");
            if (File.Exists(authorsPath))
            {
                var authorsJson = await File.ReadAllTextAsync(authorsPath);
                var authors = JsonSerializer.Deserialize<List<Author>>(authorsJson, jsonOptions);
                if (authors != null)
                {
                    if (!await context.Authors.AnyAsync())
                    {
                        await context.Authors.AddRangeAsync(authors);
                        await context.SaveChangesAsync();
                        logger.LogInformation("Seeded {Count} authors", authors.Count);
                    }
                    else
                    {
                        var existingAuthors = await context.Authors.ToListAsync();
                        bool changed = false;
                        foreach (var author in authors)
                        {
                            var existing = existingAuthors.FirstOrDefault(a => a.Name == author.Name);
                            if (existing != null && existing.ImageUrl != author.ImageUrl && !string.IsNullOrEmpty(author.ImageUrl))
                            {
                                existing.ImageUrl = author.ImageUrl;
                                changed = true;
                            }
                        }
                        if (changed)
                        {
                            await context.SaveChangesAsync();
                            logger.LogInformation("Updated author images from seed data");
                        }
                        else
                        {
                            logger.LogDebug("Authors already seeded — skipping");
                        }
                    }
                }
            }

            // 4. Seed Books and BookAuthors
            if (!await context.Books.AnyAsync())
            {
                logger.LogInformation("Seeding books and book-author relationships");
                string booksPath = Path.Combine(seedDataPath, "books.json");
                if (File.Exists(booksPath))
                {
                    var booksJson = await File.ReadAllTextAsync(booksPath);
                    var bookDtos = JsonSerializer.Deserialize<List<BookSeedDto>>(booksJson, jsonOptions);
                    if (bookDtos != null)
                    {
                        foreach (var dto in bookDtos)
                        {
                            var category = await context.Categories.FirstOrDefaultAsync(c => c.Name == dto.CategoryName);
                            var publisher = await context.Publishers.FirstOrDefaultAsync(p => p.Name == dto.PublisherName);

                            if (category != null && publisher != null)
                            {
                                var book = new Book
                                {
                                    Title = dto.Title,
                                    Price = dto.Price,
                                    ImageUrl = dto.ImageUrl,
                                    CategoryId = category.CategoryId,
                                    PublisherId = publisher.PublisherId,
                                    StockQuantity = dto.StockQuantity,
                                    Description = dto.Description
                                };

                                await context.Books.AddAsync(book);
                                // SaveChangesAsync per book to retrieve the generated BookId
                                // before creating BookAuthor join records.
                                await context.SaveChangesAsync();

                                // Associate Authors
                                if (dto.AuthorNames != null)
                                {
                                    foreach (var authorName in dto.AuthorNames)
                                      {
                                        var author = await context.Authors.FirstOrDefaultAsync(a => a.Name == authorName);
                                        if (author != null)
                                        {
                                            await context.BookAuthors.AddAsync(new BookAuthor
                                            {
                                                BookId = book.BookId,
                                                AuthorId = author.AuthorId
                                            });
                                        }
                                    }
                                    await context.SaveChangesAsync();
                                }
                            }
                        }
                    }
                }
            }

            // 5. Seed Roles & Users
            string usersPath = Path.Combine(seedDataPath, "users.json");
            if (File.Exists(usersPath))
            {
                var usersJson = await File.ReadAllTextAsync(usersPath);
                var userDtos = JsonSerializer.Deserialize<List<UserSeedDto>>(usersJson, jsonOptions);
                if (userDtos != null)
                {
                    foreach (var dto in userDtos)
                    {
                        // Ensure Role Exists
                        if (!await roleManager.RoleExistsAsync(dto.Role))
                        {
                            await roleManager.CreateAsync(new IdentityRole(dto.Role));
                            logger.LogInformation("Seeded role '{RoleName}'", dto.Role);
                        }

                        // Ensure User Exists
                        var user = await userManager.FindByNameAsync(dto.UserName);
                        if (user == null)
                        {
                            user = new ApplicationUser
                            {
                                UserName = dto.UserName,
                                Email = dto.Email,
                                // Pre-confirmed to allow immediate login without an email flow;
                                // seed accounts are managed credentials, not self-registered.
                                EmailConfirmed = true,
                                FirstName = dto.FirstName,
                                LastName = dto.LastName,
                                Address = dto.Address,
                                PhoneNumber = dto.PhoneNumber
                            };

                            var result = await userManager.CreateAsync(user, dto.Password);
                            if (result.Succeeded)
                            {
                                await userManager.AddToRoleAsync(user, dto.Role);
                                logger.LogInformation("Seeded user '{UserName}' with role '{Role}'", dto.UserName, dto.Role);
                            }
                            else
                            {
                                logger.LogWarning("Failed to seed user '{UserName}': {Errors}", dto.UserName,
                                    string.Join(", ", result.Errors.Select(e => e.Description)));
                            }
                        }
                    }
                }
            }

            // 6. Seed Orders, OrderItems, and Payments
            if (!await context.Orders.AnyAsync())
            {
                string ordersPath = Path.Combine(seedDataPath, "orders.json");
                if (File.Exists(ordersPath))
                {
                    var ordersJson = await File.ReadAllTextAsync(ordersPath);
                    var orderDtos = JsonSerializer.Deserialize<List<OrderSeedDto>>(ordersJson, jsonOptions);
                    if (orderDtos != null)
                    {
                        foreach (var dto in orderDtos)
                        {
                            var user = await userManager.FindByEmailAsync(dto.CustomerEmail);
                            if (user != null)
                            {
                                var order = new Order
                                {
                                    UserId = user.Id,
                                    // Negative offset produces past dates, making seed orders look
                                    // realistic in the dashboard's 7-day sales history chart.
                                    OrderDate = DateTime.UtcNow.AddDays(dto.OrderDateOffsetDays),
                                    TotalAmount = dto.Payment.Amount
                                };

                                await context.Orders.AddAsync(order);
                                await context.SaveChangesAsync();

                                foreach (var itemDto in dto.OrderItems)
                                {
                                    var book = await context.Books.FirstOrDefaultAsync(b => b.Title == itemDto.BookTitle);
                                    if (book != null)
                                    {
                                        var orderItem = new OrderItem
                                        {
                                            OrderId = order.OrderId,
                                            BookId = book.BookId,
                                            Quantity = itemDto.Quantity,
                                            Price = itemDto.Price
                                        };
                                        await context.OrderItems.AddAsync(orderItem);
                                    }
                                }
                                await context.SaveChangesAsync();

                                // Add Payment
                                if (Enum.TryParse<PaymentMethod>(dto.Payment.PaymentMethod, true, out var method) &&
                                    Enum.TryParse<PaymentStatus>(dto.Payment.PaymentStatus, true, out var status))
                                {
                                    var payment = new Payment
                                    {
                                        OrderId = order.OrderId,
                                        Amount = dto.Payment.Amount,
                                        PaymentMethod = method,
                                        PaymentStatus = status,
                                        CreatedAt = order.OrderDate,
                                        // Synthetic transaction ID for demo data only;
                                        // real transaction IDs come from the payment gateway.
                                        TransactionId = Guid.NewGuid().ToString().Replace("-", "").Substring(0, 12)
                                    };
                                    await context.Payments.AddAsync(payment);
                                    await context.SaveChangesAsync();
                                }
                            }
                        }
                    }
                }
            }
        }

        // DTOs for deserialization
        private class BookSeedDto
        {
            public string Title { get; set; } = string.Empty;
            public decimal Price { get; set; }
            public string? ImageUrl { get; set; }
            public string CategoryName { get; set; } = string.Empty;
            public string PublisherName { get; set; } = string.Empty;
            public int StockQuantity { get; set; }
            public string Description { get; set; } = string.Empty;
            public List<string>? AuthorNames { get; set; }
        }

        private class UserSeedDto
        {
            public string UserName { get; set; } = string.Empty;
            public string Email { get; set; } = string.Empty;
            public string? FirstName { get; set; }
            public string? LastName { get; set; }
            public string? Address { get; set; }
            public string? PhoneNumber { get; set; }
            public string Role { get; set; } = string.Empty;
            public string Password { get; set; } = string.Empty;
        }

        private class OrderSeedDto
        {
            public string CustomerEmail { get; set; } = string.Empty;
            public int OrderDateOffsetDays { get; set; }
            public List<OrderItemSeedDto> OrderItems { get; set; } = new();
            public PaymentSeedDto Payment { get; set; } = new();
        }

        private class OrderItemSeedDto
        {
            public string BookTitle { get; set; } = string.Empty;
            public int Quantity { get; set; }
            public decimal Price { get; set; }
        }

        private class PaymentSeedDto
        {
            public string PaymentMethod { get; set; } = string.Empty;
            public string PaymentStatus { get; set; } = string.Empty;
            public decimal Amount { get; set; }
        }
    }
}
