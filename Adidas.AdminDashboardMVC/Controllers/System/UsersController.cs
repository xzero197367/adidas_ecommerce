using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Models.People;
using Adidas.AdminDashboardMVC.ViewModels;
using System.Security.Claims;
using Adidas.AdminDashboardMVC.ViewModels.Users;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Adidas.AdminDashboardMVC.Controllers.System
{
    public class UsersController : BaseController
    {
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public UsersController(UserManager<User> userManager, RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public async Task<IActionResult> Index(string searchTerm = "", string roleFilter = "All", int page = 1, int pageSize = 10)
        {
            // Only admins and employees can access this page
            var query = _userManager.Users
                .Where(u => u.Role == UserRole.Admin || u.Role == UserRole.Employee);

            // Apply search filter
            if (!string.IsNullOrEmpty(searchTerm))
            {
                query = query.Where(u => u.Email.Contains(searchTerm) ||
                                       u.UserName.Contains(searchTerm) ||
                                       u.PhoneNumber.Contains(searchTerm));
            }

            // Apply role filter
            if (roleFilter != "All")
            {
                if (Enum.TryParse<UserRole>(roleFilter, out var role))
                {
                    query = query.Where(u => u.Role == role);
                }
            }

            var totalUsers = await query.CountAsync();
            var users = await query
                .OrderByDescending(u => u.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var userViewModels = new List<UserViewModel>();
            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                userViewModels.Add(new UserViewModel
                {
                    Id = user.Id,
                    Email = user.Email,
                    UserName = user.UserName,
                    PhoneNumber = user.PhoneNumber,
                    IsActive = user.IsActive,
                    Role = user.Role.ToString(),
                    CreatedAt = user.CreatedAt,
                    LastLogin = user.UpdatedAt ?? user.CreatedAt,
                    EmailConfirmed = user.EmailConfirmed
                });
            }

            var viewModel = new UsersIndexViewModel
            {
                Users = userViewModels,
                SearchTerm = searchTerm,
                RoleFilter = roleFilter,
                CurrentPage = page,
                PageSize = pageSize,
                TotalUsers = totalUsers,
                TotalPages = (int)Math.Ceiling((double)totalUsers / pageSize),
                TotalActiveUsers = await _userManager.Users.CountAsync(u => u.IsActive && (u.Role == UserRole.Admin || u.Role == UserRole.Employee)),
                AdminRolesCount = await _userManager.Users.CountAsync(u => u.Role == UserRole.Admin),
                PendingApproval = await _userManager.Users.CountAsync(u => !u.EmailConfirmed && (u.Role == UserRole.Admin || u.Role == UserRole.Employee))
            };
            ViewBag.Roles = new List<SelectListItem>
    {
        new SelectListItem { Value = "All", Text = "All Roles" },
        new SelectListItem { Value = "Admin", Text = "Admin" },
        new SelectListItem { Value = "Employee", Text = "Employee" }
    };

            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> ToggleUserStatus(string userId)
        {
            if (!IsAdmin())
            {
                return Json(new { success = false, message = "Unauthorized" });
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return Json(new { success = false, message = "User not found" });
            }

            user.IsActive = !user.IsActive;
            user.UpdatedAt = DateTime.UtcNow;

            var result = await _userManager.UpdateAsync(user);
            if (result.Succeeded)
            {
                return Json(new { success = true, isActive = user.IsActive });
            }

            return Json(new { success = false, message = "Failed to update user status" });
        }

        [HttpPost]
        public async Task<IActionResult> ChangeUserRole(string userId, string newRole)
        {
            if (!IsAdmin())
            {
                return Json(new { success = false, message = "Unauthorized" });
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return Json(new { success = false, message = "User not found" });
            }

            if (!Enum.TryParse<UserRole>(newRole, out var roleEnum))
            {
                return Json(new { success = false, message = "Invalid role" });
            }

            // Remove user from current roles
            var currentRoles = await _userManager.GetRolesAsync(user);
            if (currentRoles.Any())
            {
                await _userManager.RemoveFromRolesAsync(user, currentRoles);
            }

            // Update user role
            user.Role = roleEnum;
            user.UpdatedAt = DateTime.UtcNow;

            // Add user to new role
            await _userManager.AddToRoleAsync(user, newRole);

            var result = await _userManager.UpdateAsync(user);
            if (result.Succeeded)
            {
                return Json(new { success = true, role = newRole });
            }

            return Json(new { success = false, message = "Failed to update user role" });
        }

        public async Task<IActionResult> Details(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            var roles = await _userManager.GetRolesAsync(user);
            var viewModel = new UserDetailsViewModel
            {
                Id = user.Id,
                Email = user.Email,
                UserName = user.UserName,
                PhoneNumber = user.PhoneNumber,
                IsActive = user.IsActive,
                Role = user.Role.ToString(),
                CreatedAt = user.CreatedAt,
                UpdatedAt = user.UpdatedAt,
                EmailConfirmed = user.EmailConfirmed,
                DateOfBirth = user.DateOfBirth,
                Gender = user.Gender?.ToString(),
                PreferredLanguage = user.PreferredLanguage
            };

            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> DeleteUser(string userId)
        {
            if (!IsAdmin())
            {
                return Json(new { success = false, message = "Unauthorized" });
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return Json(new { success = false, message = "User not found" });
            }

            var result = await _userManager.DeleteAsync(user);
            if (result.Succeeded)
            {
                return Json(new { success = true });
            }

            return Json(new { success = false, message = "Failed to delete user" });
        }
    }
}