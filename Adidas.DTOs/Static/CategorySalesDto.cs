using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Adidas.DTOs.Static
{
    public class CategorySalesDto
    {
        public string CategoryName { get; set; } = string.Empty;
        public decimal Sales { get; set; }
        public int ProductsSold { get; set; }
    }
}
