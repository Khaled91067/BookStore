using BookStore.Data;
using BookStore.Models;
using Microsoft.AspNetCore.Identity;

namespace BookStore.Areas.Admin.Models
{
    public class UserVM
    {
        public IEnumerable<ApplicationUser> Users { get; set; } = null!;
        
        public IEnumerable<IdentityRole> Roles { get; set; } = null!;
    }

}
