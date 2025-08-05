using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Models.People;
using System.Security.Claims;

namespace Adidas.AdminDashboardMVC.Services
{
    public class CustomClaimsTransformation : IClaimsTransformation
    {
        private readonly UserManager<User> _userManager;

        public CustomClaimsTransformation(UserManager<User> userManager)
        {
            _userManager = userManager;
        }

        public async Task<ClaimsPrincipal> TransformAsync(ClaimsPrincipal principal)
        {
            if (principal.Identity.IsAuthenticated)
            {
                var user = await _userManager.GetUserAsync(principal);
                if (user != null)
                {
                    var identity = (ClaimsIdentity)principal.Identity;

                    // Add custom claims if they don't exist
                    if (!principal.HasClaim("IsActive", user.IsActive.ToString()))
                    {
                        identity.AddClaim(new Claim("IsActive", user.IsActive.ToString()));
                    }

                    if (!principal.HasClaim("UserRole", user.Role.ToString()))
                    {
                        identity.AddClaim(new Claim("UserRole", user.Role.ToString()));
                    }
                }
            }

            return principal;
        }
    }
}