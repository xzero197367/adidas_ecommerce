using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Adidas.DTOs.Static
{
    public class CustomerInsightsDto
    {
        public int NewCustomers { get; set; }
        public int ReturningCustomers { get; set; }
        public decimal AverageCustomerValue { get; set; }
        public IEnumerable<CustomerSegmentDto> CustomerSegments { get; set; } = new List<CustomerSegmentDto>();
    }
}
