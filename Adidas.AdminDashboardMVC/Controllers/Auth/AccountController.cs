// Updated Account Controller - Added admin-initiated Google user creation
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using Models.People;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Adidas.AdminDashboardMVC.ViewModels.Account;

namespace Adidas.AdminDashboardMVC.Controllers.Auth
{
    [Authorize(Policy = "EmployeeOrAdmin")]

    public class AccountController : Controller
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ILogger<AccountController> _logger;

        public AccountController(
            UserManager<User> userManager,
            SignInManager<User> signInManager,
            RoleManager<IdentityRole> roleManager,
            ILogger<AccountController> logger)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
            _logger = logger;
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Login(string returnUrl = null)
        {
            if (User.Identity.IsAuthenticated)
                return RedirectToAction("Index", "Dashboard");

            ViewData["ReturnUrl"] = returnUrl;
            return View(new LoginViewModel());
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;

            if (!ModelState.IsValid)
                return View(model);

            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                return View(model);
            }

            // Check if user is active
            if (!user.IsActive)
            {
                ModelState.AddModelError(string.Empty, "Your account has been deactivated. Please contact administrator.");
                return View(model);
            }

            var result = await _signInManager.PasswordSignInAsync(user.UserName, model.Password, model.RememberMe, lockoutOnFailure: true);

            if (result.Succeeded)
            {
                // Add custom claims
                var claims = new List<Claim>
                {
                    new Claim("IsActive", user.IsActive.ToString()),
                    new Claim("Role", user.Role.ToString())
                };

                await _userManager.AddClaimsAsync(user, claims);

                _logger.LogInformation("User logged in.");
                return RedirectToLocal(returnUrl);
            }

            if (result.IsLockedOut)
            {
                _logger.LogWarning("User account locked out.");
                ModelState.AddModelError(string.Empty, "Account locked out. Please try again later.");
            }
            else
            {
                ModelState.AddModelError(string.Empty, "Invalid login attempt.");
            }

            return View(model);
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult ExternalLogin(string provider, string returnUrl = null, string role = null, string phoneNumber = null, string dateOfBirth = null, string gender = null, bool isActive = true, string createMode = null)
        {
            // Store admin creation parameters in TempData for use after authentication
            if (createMode == "admin" && User.Identity.IsAuthenticated && User.IsInRole("Admin"))
            {
                TempData["AdminCreateMode"] = "true";
                TempData["AdminCreateRole"] = role;
                TempData["AdminCreatePhoneNumber"] = phoneNumber;
                TempData["AdminCreateDateOfBirth"] = dateOfBirth;
                TempData["AdminCreateGender"] = gender;
                TempData["AdminCreateIsActive"] = isActive.ToString();
            }

            var redirectUrl = Url.Action(nameof(ExternalLoginCallback), "Account", new { returnUrl });
            var properties = _signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl);
            return Challenge(properties, provider);
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> ExternalLoginCallback(string returnUrl = null, string remoteError = null)
        {
            if (remoteError != null)
            {
                ModelState.AddModelError(string.Empty, $"Error from external provider: {remoteError}");
                return View(nameof(Login));
            }

            var info = await _signInManager.GetExternalLoginInfoAsync();
            if (info == null)
            {
                return RedirectToAction(nameof(Login));
            }

            // Check if this is an admin-initiated user creation
            bool isAdminCreateMode = TempData["AdminCreateMode"]?.ToString() == "true";

            var result = await _signInManager.ExternalLoginSignInAsync(info.LoginProvider, info.ProviderKey, isPersistent: false, bypassTwoFactor: true);

            if (result.Succeeded)
            {
                var user = await _userManager.FindByLoginAsync(info.LoginProvider, info.ProviderKey);
                if (!user.IsActive)
                {
                    await _signInManager.SignOutAsync();
                    ModelState.AddModelError(string.Empty, "Your account has been deactivated.");
                    return View("Login");
                }

                _logger.LogInformation("User logged in with {Name} provider.", info.LoginProvider);
                return RedirectToLocal(returnUrl);
            }

            if (result.IsLockedOut)
            {
                return View("Lockout");
            }
            else
            {
                // Handle user creation
                if (isAdminCreateMode)
                {
                    // Admin is creating a user via Google authentication
                    return await HandleAdminGoogleUserCreation(info, returnUrl);
                }
                else
                {
                    // Regular external login - not allowed for new users
                    ModelState.AddModelError(string.Empty, "No account found. Please contact your administrator to create an account.");
                    return View("Login");
                }
            }
        }

