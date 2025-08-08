using Adidas.DTOs.Common_DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

//namespace Adidas.DTOs.Operation.ReviewDTOs.Query
//{
    //public class ReviewDto
    //{
    //    public Guid Id { get; set; }
    //    public int Rating { get; set; }
    //    public string? Title { get; set; }
    //    public string? ReviewText { get; set; }
    //    public bool IsVerifiedPurchase { get; set; }
    //    public bool IsApproved { get; set; }
    //    public Guid ProductId { get; set; }
    //    public string UserId { get; set; }
    //    public DateTime CreatedAt { get; set; }
    //    public DateTime? ModifiedAt { get; set; }
    //    public string? CreatedBy { get; set; }
    //    public string? ModifiedBy { get; set; }
    //}
    //}
    namespace Adidas.DTOs.Operation.ReviewDTOs.Query
    {

    public class ReviewDto
    {
        public Guid Id { get; set; }
        public int Rating { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? ReviewText { get; set; }
        public bool IsVerifiedPurchase { get; set; }
        public bool IsApproved { get; set; }
        public bool IsActive { get; set; } = true;
        public bool IsDeleted { get; set; } = false;
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        // ✅ Add these if needed for the UI:
        public string? CreatedBy { get; set; }     // Optional
        public DateTime? ModifiedAt { get; set; }  // Optional
        public string? ModifiedBy { get; set; }    // Optional


        // Foreign keys
        public Guid ProductId { get; set; }
        public string UserId { get; set; } = string.Empty;

        // Navigation properties (optional for display)
        public string? ProductName { get; set; }
        public string? UserEmail { get; set; }
    }
}

