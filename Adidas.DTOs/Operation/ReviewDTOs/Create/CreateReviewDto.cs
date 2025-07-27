using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Adidas.DTOs.Operation.ReviewDTOs.Create
{
    public class CreateReviewDto
    {
        [Required]
        [Range(1, 5, ErrorMessage = "Rating must be between 1 and 5")]
        public required int Rating { get; set; }

        [StringLength(200)]
        public string? Title { get; set; }

        [StringLength(2000)]
        public string? ReviewText { get; set; }

        [Required]
        public required Guid ProductId { get; set; }

        [Required]
        public required string UserId { get; set; }

        public bool IsVerifiedPurchase { get; set; } = false;
    }
}
