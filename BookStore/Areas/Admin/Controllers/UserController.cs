using BookStore.Areas.Admin.Models;
using BookStore.Data;
using BookStore.Models;
using BookStore.Services.Implementaion;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Linq;

namespace BookStore.Areas.Admin.Controllers
{
    [Authorize(Roles = "Admin")]
    [Area("Admin")]
    public class UserController : Controller
    {
        private UserManager<ApplicationUser> _userManager;
        private RoleManager<IdentityRole> _roleManager;
        private UserService _userService;
        private readonly ILogger<UserController> _logger;

        public UserController(
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            UserService userService,
            ILogger<UserController> logger)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _userService = userService;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            UserVM model = await _userService.GetUsersAsync();
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Delete(string id)
        {
            IdentityResult result = await _userService.DeleteUserByIdAsync(id);

            if (!result.Succeeded)
            {
                _logger.LogWarning("Delete user {UserId} failed: {Errors}", id,
                    string.Join(", ", result.Errors.Select(e => e.Description)));

                foreach (IdentityError error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
            }
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> AddToRole(string id, string roleName)
        {
            if (!await _userService.AddUserToRoleAsync(id, roleName))
            {
                return NotFound();
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> RemoveFromRole(string userId, string roleName)
        {
            await _userService.RemoveUserFromRoleAsync(userId, roleName);
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> CreateRole(string roleName)
        {
            if (!await _userService.CreateRoleAsync(roleName))
            {
                ModelState.AddModelError("", "role already exists");
                return View("Index", await _userService.GetUsersAsync());
            }
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> CreateAdminRole()
        {
            _logger.LogInformation("CreateAdminRole utility action invoked");
            await _roleManager.CreateAsync(new IdentityRole("Admin"));
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> DeleteRole(string id)
        {
            IdentityRole role = await _roleManager.FindByIdAsync(id);
            if (role != null)
            {
                _logger.LogInformation("Role '{RoleName}' (id: {RoleId}) deleted via admin", role.Name, id);
                await _roleManager.DeleteAsync(role);
            }
            else
            {
                _logger.LogWarning("DeleteRole: role {RoleId} not found", id);
            }
            return RedirectToAction("Index");
        }
    }
}
