using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Models.People;
using Google.Apis.Auth;
using Microsoft.AspNetCore.Authorization;
using System.Security.Cryptography;
using System.ComponentModel.DataAnnotations;
using Adidas.DTOs.People.Auth;

namespace Adidas.ClientAPI.Controllers.Auth
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly IConfiguration _configuration;
        private readonly ILogger<AuthController> _logger;

        public AuthController(
            UserManager<User> userManager,
            SignInManager<User> signInManager,
            IConfiguration configuration,
            ILogger<AuthController> logger)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _configuration = configuration;
            _logger = logger;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto registerDto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                // Check if user already exists
                var existingUser = await _userManager.FindByEmailAsync(registerDto.Email);
                if (existingUser != null)
                    return BadRequest(new { message = "User with this email already exists" });

                // Create new user with Customer role by default
                var user = new User
                {
                    UserName = registerDto.Email,
                    Email = registerDto.Email,
                    FirstName = registerDto.FirstName,
                    LastName = registerDto.LastName,
                    Phone = registerDto.Phone,
                    Role = UserRole.Customer, // Keep enum for backward compatibility
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    EmailConfirmed = true // For demo purposes, set to true
                };

                var result = await _userManager.CreateAsync(user, registerDto.Password);

                if (!result.Succeeded)
                {
                    return BadRequest(new { message = "Registration failed", errors = result.Errors });
                }

                // Assign Customer role using Identity roles
                var roleResult = await _userManager.AddToRoleAsync(user, "Customer");
                if (!roleResult.Succeeded)
                {
                    // If role assignment fails, still log the error but don't fail the registration
                    _logger.LogError("Failed to assign Customer role to user {UserId}: {Errors}",
                        user.Id, string.Join(", ", roleResult.Errors.Select(e => e.Description)));
                }

                // Generate JWT token
                var token = await GenerateJwtToken(user);
                var refreshToken = GenerateRefreshToken();

                return Ok(new AuthResponseDto
                {
                    Token = token,
                    RefreshToken = refreshToken,
                    User = new UserInfoDto
                    {
                        Id = user.Id,
                        Email = user.Email,
                        FirstName = user.FirstName,
                        LastName = user.LastName,
                        Phone = user.Phone,
                        Role = "Customer"
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during user registration");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var user = await _userManager.FindByEmailAsync(loginDto.Email);
                if (user == null)
                    return Unauthorized(new { message = "Invalid credentials" });

                // Check if user is active and not suspended
                if (!user.IsActive || user.IsDeleted)
                    return StatusCode(403, new { message = "Account is suspended or deactivated" });

                // Check if user has Customer role using Identity roles
                var userRoles = await _userManager.GetRolesAsync(user);
                if (!userRoles.Contains("Customer"))
                    return StatusCode(403, new { message = "Access denied. Customer role required." });

                var result = await _signInManager.CheckPasswordSignInAsync(user, loginDto.Password, lockoutOnFailure: false);

                if (!result.Succeeded)
                    return Unauthorized(new { message = "Invalid credentials" });

                var token = await GenerateJwtToken(user);
                var refreshToken = GenerateRefreshToken();

                // Update last login
                user.UpdatedAt = DateTime.UtcNow;
                await _userManager.UpdateAsync(user);

                return Ok(new AuthResponseDto
                {
                    Token = token,
                    RefreshToken = refreshToken,
                    User = new UserInfoDto
                    {
                        Id = user.Id,
                        Email = user.Email,
                        FirstName = user.FirstName,
                        LastName = user.LastName,
                        Phone = user.Phone,
                        Role = "Customer"
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during user login");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        [HttpPost("google-login")]
        public async Task<IActionResult> GoogleLogin([FromBody] GoogleLoginDto googleLoginDto)
        {
            try
            {
                // Verify Google token
                var googleUser = await VerifyGoogleToken(googleLoginDto.IdToken);
                if (googleUser == null)
                    return BadRequest(new { message = "Invalid Google token" });

                var user = await _userManager.FindByEmailAsync(googleUser.Email);

                if (user == null)
                {
                    // Create new user if doesn't exist
                    user = new User
                    {
                        UserName = googleUser.Email,
                        Email = googleUser.Email,
                        FirstName = googleUser.GivenName,
                        LastName = googleUser.FamilyName,
                        Role = UserRole.Customer, // Keep enum for backward compatibility
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow,
                        EmailConfirmed = true
                    };

                    var createResult = await _userManager.CreateAsync(user);
                    if (!createResult.Succeeded)
                        return BadRequest(new { message = "Failed to create user account" });

                    // Assign Customer role using Identity roles
                    var roleResult = await _userManager.AddToRoleAsync(user, "Customer");
                    if (!roleResult.Succeeded)
                    {
                        _logger.LogError("Failed to assign Customer role to Google user {UserId}: {Errors}",
                            user.Id, string.Join(", ", roleResult.Errors.Select(e => e.Description)));
                    }
                }
                else
                {
                    // Check if existing user is active
                    if (!user.IsActive || user.IsDeleted)
                        return StatusCode(403, new { message = "Account is suspended or deactivated" });

                    // Check if user has Customer role using Identity roles
                    var userRoles = await _userManager.GetRolesAsync(user);
                    if (!userRoles.Contains("Customer"))
                        return StatusCode(403, new { message = "Access denied. Customer role required." });
                }

                var token = await GenerateJwtToken(user);
                var refreshToken = GenerateRefreshToken();

                // Update last login
                user.UpdatedAt = DateTime.UtcNow;
                await _userManager.UpdateAsync(user);

                return Ok(new AuthResponseDto
                {
                    Token = token,
                    RefreshToken = refreshToken,
                    User = new UserInfoDto
                    {
                        Id = user.Id,
                        Email = user.Email,
                        FirstName = user.FirstName,
                        LastName = user.LastName,
                        Phone = user.Phone,
                        Role = "Customer"
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during Google login");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        [HttpPost("change-password")]
        [Authorize]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto changePasswordDto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var user = await _userManager.FindByIdAsync(userId);

                if (user == null)
                    return NotFound(new { message = "User not found" });

                var result = await _userManager.ChangePasswordAsync(user, changePasswordDto.CurrentPassword, changePasswordDto.NewPassword);

                if (!result.Succeeded)
                    return BadRequest(new { message = "Failed to change password", errors = result.Errors });

                return Ok(new { message = "Password changed successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during password change");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        [HttpPost("logout")]
        [Authorize]
        public async Task<IActionResult> Logout()
        {
            try
            {
                await _signInManager.SignOutAsync();
                return Ok(new { message = "Logged out successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during logout");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        private async Task<string> GenerateJwtToken(User user)
        {
            var tokenHandler = new JwtSecurityTokenHandler();

            // Get JWT settings with null checks
            var secretKey = _configuration["JwtSettings:SecretKey"];
            if (string.IsNullOrEmpty(secretKey))
            {
                _logger.LogError("JWT SecretKey is not configured in appsettings.json");
                throw new InvalidOperationException("JWT configuration is missing");
            }

            var issuer = _configuration["JwtSettings:Issuer"];
            var audience = _configuration["JwtSettings:Audience"];
            var expirationHours = _configuration["JwtSettings:ExpirationInHours"];

            if (string.IsNullOrEmpty(issuer) || string.IsNullOrEmpty(audience) || string.IsNullOrEmpty(expirationHours))
            {
                _logger.LogError("JWT configuration is incomplete in appsettings.json");
                throw new InvalidOperationException("JWT configuration is incomplete");
            }

            var key = Encoding.ASCII.GetBytes(secretKey);

            // Get user roles from Identity
            var userRoles = await _userManager.GetRolesAsync(user);

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.Email, user.Email ?? ""),
                new Claim(ClaimTypes.Name, $"{user.FirstName} {user.LastName}".Trim()),
                new Claim("userId", user.Id)
            };

            // Add role claims from Identity roles
            foreach (var role in userRoles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddHours(Convert.ToDouble(expirationHours)),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature),
                Issuer = issuer,
                Audience = audience
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        private string GenerateRefreshToken()
        {
            var randomNumber = new byte[32];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(randomNumber);
                return Convert.ToBase64String(randomNumber);
            }
        }

        private async Task<GoogleJsonWebSignature.Payload> VerifyGoogleToken(string idToken)
        {
            try
            {
                var settings = new GoogleJsonWebSignature.ValidationSettings()
                {
                    Audience = new List<string> { _configuration["Authentication:Google:ClientId"] }
                };

                var payload = await GoogleJsonWebSignature.ValidateAsync(idToken, settings);
                return payload;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error verifying Google token");
                return null;
            }
        }
    }
}

