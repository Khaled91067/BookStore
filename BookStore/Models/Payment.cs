using BookStore.ViewModels;

namespace BookStore.Models
{
    public enum PaymentStatus
    { 
        Pending,
        Succeeded,
        Failed
    }
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

        public string? TransactionId { get; set; }

        public decimal Amount { get; set; }
        
        public int OrderId { get; set; }
        public Order Order { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

    }
}
