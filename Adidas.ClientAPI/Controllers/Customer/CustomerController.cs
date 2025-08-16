using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using Models.People;
using Microsoft.EntityFrameworkCore;
using Adidas.DTOs.People.Customer_DTOs;

namespace Adidas.ClientAPI.Controllers.Customer
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Customer")] // Use Identity role authorization
    public class CustomerController : ControllerBase
    {
        private readonly UserManager<User> _userManager;
        private readonly ILogger<CustomerController> _logger;

        public CustomerController(UserManager<User> userManager, ILogger<CustomerController> logger)
        {
            _userManager = userManager;
            _logger = logger;
        }

        /// <summary>
        /// Get current customer's profile data
        /// </summary>
        /// <returns></returns>
        [HttpGet("profile")]
        public async Task<IActionResult> GetCustomerProfile()
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized(new { message = "User not authenticated" });

                var user = await _userManager.Users
                    .Where(u => u.Id == userId && !u.IsDeleted)
                    .Select(u => new CustomerProfileDto
                    {
                        Id = u.Id,
                        Email = u.Email,
                        FirstName = u.FirstName,
                        LastName = u.LastName,
                        Phone = u.Phone,
                        DateOfBirth = u.DateOfBirth,
                        Gender = u.Gender,
                        PreferredLanguage = u.PreferredLanguage,
                        CreatedAt = u.CreatedAt,
                        UpdatedAt = u.UpdatedAt,
                        IsActive = u.IsActive,
                        UserName = u.UserName,
                        PhoneNumber = u.PhoneNumber
                    })
                    .FirstOrDefaultAsync();

                if (user == null)
                    return NotFound(new { message = "Customer profile not found" });

                return Ok(new { customer = user });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving customer profile for user: {UserId}", User.FindFirstValue(ClaimTypes.NameIdentifier));
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        /// <summary>
        /// Update customer profile data
        /// </summary>
        /// <param name="updateDto"></param>
        /// <returns></returns>
        [HttpPut("profile")]
        public async Task<IActionResult> UpdateCustomerProfile([FromBody] CustomerUpdateAPIDto updateDto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized(new { message = "User not authenticated" });

                var user = await _userManager.FindByIdAsync(userId);
                if (user == null || user.IsDeleted)
                    return NotFound(new { message = "Customer not found" });

                // Verify user has Customer role using Identity
                var userRoles = await _userManager.GetRolesAsync(user);
                if (!userRoles.Contains("Customer"))
                    return StatusCode(403, new { message = "Access denied. Customer role required." });

                // Check if email is being changed and if it's already taken by another user
                if (!string.IsNullOrEmpty(updateDto.Email) && updateDto.Email != user.Email)
                {
                    var existingUser = await _userManager.FindByEmailAsync(updateDto.Email);
                    if (existingUser != null && existingUser.Id != userId)
                        return BadRequest(new { message = "Email is already taken by another user" });
                }

                // Update user properties
                if (!string.IsNullOrEmpty(updateDto.FirstName))
                    user.FirstName = updateDto.FirstName.Trim();

                if (!string.IsNullOrEmpty(updateDto.LastName))
                    user.LastName = updateDto.LastName.Trim();

                if (!string.IsNullOrEmpty(updateDto.Email))
                {
                    user.Email = updateDto.Email.Trim();
                    user.UserName = updateDto.Email.Trim(); // Keep username in sync with email
                }

                if (!string.IsNullOrEmpty(updateDto.Phone))
                {
                    user.Phone = updateDto.Phone.Trim();
                    user.PhoneNumber = updateDto.Phone.Trim(); // Keep PhoneNumber in sync
                }

                if (updateDto.DateOfBirth.HasValue)
                    user.DateOfBirth = updateDto.DateOfBirth.Value;

                if (updateDto.Gender.HasValue)
                    user.Gender = updateDto.Gender.Value;

                if (!string.IsNullOrEmpty(updateDto.PreferredLanguage))
                    user.PreferredLanguage = updateDto.PreferredLanguage;

                user.UpdatedAt = DateTime.UtcNow;

                var result = await _userManager.UpdateAsync(user);
                if (!result.Succeeded)
                {
                    return BadRequest(new
                    {
                        message = "Failed to update profile",
                        errors = result.Errors.Select(e => e.Description)
                    });
                }

                // Return updated profile
                var updatedProfile = new CustomerProfileDto
                {
                    Id = user.Id,
                    Email = user.Email,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Phone = user.Phone,
                    DateOfBirth = user.DateOfBirth,
                    Gender = user.Gender,
                    PreferredLanguage = user.PreferredLanguage,
                    CreatedAt = user.CreatedAt,
                    UpdatedAt = user.UpdatedAt,
                    IsActive = user.IsActive,
                    UserName = user.UserName,
                    PhoneNumber = user.PhoneNumber
                };

                return Ok(new { customer = updatedProfile, message = "Profile updated successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating customer profile for user: {UserId}", User.FindFirstValue(ClaimTypes.NameIdentifier));
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        /// <summary>
        /// Get basic customer info (minimal data)
        /// </summary>
        /// <returns></returns>
        [HttpGet("info")]
        public async Task<IActionResult> GetCustomerInfo()
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized(new { message = "User not authenticated" });

                var user = await _userManager.Users
                    .Where(u => u.Id == userId && !u.IsDeleted)
                    .Select(u => new CustomerInfoDto
                    {
                        Id = u.Id,
                        Email = u.Email,
                        FirstName = u.FirstName,
                        LastName = u.LastName,
                        Phone = u.Phone,
                        FullName = $"{u.FirstName} {u.LastName}".Trim()
                    })
                    .FirstOrDefaultAsync();

                if (user == null)
                    return NotFound(new { message = "Customer not found" });

                return Ok(new { customer = user });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving customer info for user: {UserId}", User.FindFirstValue(ClaimTypes.NameIdentifier));
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        /// <summary>
        /// Update customer's email
        /// </summary>
        /// <param name="emailDto"></param>
        /// <returns></returns>
        [HttpPatch("email")]
        public async Task<IActionResult> UpdateEmail([FromBody] UpdateEmailDto emailDto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized(new { message = "User not authenticated" });

                var user = await _userManager.FindByIdAsync(userId);
                if (user == null || user.IsDeleted)
                    return NotFound(new { message = "Customer not found" });

                // Verify user has Customer role using Identity
                var userRoles = await _userManager.GetRolesAsync(user);
                if (!userRoles.Contains("Customer"))
                    return StatusCode(403, new { message = "Access denied. Customer role required." });

                // Check if new email is already taken
                var existingUser = await _userManager.FindByEmailAsync(emailDto.Email);
                if (existingUser != null && existingUser.Id != userId)
                    return BadRequest(new { message = "Email is already taken by another user" });

                user.Email = emailDto.Email.Trim();
                user.UserName = emailDto.Email.Trim();
                user.UpdatedAt = DateTime.UtcNow;

                var result = await _userManager.UpdateAsync(user);
                if (!result.Succeeded)
                {
                    return BadRequest(new
                    {
                        message = "Failed to update email",
                        errors = result.Errors.Select(e => e.Description)
                    });
                }

                return Ok(new { message = "Email updated successfully", email = user.Email });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating email for user: {UserId}", User.FindFirstValue(ClaimTypes.NameIdentifier));
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        /// <summary>
        /// Update customer's phone number
        /// </summary>
        /// <param name="phoneDto"></param>
        /// <returns></returns>
        [HttpPatch("phone")]
        public async Task<IActionResult> UpdatePhone([FromBody] UpdatePhoneDto phoneDto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized(new { message = "User not authenticated" });

                var user = await _userManager.FindByIdAsync(userId);
                if (user == null || user.IsDeleted)
                    return NotFound(new { message = "Customer not found" });

                user.Phone = phoneDto.Phone.Trim();
                user.PhoneNumber = phoneDto.Phone.Trim();
                user.UpdatedAt = DateTime.UtcNow;

                var result = await _userManager.UpdateAsync(user);
                if (!result.Succeeded)
                {
                    return BadRequest(new
                    {
                        message = "Failed to update phone number",
                        errors = result.Errors.Select(e => e.Description)
                    });
                }

                return Ok(new { message = "Phone number updated successfully", phone = user.Phone });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating phone for user: {UserId}", User.FindFirstValue(ClaimTypes.NameIdentifier));
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        /// <summary>
        /// Update customer's name (first and last name)
        /// </summary>
        /// <param name="nameDto"></param>
        /// <returns></returns>
        [HttpPatch("name")]
        public async Task<IActionResult> UpdateName([FromBody] UpdateNameDto nameDto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized(new { message = "User not authenticated" });

                var user = await _userManager.FindByIdAsync(userId);
                if (user == null || user.IsDeleted)
                    return NotFound(new { message = "Customer not found" });

                user.FirstName = nameDto.FirstName.Trim();
                user.LastName = nameDto.LastName.Trim();
                user.UpdatedAt = DateTime.UtcNow;

                var result = await _userManager.UpdateAsync(user);
                if (!result.Succeeded)
                {
                    return BadRequest(new
                    {
                        message = "Failed to update name",
                        errors = result.Errors.Select(e => e.Description)
                    });
                }

                return Ok(new
                {
                    message = "Name updated successfully",
                    firstName = user.FirstName,
                    lastName = user.LastName,
                    fullName = $"{user.FirstName} {user.LastName}".Trim()
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating name for user: {UserId}", User.FindFirstValue(ClaimTypes.NameIdentifier));
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        /// <summary>
        /// Delete customer account (soft delete)
        /// </summary>
        /// <returns></returns>
        [HttpDelete("account")]
        public async Task<IActionResult> DeleteAccount()
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized(new { message = "User not authenticated" });

                var user = await _userManager.FindByIdAsync(userId);
                if (user == null || user.IsDeleted)
                    return NotFound(new { message = "Customer not found" });

                // Soft delete
                user.IsDeleted = true;
                user.IsActive = false;
                user.UpdatedAt = DateTime.UtcNow;

                var result = await _userManager.UpdateAsync(user);
                if (!result.Succeeded)
                {
                    return BadRequest(new
                    {
                        message = "Failed to delete account",
                        errors = result.Errors.Select(e => e.Description)
                    });
                }

                return Ok(new { message = "Account deleted successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting account for user: {UserId}", User.FindFirstValue(ClaimTypes.NameIdentifier));
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        /// <summary>
        /// Get customer statistics/summary
        /// </summary>
        /// <returns></returns>
        [HttpGet("summary")]
        public async Task<IActionResult> GetCustomerSummary()
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized(new { message = "User not authenticated" });

                var user = await _userManager.Users
                    .Where(u => u.Id == userId && !u.IsDeleted)
                    .Include(u => u.Orders)
                    .Include(u => u.Addresses)
                    .Include(u => u.Reviews)
                    .Include(u => u.Wishlists)
                    .Select(u => new CustomerSummaryDto
                    {
                        Id = u.Id,
                        Email = u.Email,
                        FirstName = u.FirstName,
                        LastName = u.LastName,
                        MemberSince = u.CreatedAt,
                        TotalOrders = u.Orders.Count(o => !o.IsDeleted),
                        TotalAddresses = u.Addresses.Count(a => !a.IsDeleted),
                        TotalReviews = u.Reviews.Count(r => !r.IsDeleted),
                        TotalWishlistItems = u.Wishlists.Count(w => !w.IsDeleted),
                        IsActive = u.IsActive
                    })
                    .FirstOrDefaultAsync();

                if (user == null)
                    return NotFound(new { message = "Customer not found" });

                return Ok(new { summary = user });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving customer summary for user: {UserId}", User.FindFirstValue(ClaimTypes.NameIdentifier));
                return StatusCode(500, new { message = "Internal server error" });
            }
        }
    }
}

