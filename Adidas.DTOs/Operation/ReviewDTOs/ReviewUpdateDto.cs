using System.ComponentModel.DataAnnotations;
using Adidas.DTOs.CommonDTOs;

namespace Adidas.DTOs.Operation.ReviewDTOs
{
    public class ReviewUpdateDto : BaseUpdateDto
    {
        //[Required]
        //[Range(1, 5, ErrorMessage = "Rating must be between 1 and 5")]
        //public required int Rating { get; set; }

        //[StringLength(200)]
        //public string? Title { get; set; }

        //[StringLength(2000)]
        //public string? ReviewText { get; set; }
        [Range(1, 5, ErrorMessage = "Rating must be between 1 and 5")]
        public int? Rating { get; set; }

        [StringLength(200, ErrorMessage = "Title cannot exceed 200 characters")]
        public string? Title { get; set; }

        [StringLength(1000, ErrorMessage = "Review text cannot exceed 1000 characters")]
        public string? ReviewText { get; set; }

        public bool? IsVerifiedPurchase { get; set; }
        public bool? IsApproved { get; set; }
        public bool? IsActive { get; set; }
    }

}