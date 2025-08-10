using Adidas.AdminDashboardMVC.ViewModels.Users;
using Models.People;

namespace Adidas.AdminDashboardMVC.ViewModels.Account
{
    public class UserProfileViewModel
    {
        public string Email { get; set; }
        public string Role { get; set; }
        public DateTime? CreatedAt { get; set; }
        public bool IsActive { get; set; }

        // New fields
        public bool EmailConfirmed { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string PhoneNumber { get; set; }
        public string Gender { get; set; }
        public string PreferredLanguage { get; set; }
    }


}
