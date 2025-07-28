using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Adidas.DTOs.Operation.PaymentDTOs.Statistics
{
    public class PaymentStatsDto
    {
        public decimal TotalAmount { get; set; }
        public int TotalCount { get; set; }
        public int SuccessfulCount { get; set; }
        public int FailedCount { get; set; }
        public int PendingCount { get; set; }
        public decimal SuccessRate { get; set; }
    }
}
