using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Adidas.DTOs.Operation.PaymentDTOs.Statistics
{
    public class PaymentStatsDto
    {
        public int TotalPayments { get; set; }
        public decimal TotalAmount { get; set; }
        public int SuccessfulPayments { get; set; }
        public int FailedPayments { get; set; }
        public int PendingPayments { get; set; }
        public decimal AverageAmount { get; set; }
        public int RefundedPayments { get; set; }
        public decimal TotalRefundAmount { get; set; }

        // Payment method breakdown
        public Dictionary<string, int> PaymentMethodBreakdown { get; set; } = new();
        public Dictionary<string, decimal> PaymentMethodAmounts { get; set; } = new();
    }
}
