using BookStore.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BookStore.Data.Configurations;

public class OrderItemConfiguration : IEntityTypeConfiguration<OrderItem>
{
    public void Configure(EntityTypeBuilder<OrderItem> builder)
    {
        builder.Property(b => b.Price)
               .HasPrecision(18, 2);

        // Restrict deleting a book if it is referenced in any order items
        builder.HasOne(oi => oi.Book)
               .WithMany(b => b.OrderItems)
               .HasForeignKey(oi => oi.BookId)
               .OnDelete(DeleteBehavior.Restrict);

        // Cascade delete items if the order is deleted
        builder.HasOne(oi => oi.Order)
               .WithMany(o => o.OrderItems)
               .HasForeignKey(oi => oi.OrderId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}