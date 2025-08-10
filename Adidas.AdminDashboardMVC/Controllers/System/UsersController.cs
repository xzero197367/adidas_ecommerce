// Updated Users Controller - Added Create User functionality for Admins
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Models.People;
using System.Security.Claims;
using Adidas.AdminDashboardMVC.ViewModels.Users;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Authorization;

namespace Adidas.AdminDashboardMVC.Controllers.System
{
    [Authorize(Roles = "Admin")]
    public class UsersController : BaseController
    {
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ILogger<UsersController> _logger;

        public UsersController(UserManager<User> userManager, RoleManager<IdentityRole> roleManager, ILogger<UsersController> logger)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _logger = logger;
        }

        public async Task<IActionResult> Index(string searchTerm = "", string roleFilter = "All", int page = 1, int pageSize = 10)
        {
            // Only admins can access this page - get all users including clients
            var query = _userManager.Users
    .Where(u => u.Role == UserRole.Admin || u.Role == UserRole.Employee)
    .AsQueryable();


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
                TotalActiveUsers = await _userManager.Users.CountAsync(u => u.IsActive),
                AdminRolesCount = await _userManager.Users.CountAsync(u => u.Role == UserRole.Admin),
                PendingApproval = await _userManager.Users.CountAsync(u => !u.EmailConfirmed)
            };

            ViewBag.Roles = new List<SelectListItem>
            {
                new SelectListItem { Value = "All", Text = "All Roles" },
                new SelectListItem { Value = "Admin", Text = "Admin" },
                new SelectListItem { Value = "Employee", Text = "Employee" },
            };

