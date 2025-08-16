using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Models.People;

namespace Adidas.DTOs.People.Customer_DTOs
{
    public class CustomerProfileDto
    {
        public string Id { get; set; }
        public string Email { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Phone { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public Gender? Gender { get; set; }
        public string PreferredLanguage { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public bool IsActive { get; set; }
        public string UserName { get; set; }
        public string PhoneNumber { get; set; }
        public string FullName => $"{FirstName} {LastName}".Trim();
    }
}
