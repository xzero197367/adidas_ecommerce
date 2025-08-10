using System.ComponentModel.DataAnnotations;

namespace Adidas.AdminDashboardMVC.ViewModels.Account
{
    public class EditUserProfileViewModel
    {
        [Required, EmailAddress]
        public string Email { get; set; }

        [Phone(ErrorMessage = "Invalid phone number format.")]
        [RegularExpression(@"^\+?[0-9]{10,15}$", ErrorMessage = "Phone number must be 10-15 digits.")]
        [Display(Name = "Phone Number")]
        public string PhoneNumber { get; set; }

        [DataType(DataType.Date)]
        [CustomDateOfBirthValidation(ErrorMessage = "Date of birth must be between 18 and 100 years old.")]
        [Display(Name = "Date of Birth")]
        public DateTime? DateOfBirth { get; set; }

        public string? PreferredLanguage { get; set; }

        public string Gender { get; set; } // store as string, parse to enum later
    }

}
