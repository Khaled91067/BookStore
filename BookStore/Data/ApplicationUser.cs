using BookStore.Models;
using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations.Schema;

namespace BookStore.Data
{
    public class ApplicationUser : IdentityUser
    {
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Address { get; set; }

        public ICollection<Order> Orders { get; set; } = new List<Order>();

        // Populated on-demand (e.g., by UserService) after loading from Identity.
        // Not persisted — accessing this without explicit population will throw NullReferenceException.
        [NotMapped]
        public IList<string> RoleNames { get; set; } = null!;
    }
}
