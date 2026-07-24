using BookStore.ViewModels;

namespace BookStore.Models
{
    public enum PaymentStatus
    { 
        Pending,
        Succeeded,
        Failed
    }

    // Visa and Stripe are declared for future integration; only Cash and Paymob are
    // actively handled in the current checkout flow.
    public enum PaymentMethod
    {
        Cash,
        Visa,
        Paymob,
        Stripe
    }
    public class Payment
    {
        public int PaymentId { get; set; }
        public PaymentMethod PaymentMethod { get; set; }
        public PaymentStatus PaymentStatus { get; set; }
        // Paymob's internal order ID, stored after intention creation and used to
        // correlate the incoming webhook with our Payment record.
        public string? ProviderReference { get; set; }

        // The individual transaction ID returned by Paymob on success.
        // Distinct from ProviderReference: one order can have multiple transaction attempts.
        public string? TransactionId { get; set; }

        public decimal Amount { get; set; }
        
        public int OrderId { get; set; }
        public Order Order { get; set; } = null!;

        // NOTE: Uses local time — consider DateTime.UtcNow for consistency with Order.OrderDate.
        public DateTime CreatedAt { get; set; } = DateTime.Now;

    }
}
