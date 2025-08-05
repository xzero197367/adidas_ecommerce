using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Models.People;

namespace Adidas.DTOs.People.Customer_DTOs
{
    public class UpdateCustomerDto
    {
        public string? Phone { get; set; }
        public Gender? Gender { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string? PreferredLanguage { get; set; }
    }
}