        private async Task<IActionResult> HandleAdminGoogleUserCreation(ExternalLoginInfo info, string returnUrl)
        {
            try
            {
                // Get admin creation parameters
                var roleString = TempData["AdminCreateRole"]?.ToString();
                var phoneNumber = TempData["AdminCreatePhoneNumber"]?.ToString();
                var dateOfBirthString = TempData["AdminCreateDateOfBirth"]?.ToString();
                var genderString = TempData["AdminCreateGender"]?.ToString();
                var isActiveString = TempData["AdminCreateIsActive"]?.ToString();
                var isActive = bool.Parse(isActiveString ?? "true");

                if (string.IsNullOrEmpty(roleString) || !Enum.TryParse<UserRole>(roleString, out var userRole))
                {
                    TempData["ErrorMessage"] = "Invalid role specified for user creation.";
                    return RedirectToAction("Create", "Users");
                }

                var email = info.Principal.FindFirstValue(ClaimTypes.Email);
                if (string.IsNullOrEmpty(email))
                {
                    TempData["ErrorMessage"] = "Unable to get email from Google account.";
                    return RedirectToAction("Create", "Users");
                }

                // Check if user already exists
                var existingUser = await _userManager.FindByEmailAsync(email);
                if (existingUser != null)
                {
                    TempData["ErrorMessage"] = $"A user with email {email} already exists.";
                    return RedirectToAction("Create", "Users");
                }

                // Parse optional fields
                DateTime? dateOfBirth = null;
                if (!string.IsNullOrEmpty(dateOfBirthString) && DateTime.TryParse(dateOfBirthString, out var parsedDate))
                {
                    dateOfBirth = parsedDate;
                }

                Gender? gender = null;
                if (!string.IsNullOrEmpty(genderString) && Enum.TryParse<Gender>(genderString, out var parsedGender))
                {
                    gender = parsedGender;
                }

                // Create the user
                var user = new User
                {
                    UserName = email,
                    Email = email,
                    PhoneNumber = phoneNumber,
                    Role = userRole,
                    CreatedAt = DateTime.UtcNow,
                    IsActive = isActive,
                    EmailConfirmed = true, // Google users are auto-confirmed
                    DateOfBirth = dateOfBirth,
                    Gender = gender
                };

                var createResult = await _userManager.CreateAsync(user);
                if (createResult.Succeeded)
                {
                    // Add the external login
                    var addLoginResult = await _userManager.AddLoginAsync(user, info);
                    if (addLoginResult.Succeeded)
                    {
                        // Ensure role exists and assign it
                        if (!await _roleManager.RoleExistsAsync(roleString))
                        {
                            await _roleManager.CreateAsync(new IdentityRole(roleString));
                        }
                        await _userManager.AddToRoleAsync(user, roleString);

                        _logger.LogInformation("Admin created user via Google: {Email} with role: {Role}", email, roleString);
                        TempData["SuccessMessage"] = $"User {email} created successfully via Google with role {roleString}.";

                        return RedirectToAction("Index", "Users");
                    }
                    else
                    {
                        // If adding login failed, delete the user
                        await _userManager.DeleteAsync(user);
                        TempData["ErrorMessage"] = "Failed to link Google account. Please try again.";
                        return RedirectToAction("Create", "Users");
                    }
                }
                else
                {
                    var errors = string.Join(", ", createResult.Errors.Select(e => e.Description));
                    TempData["ErrorMessage"] = $"Failed to create user: {errors}";
                    return RedirectToAction("Create", "Users");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating user via Google authentication");
                TempData["ErrorMessage"] = "An error occurred while creating the user. Please try again.";
                return RedirectToAction("Create", "Users");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            _logger.LogInformation("User logged out.");
            return RedirectToAction("Login", "Account");
        }

        [Authorize]
        public async Task<IActionResult> Profile()
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
                return RedirectToAction("Login");

            var roles = await _userManager.GetRolesAsync(currentUser);

            var viewModel = new UserProfileViewModel
            {
                Email = currentUser.Email,
                Role = roles.FirstOrDefault() ?? "N/A",
                CreatedAt = currentUser.CreatedAt,
                IsActive = currentUser.IsActive,
                EmailConfirmed = currentUser.EmailConfirmed,
                DateOfBirth = currentUser.DateOfBirth,
                PhoneNumber = currentUser.PhoneNumber,
                Gender = currentUser.Gender?.ToString() ?? "Not Specified",
                PreferredLanguage = currentUser.PreferredLanguage
            };

            return View(viewModel);
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> EditProfile()
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
                return RedirectToAction("Login");

            var viewModel = new EditUserProfileViewModel
            {
                Email = currentUser.Email,
                PhoneNumber = currentUser.PhoneNumber,
                DateOfBirth = currentUser.DateOfBirth,
                PreferredLanguage = currentUser.PreferredLanguage,
                Gender = currentUser.Gender?.ToString()
            };

            return View(viewModel);
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditProfile(EditUserProfileViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
                return RedirectToAction("Login");

            // Update user fields
            currentUser.Email = model.Email;
            currentUser.UserName = model.Email; // keep UserName in sync
            currentUser.PhoneNumber = model.PhoneNumber;
            currentUser.DateOfBirth = model.DateOfBirth;
            currentUser.PreferredLanguage = model.PreferredLanguage;

            if (!string.IsNullOrEmpty(model.Gender) &&
                Enum.TryParse<Gender>(model.Gender, out var genderValue))
            {
                currentUser.Gender = genderValue;
            }

            var result = await _userManager.UpdateAsync(currentUser);

            if (result.Succeeded)
            {
                TempData["SuccessMessage"] = "Profile updated successfully.";
                return RedirectToAction("Profile");
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return View(model);
        }


        [HttpGet]
        [Authorize]
        public IActionResult ChangePassword()
        {
            return View();
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return RedirectToAction("Login");

            var result = await _userManager.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword);
            if (result.Succeeded)
            {
                await _signInManager.RefreshSignInAsync(user); // refresh login
                TempData["SuccessMessage"] = "Your password has been changed.";
                return RedirectToAction("Profile");
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return View(model);
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult AccessDenied()
        {
            return View();
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Error(int statusCode)
        {
            if (statusCode == 404)
            {
                return View("NotFound");
            }

            return View("Error");
        }

        private IActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            else
            {
                return RedirectToAction("Index", "Dashboard");
            }
        }

        private bool IsAdmin()
        {
            return User.IsInRole("Admin");
        }
    }
}