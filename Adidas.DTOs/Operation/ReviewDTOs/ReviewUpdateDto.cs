
using System.ComponentModel.DataAnnotations;

namespace Adidas.DTOs.Operation.ReviewDTOs
{
    public class ReviewUpdateDto
    {
        [Required]
        [Range(1, 5, ErrorMessage = "Rating must be between 1 and 5")]
        public required int Rating { get; set; }

        [StringLength(200)]
        public string? Title { get; set; }

        [StringLength(2000)]
        public string? ReviewText { get; set; }
    }

}
