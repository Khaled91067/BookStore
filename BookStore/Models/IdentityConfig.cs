using Microsoft.AspNetCore.Identity;

namespace BookStore.Models
{
    public class IdentityConfig
    {
        public static async Task CreateAdminUserAsync(IServiceProvider provider)
        {
            var roleManger = provider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = provider.GetRequiredService<UserManager<ApplicationUser>>();

            string username = "admin@bookstore.com";
            string password = "#Bengobega1";
            string roleName = "Admin";

            if(await roleManger.FindByNameAsync(roleName)==null)
            {
                await roleManger.CreateAsync(new IdentityRole(roleName));
            }

            if(await userManager.FindByNameAsync(username)==null)
            {
                ApplicationUser user = new ApplicationUser { UserName = username, Email = "admin@bookstore.com", EmailConfirmed = true };
                var result =await userManager.CreateAsync(user,password);
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(user, roleName);
                }
            }

        }
    }
}
