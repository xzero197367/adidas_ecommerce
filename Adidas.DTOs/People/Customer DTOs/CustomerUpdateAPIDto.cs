using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Models.People;

namespace Adidas.DTOs.People.Customer_DTOs
{
    public class CustomerUpdateAPIDto
    {
        [MaxLength(50)]
        public string FirstName { get; set; }

        [MaxLength(50)]
        public string LastName { get; set; }

        [EmailAddress]
        public string Email { get; set; }

        [Phone]
        public string Phone { get; set; }

        public DateTime? DateOfBirth { get; set; }
        public Gender? Gender { get; set; }

        [MaxLength(10)]
        public string PreferredLanguage { get; set; }
    }
}
