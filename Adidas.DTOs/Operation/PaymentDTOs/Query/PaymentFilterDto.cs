using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Adidas.DTOs.Operation.PaymentDTOs.Query
{
    public class PaymentFilterDto
    {
        public string? PaymentMethod { get; set; }
        public string? PaymentStatus { get; set; }
        public decimal? MinAmount { get; set; }
        public decimal? MaxAmount { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public Guid? OrderId { get; set; }
        public string? TransactionId { get; set; }
    }
}
