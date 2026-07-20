using BookStore.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BookStore.Data.Configurations;

public class PaymentConfiguration : IEntityTypeConfiguration<Payment>
{
    public void Configure(EntityTypeBuilder<Payment> builder)
    {
        builder.Property(p => p.Amount)
               .HasPrecision(18, 2);

        // Store enums as string names in DB
        builder.Property(p => p.PaymentMethod)
               .HasConversion<string>()
               .HasMaxLength(50);

        builder.Property(p => p.PaymentStatus)
               .HasConversion<string>()
               .HasMaxLength(50);

        builder.Property(p => p.ProviderReference)
               .HasMaxLength(255);

        builder.Property(p => p.TransactionId)
               .HasMaxLength(255);

        // Cascade delete payments if the order is deleted
        builder.HasOne(p => p.Order)
               .WithMany(o => o.Payments)
               .HasForeignKey(p => p.OrderId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}
