using System.ComponentModel.DataAnnotations;

namespace Adidas.AdminDashboardMVC.ViewModels.Users
{
    public class EditUserViewModel
    {
        public string Id { get; set; }

        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; }

       
        [Phone(ErrorMessage = "Invalid phone number format.")]
        [RegularExpression(@"^\+?[0-9]{10,15}$", ErrorMessage = "Phone number must be 10-15 digits.")]
        [Display(Name = "Phone Number")]
        public string PhoneNumber { get; set; }

        [Required]
        [Display(Name = "Role")]
        public string Role { get; set; }

        [Display(Name = "Is Active")]
        public bool IsActive { get; set; }

        [DataType(DataType.Date)]
        [CustomDateOfBirthValidation(ErrorMessage = "Date of birth must be between 18 and 100 years old.")]
        [Display(Name = "Date of Birth")]
        public DateTime? DateOfBirth { get; set; }

        [Display(Name = "Gender")]
        public GenderCreationStatus? Gender { get; set; }
    }
}
