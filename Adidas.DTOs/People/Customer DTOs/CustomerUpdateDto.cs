
using Models.People;

namespace Adidas.DTOs.People.Customer_DTOs
{
    public class CustomerUpdateDto
    {
        public string? Phone { get; set; }
        public Gender? Gender { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string? PreferredLanguage { get; set; }
    }
}
