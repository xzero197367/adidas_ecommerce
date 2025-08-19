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
// Add these using statements at the top
using System.Text.Json;
using System.Net.Http;
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



// Add this method to handle authorization code exchange
[HttpPost("google-login")]
    public async Task<IActionResult> GoogleLogin([FromBody] GoogleLoginDto googleLoginDto)
    {
        try
        {
            GoogleJsonWebSignature.Payload googleUser = null;

            // Handle both IdToken and AuthorizationCode
            if (!string.IsNullOrEmpty(googleLoginDto.IdToken))
            {
                // Direct ID Token verification (existing flow)
                googleUser = await VerifyGoogleToken(googleLoginDto.IdToken);
            }
            else if (!string.IsNullOrEmpty(googleLoginDto.AuthorizationCode))
            {
                // Authorization Code flow (new flow)
                googleUser = await ExchangeAuthorizationCodeForUserInfo(googleLoginDto.AuthorizationCode, googleLoginDto.RedirectUri);
            }
            else
            {
                return BadRequest(new { message = "Either IdToken or AuthorizationCode is required" });
            }

            if (googleUser == null)
                return BadRequest(new { message = "Invalid Google authentication" });

            var user = await _userManager.FindByEmailAsync(googleUser.Email);

            if (user == null)
            {
                // Create new user if doesn't exist (Registration)
                user = new User
                {
                    UserName = googleUser.Email,
                    Email = googleUser.Email,
                    FirstName = googleUser.GivenName ?? googleUser.Name?.Split(' ').FirstOrDefault() ?? "",
                    LastName = googleUser.FamilyName ?? googleUser.Name?.Split(' ').LastOrDefault() ?? "",
                    Role = UserRole.Customer,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    EmailConfirmed = true
                };

                var createResult = await _userManager.CreateAsync(user);
                if (!createResult.Succeeded)
                    return BadRequest(new { message = "Failed to create user account", errors = createResult.Errors });

                // Assign Customer role using Identity roles
                var roleResult = await _userManager.AddToRoleAsync(user, "Customer");
                if (!roleResult.Succeeded)
                {
                    _logger.LogError("Failed to assign Customer role to Google user {UserId}: {Errors}",
                        user.Id, string.Join(", ", roleResult.Errors.Select(e => e.Description)));
                }

                _logger.LogInformation("New Google user registered: {Email}", user.Email);
            }
            else
            {
                // Existing user login
                if (!user.IsActive || user.IsDeleted)
                    return StatusCode(403, new { message = "Account is suspended or deactivated" });

                var userRoles = await _userManager.GetRolesAsync(user);
                if (!userRoles.Contains("Customer"))
                    return StatusCode(403, new { message = "Access denied. Customer role required." });

                _logger.LogInformation("Existing Google user logged in: {Email}", user.Email);
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
            _logger.LogError(ex, "Error during Google authentication");
            return StatusCode(500, new { message = "Internal server error" });
        }
    }

    // Add this new method to exchange authorization code for user info
    private async Task<GoogleJsonWebSignature.Payload> ExchangeAuthorizationCodeForUserInfo(string authorizationCode, string redirectUri)
    {
        try
        {
            using var httpClient = new HttpClient();

            // Step 1: Exchange authorization code for tokens
            var tokenRequest = new FormUrlEncodedContent(new[]
            {
            new KeyValuePair<string, string>("code", authorizationCode),
            new KeyValuePair<string, string>("client_id", _configuration["Authentication:Google:ClientId"]),
            new KeyValuePair<string, string>("client_secret", _configuration["Authentication:Google:ClientSecret"]),
            new KeyValuePair<string, string>("redirect_uri", redirectUri),
            new KeyValuePair<string, string>("grant_type", "authorization_code")
        });

            var tokenResponse = await httpClient.PostAsync("https://oauth2.googleapis.com/token", tokenRequest);
            var tokenContent = await tokenResponse.Content.ReadAsStringAsync();

            if (!tokenResponse.IsSuccessStatusCode)
            {
                _logger.LogError("Failed to exchange authorization code: {Response}", tokenContent);
                return null;
            }

            var tokenData = JsonSerializer.Deserialize<GoogleTokenResponse>(tokenContent);

            // Step 2: Get user info using access token
            httpClient.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", tokenData.access_token);

            var userInfoResponse = await httpClient.GetAsync("https://www.googleapis.com/oauth2/v2/userinfo");
            var userInfoContent = await userInfoResponse.Content.ReadAsStringAsync();

            if (!userInfoResponse.IsSuccessStatusCode)
            {
                _logger.LogError("Failed to get user info from Google: {Response}", userInfoContent);
                return null;
            }

            var userInfo = JsonSerializer.Deserialize<GoogleUserInfo>(userInfoContent);

            // Convert to GoogleJsonWebSignature.Payload format
            return new GoogleJsonWebSignature.Payload
            {
                Email = userInfo.email,
                Name = userInfo.name,
                GivenName = userInfo.given_name,
                FamilyName = userInfo.family_name,
                Picture = userInfo.picture,
                EmailVerified = userInfo.verified_email
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error exchanging authorization code for user info");
            return null;
        }
    }

    // Add these classes for JSON deserialization
    public class GoogleTokenResponse
    {
        public string access_token { get; set; }
        public string token_type { get; set; }
        public int expires_in { get; set; }
        public string refresh_token { get; set; }
        public string id_token { get; set; }
    }

    public class GoogleUserInfo
    {
        public string id { get; set; }
        public string email { get; set; }
        public bool verified_email { get; set; }
        public string name { get; set; }
        public string given_name { get; set; }
        public string family_name { get; set; }
        public string picture { get; set; }
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

