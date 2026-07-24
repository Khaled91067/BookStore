using BookStore.Data;
using System.ComponentModel.DataAnnotations;

namespace BookStore.Models
{
    public class Order
    {
        public int OrderId { get; set; }

        [Required]
        public DateTime OrderDate { get; set; }

        [Required]
        [StringLength(450, ErrorMessage = "User is required.")]
        public string UserId { get; set; } = string.Empty;
        public ApplicationUser? User { get; set; }
        // Denormalized for dashboard/reporting queries; must stay in sync with
        // the sum of OrderItems.Price * Quantity when items change.
        public decimal TotalAmount {  get; set; }

        public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();

        // A one-to-many collection supports retries: a user can attempt payment
        // multiple times on the same order. The most recent record (highest PaymentId)
        // is treated as the authoritative status.
        public ICollection<Payment> Payments { get; set; } = new List<Payment>();
    
    }

}
