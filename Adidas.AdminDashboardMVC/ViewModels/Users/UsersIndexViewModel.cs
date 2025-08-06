namespace Adidas.AdminDashboardMVC.ViewModels.Users
{
    public class UsersIndexViewModel
    {
        public List<UserViewModel> Users { get; set; } = new();
        public string SearchTerm { get; set; } = "";
        public string RoleFilter { get; set; } = "All";
        public int CurrentPage { get; set; }
        public int PageSize { get; set; }
        public int TotalUsers { get; set; }
        public int TotalPages { get; set; }
        public int TotalActiveUsers { get; set; }
        public int AdminRolesCount { get; set; }
        public int PendingApproval { get; set; }
    }
}
