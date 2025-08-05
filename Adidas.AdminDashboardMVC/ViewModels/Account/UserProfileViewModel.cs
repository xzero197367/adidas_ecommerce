namespace Adidas.AdminDashboardMVC.ViewModels.Account
{
    public class UserProfileViewModel
    {
        public string Email { get; set; }
        public string Role { get; set; }
        public DateTime? CreatedAt { get; set; }
        public bool IsActive { get; set; }
    }
}