            return View(viewModel);
        }

        [HttpGet]
        public IActionResult Create()
        {
            var model = new CreateUserViewModel();

            ViewBag.Roles = new List<SelectListItem>
            {
                new SelectListItem { Value = "Admin", Text = "Admin" },
                new SelectListItem { Value = "Employee", Text = "Employee" },
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateUserViewModel model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Roles = new List<SelectListItem>
                {
                    new SelectListItem { Value = "Admin", Text = "Admin" },
                    new SelectListItem { Value = "Employee", Text = "Employee" },
                };
                return View(model);
            }

            // Check if user already exists
            var existingUser = await _userManager.FindByEmailAsync(model.Email);
            if (existingUser != null)
            {
                ModelState.AddModelError("Email", "A user with this email already exists.");
                ViewBag.Roles = new List<SelectListItem>
                {
                    new SelectListItem { Value = "Admin", Text = "Admin" },
                    new SelectListItem { Value = "Employee", Text = "Employee" },
                };
                return View(model);
            }

            if (!Enum.TryParse<UserRole>(model.Role, out var roleEnum))
            {
                ModelState.AddModelError("Role", "Invalid role selected.");
                ViewBag.Roles = new List<SelectListItem>
                {
                    new SelectListItem { Value = "Admin", Text = "Admin" },
                    new SelectListItem { Value = "Employee", Text = "Employee" },
                };
                return View(model);
            }

            var user = new User
            {
                UserName = model.Email,
                Email = model.Email,
                PhoneNumber = model.PhoneNumber,
                Role = roleEnum,
                CreatedAt = DateTime.UtcNow,
                IsActive = model.IsActive,
                EmailConfirmed = true, // Admin-created users are auto-confirmed
                DateOfBirth = model.DateOfBirth,
                Gender = model.Gender.HasValue ? (Gender)Enum.Parse(typeof(Gender), model.Gender.ToString()) : null
            };

            var result = await _userManager.CreateAsync(user, model.Password);

            if (result.Succeeded)
            {
                // Ensure the role exists in Identity
                if (!await _roleManager.RoleExistsAsync(model.Role))
                {
                    var roleResult = await _roleManager.CreateAsync(new IdentityRole(model.Role));
                    if (!roleResult.Succeeded)
                    {
                        _logger.LogError("Failed to create role: {Role}", model.Role);
                    }
                }

                // Assign the user to the selected role
                await _userManager.AddToRoleAsync(user, model.Role);

                _logger.LogInformation("Admin created new user: {Email} with role: {Role}", user.Email, model.Role);
                TempData["SuccessMessage"] = $"User {user.Email} created successfully with role {model.Role}.";
                return RedirectToAction(nameof(Index));
            }

            // If user creation failed, show validation errors
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            ViewBag.Roles = new List<SelectListItem>
            {
                new SelectListItem { Value = "Admin", Text = "Admin" },
                new SelectListItem { Value = "Employee", Text = "Employee" },
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> ToggleUserStatus(string userId)
        {
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
                _logger.LogInformation("Admin toggled user status: {Email} - Active: {IsActive}", user.Email, user.IsActive);
                return Json(new { success = true, isActive = user.IsActive });
            }

            return Json(new { success = false, message = "Failed to update user status" });
        }

        [HttpPost]
        public async Task<IActionResult> ChangeUserRole(string userId, string newRole)
        {
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

            // Ensure role exists
            if (!await _roleManager.RoleExistsAsync(newRole))
            {
                await _roleManager.CreateAsync(new IdentityRole(newRole));
            }

            // Add user to new role
            await _userManager.AddToRoleAsync(user, newRole);

            var result = await _userManager.UpdateAsync(user);
            if (result.Succeeded)
            {
                _logger.LogInformation("Admin changed user role: {Email} to {Role}", user.Email, newRole);
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
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return Json(new { success = false, message = "User not found" });
            }

            // Prevent admin from deleting themselves
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser.Id == userId)
            {
                return Json(new { success = false, message = "You cannot delete your own account" });
            }

            var result = await _userManager.DeleteAsync(user);
            if (result.Succeeded)
            {
                _logger.LogInformation("Admin deleted user: {Email}", user.Email);
                return Json(new { success = true });
            }

            return Json(new { success = false, message = "Failed to delete user" });
        }

        [HttpGet]
        public async Task<IActionResult> Edit(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            var model = new EditUserViewModel
            {
                Id = user.Id,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                Role = user.Role.ToString(),
                IsActive = user.IsActive,
                DateOfBirth = user.DateOfBirth,
                Gender = user.Gender.HasValue ? (ViewModels.Users.GenderCreationStatus?)Enum.Parse(typeof(ViewModels.Users.GenderCreationStatus), user.Gender.ToString()) : null
            };

            ViewBag.Roles = new List<SelectListItem>
            {
                new SelectListItem { Value = "Admin", Text = "Admin", Selected = model.Role == "Admin" },
                new SelectListItem { Value = "Employee", Text = "Employee", Selected = model.Role == "Employee" },
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(EditUserViewModel model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Roles = new List<SelectListItem>
                {
                    new SelectListItem { Value = "Admin", Text = "Admin", Selected = model.Role == "Admin" },
                    new SelectListItem { Value = "Employee", Text = "Employee", Selected = model.Role == "Employee" },
                };
                return View(model);
            }

            var user = await _userManager.FindByIdAsync(model.Id);
            if (user == null)
            {
                return NotFound();
            }

            // Update user properties
            user.PhoneNumber = model.PhoneNumber;
            user.IsActive = model.IsActive;
            user.UpdatedAt = DateTime.UtcNow;
            user.DateOfBirth = model.DateOfBirth;
            user.Gender = model.Gender.HasValue ? (Gender?)Enum.Parse(typeof(Gender), model.Gender.ToString()) : null;

            // Handle role change
            if (user.Role.ToString() != model.Role)
            {
                if (Enum.TryParse<UserRole>(model.Role, out var newRole))
                {
                    var currentRoles = await _userManager.GetRolesAsync(user);
                    if (currentRoles.Any())
                    {
                        await _userManager.RemoveFromRolesAsync(user, currentRoles);
                    }

                    user.Role = newRole;

                    // Ensure role exists
                    if (!await _roleManager.RoleExistsAsync(model.Role))
                    {
                        await _roleManager.CreateAsync(new IdentityRole(model.Role));
                    }

                    await _userManager.AddToRoleAsync(user, model.Role);
                }
            }

            var result = await _userManager.UpdateAsync(user);
            if (result.Succeeded)
            {
                _logger.LogInformation("Admin updated user: {Email}", user.Email);
                TempData["SuccessMessage"] = "User updated successfully.";
                return RedirectToAction(nameof(Details), new { id = user.Id });
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            ViewBag.Roles = new List<SelectListItem>
            {
                new SelectListItem { Value = "Admin", Text = "Admin", Selected = model.Role == "Admin" },
                new SelectListItem { Value = "Employee", Text = "Employee", Selected = model.Role == "Employee" },
            };

            return View(model);
        }
    }
}