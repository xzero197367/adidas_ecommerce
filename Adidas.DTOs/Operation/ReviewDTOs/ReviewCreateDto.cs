
using System.ComponentModel.DataAnnotations;

namespace Adidas.DTOs.Operation.ReviewDTOs
{
    public class ReviewCreateDto
    {
        //[Required]
        //[Range(1, 5, ErrorMessage = "Rating must be between 1 and 5")]
        //public required int Rating { get; set; }

        //[StringLength(200)]
        //public string? Title { get; set; }

        //[StringLength(2000)]
        //public string? ReviewText { get; set; }

        //[Required]
        //public required Guid ProductId { get; set; }

        //[Required]
        //public required string UserId { get; set; }

        //public bool IsVerifiedPurchase { get; set; } = false;
        [Required]
        [Range(1, 5, ErrorMessage = "Rating must be between 1 and 5")]
        public int Rating { get; set; }

        [Required]
        [StringLength(200, ErrorMessage = "Title cannot exceed 200 characters")]
        public string Title { get; set; } = string.Empty;

        [StringLength(1000, ErrorMessage = "Review text cannot exceed 1000 characters")]
        public string? ReviewText { get; set; }

        [Required]
        public bool IsVerifiedPurchase { get; set; }

        [Required]
        public Guid ProductId { get; set; }

        [Required]
        public string UserId { get; set; } = string.Empty;
    }
}

