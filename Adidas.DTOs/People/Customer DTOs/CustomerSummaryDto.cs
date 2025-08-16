using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Adidas.DTOs.People.Customer_DTOs
{
    public class CustomerSummaryDto
    {
        public string Id { get; set; }
        public string Email { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public DateTime MemberSince { get; set; }
        public int TotalOrders { get; set; }
        public int TotalAddresses { get; set; }
        public int TotalReviews { get; set; }
        public int TotalWishlistItems { get; set; }
        public bool IsActive { get; set; }
        public string FullName => $"{FirstName} {LastName}".Trim();
    }
}
