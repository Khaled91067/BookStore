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

        [NotMapped]
        public IList<string> RoleNames { get; set; } = null!;
    }
}
