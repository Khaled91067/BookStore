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
        public decimal TotalAmount {  get; set; }

        public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();

        public ICollection<Payment> Payments { get; set; } = new List<Payment>();
    
    }

}
