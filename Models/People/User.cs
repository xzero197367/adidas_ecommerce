

using System.ComponentModel.DataAnnotations;
using Adidas.Models.Feature;
using Adidas.Models.Operation;
using Microsoft.AspNetCore.Identity;
using Models.Feature;

namespace Models.People
{


    public enum UserRole
    {
        Customer,
        Admin,
        Employee
    }
    
    public enum Gender
    {
        Male,
        Female,
        Kids
    }


    public enum Language
    {
        arabic, english
    }
    public class User : IdentityUser
    {


        public DateTime? DateOfBirth { get; set; }

        public Gender? Gender { get; set; }

        [MaxLength(20)]
        public string Phone { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        [Required]
        public bool IsActive { get; set; } = true;

        [MaxLength(10)]
        public string PreferredLanguage { get; set; }

        [Required]
        public UserRole Role { get; set; } = UserRole.Customer;

        // Relationships
        public virtual ICollection<Address> Addresses { get; set; } = new List<Address>();
        public virtual ICollection<ShoppingCart> CartItems { get; set; } = new List<ShoppingCart>();
        public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
        public virtual ICollection<Review> Reviews { get; set; } = new List<Review>();
        public virtual ICollection<Wishlist> Wishlists { get; set; } = new List<Wishlist>();
    }

}
