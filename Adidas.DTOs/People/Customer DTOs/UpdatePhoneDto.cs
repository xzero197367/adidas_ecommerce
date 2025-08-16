using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Adidas.DTOs.People.Customer_DTOs
{
    public class UpdatePhoneDto
    {
        [Required]
        [Phone]
        public string Phone { get; set; }
    }
}
