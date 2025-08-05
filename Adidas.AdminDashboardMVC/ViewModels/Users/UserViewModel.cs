namespace Adidas.AdminDashboardMVC.ViewModels.Users
{
    public class UserViewModel
    {
        public string Id { get; set; }
        public string Email { get; set; }
        public string UserName { get; set; }
        public string PhoneNumber { get; set; }
        public bool IsActive { get; set; }
        public string Role { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime LastLogin { get; set; }
        public bool EmailConfirmed { get; set; }
    }
}
