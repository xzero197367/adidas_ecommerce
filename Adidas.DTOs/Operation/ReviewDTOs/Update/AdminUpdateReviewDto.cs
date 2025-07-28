using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Adidas.DTOs.Operation.ReviewDTOs.Update
{
    public class AdminUpdateReviewDto
    {
        [Required]
        public required bool IsApproved { get; set; }

        public string? ModerationNotes { get; set; }
    }
}
