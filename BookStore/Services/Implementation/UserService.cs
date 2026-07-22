using BookStore.Areas.Admin.Models;
using BookStore.Data;
using BookStore.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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
            var users = await _userManager.Users
                .AsNoTracking()
                .ToListAsync();

            foreach (var user in users)
            {
                user.RoleNames = await _userManager.GetRolesAsync(user);
            }

            return new UserVM
            {
                Users = users,
                Roles = await _roleManager.Roles
                    .AsNoTracking()
                    .ToListAsync()
            };
        }

        public async Task<bool> AddUserToRoleAsync(string id, string roleName)
        {
            IdentityRole? role = await _roleManager.FindByNameAsync(roleName);
            if (role == null)
            {
                _logger.LogWarning("AddUserToRole: role '{RoleName}' not found", roleName);
                return false;
            }

            ApplicationUser? user = await _userManager.FindByIdAsync(id);
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
            ApplicationUser? user = await _userManager.FindByIdAsync(userId);
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
            ApplicationUser? user = await _userManager.FindByIdAsync(id);

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

        public async Task CreateAdminRoleAsync()
        {
            if (!await _roleManager.RoleExistsAsync("Admin"))
            {
                _logger.LogInformation("Creating Admin role");
                await _roleManager.CreateAsync(new IdentityRole("Admin"));
            }
        }

        public async Task<bool> DeleteRoleByIdAsync(string roleId)
        {
            IdentityRole? role = await _roleManager.FindByIdAsync(roleId);
            if (role != null)
            {
                _logger.LogInformation("Role '{RoleName}' (id: {RoleId}) deleted via admin", role.Name, roleId);
                var result = await _roleManager.DeleteAsync(role);
                return result.Succeeded;
            }
            _logger.LogWarning("DeleteRole: role {RoleId} not found", roleId);
            return false;
        }
    }
}
