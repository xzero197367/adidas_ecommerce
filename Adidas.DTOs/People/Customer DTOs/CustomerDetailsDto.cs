using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Adidas.DTOs.Operation.ReviewDTOs.Query;
using Adidas.Models.Operation;
using Models.People;

namespace Adidas.DTOs.People.Customer_DTOs
{
    public class CustomerDetailsDto
    {
        // ✅ Basic Customer Info
        public string Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public Gender? Gender { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string PreferredLanguage { get; set; }
        public bool IsActive { get; set; }

        // ✅ Audit Info
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        // ✅ Stats
        public int TotalOrders { get; set; }
        public decimal TotalSpent { get; set; }

        // ✅ New: Related Data
        public List<Address> Addresses { get; set; } = new List<Address>();
        public List<Order> Orders { get; set; } = new List<Order>();
        public List<ReviewDto> Reviews { get; set; } = new List<ReviewDto>();
    }
}
