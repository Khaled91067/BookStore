using BookStore.Areas.Admin.Models;
using BookStore.Data;
using BookStore.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace BookStore.Services.Implementaion
{
    public class UserService
    {
        private UserManager<ApplicationUser> _userManager;
        private RoleManager<IdentityRole> _roleManager;
        private readonly ILogger<UserService> _logger;

        public UserService(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager, ILogger<UserService> logger)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _logger = logger;
        }

        public async Task<bool> CreateRoleAsync(string roleName)
        {
            if (await _roleManager.RoleExistsAsync(roleName))
            {
                _logger.LogWarning("CreateRole: role '{RoleName}' already exists", roleName);
                return false;
            }

            IdentityRole role = new IdentityRole { Name = roleName };
            await _roleManager.CreateAsync(role);

            _logger.LogInformation("Role '{RoleName}' created", roleName);
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
            {
                _logger.LogWarning("AddUserToRole: role '{RoleName}' not found", roleName);
                return false;
            }

            ApplicationUser user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                _logger.LogWarning("AddUserToRole: user {UserId} not found", id);
                return false;
            }

            await _userManager.AddToRoleAsync(user, roleName);
            _logger.LogInformation("User {UserId} added to role '{RoleName}'", id, roleName);
            return true;
        }

        public async Task RemoveUserFromRoleAsync(string userId, string roleName)
        {
            ApplicationUser user = await _userManager.FindByIdAsync(userId);
            if (user != null)
            {
                await _userManager.RemoveFromRoleAsync(user, roleName);
                _logger.LogInformation("User {UserId} removed from role '{RoleName}'", userId, roleName);
            }
            else
            {
                _logger.LogWarning("RemoveUserFromRole: user {UserId} not found", userId);
            }
        }

        public async Task<IdentityResult> DeleteUserByIdAsync(string id)
        {
            ApplicationUser user = await _userManager.FindByIdAsync(id);

            if (user == null)
            {
                _logger.LogWarning("DeleteUser: user {UserId} not found", id);
                return IdentityResult.Failed(new IdentityError { Description = "User not found." });
            }

            var result = await _userManager.DeleteAsync(user);

            if (result.Succeeded)
                _logger.LogInformation("User {UserId} deleted", id);
            else
                _logger.LogWarning("DeleteUser failed for {UserId}: {Errors}", id, string.Join(", ", result.Errors.Select(e => e.Description)));

            return result;
        }
    }
}
