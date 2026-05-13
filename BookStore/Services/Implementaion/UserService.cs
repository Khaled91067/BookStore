using BookStore.Areas.Admin.Models;
using BookStore.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace BookStore.Services.Implementaion
{

    public class UserService
    {
        private UserManager<ApplicationUser> _userManager;
        private RoleManager<IdentityRole> _roleManager;

        public UserService(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }
        public async Task<bool> CreateRoleAsync(string roleName)
        {
            if (await _roleManager.RoleExistsAsync(roleName))
                return false;

            IdentityRole role = new IdentityRole { Name = roleName };

            await _roleManager.CreateAsync(role);
            return true;
        }
        public async Task<UserVM> GetUsersAsync()
        {
            List<ApplicationUser> users = new List<ApplicationUser>();

            foreach (ApplicationUser user in _userManager.Users)
            {
                user.RoleNames = await _userManager.GetRolesAsync(user);
                users.Add(user);
            }

            UserVM model = new UserVM
            {
                Users = users,
                Roles = _roleManager.Roles.ToList()
            };



            return model;
        }
        public async Task<bool> AddUserToRoleAsync(string id, string roleName)
        {
            IdentityRole role = await _roleManager.FindByNameAsync(roleName);
            if (role == null)
                return false;

            ApplicationUser user = await _userManager.FindByIdAsync(id);
            if (user == null)
                return false;

            await _userManager.AddToRoleAsync(user, roleName);
            return true;
        }

        public async Task RemoveUserFromRoleAsync(string userId, string roleName)
        {
            ApplicationUser user = await _userManager.FindByIdAsync(userId);
            if (user != null)
            {
                await _userManager.RemoveFromRoleAsync(user, roleName);
            }
        }
        public async Task<IdentityResult> DeleteUserByIdAsync(string id)
        {
            ApplicationUser user = await _userManager.FindByIdAsync(id);

            if (user == null)
            {
                return IdentityResult.Failed(new IdentityError { Description = "User not found." });
            }

            return await _userManager.DeleteAsync(user);


        }



    }
}
