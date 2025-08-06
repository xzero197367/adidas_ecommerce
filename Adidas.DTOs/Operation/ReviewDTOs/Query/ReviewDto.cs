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
        public class ReviewDto : BaseDto
        {
            public Guid Id { get; set; }
            public string Title { get; set; } = string.Empty;
            public string ReviewText { get; set; } = string.Empty;
            public int Rating { get; set; }
            public bool IsVerifiedPurchase { get; set; }
            public bool IsApproved { get; set; }
            public bool IsActive { get; set; }
            public bool IsDeleted { get; set; }

            public Guid ProductId { get; set; }
            public string UserId { get; set; } = string.Empty;

            public string? ProductName { get; set; }
            public string? ProductImageUrl { get; set; }
            public string? UserName { get; set; }
            public string? UserEmail { get; set; }

            public string? RejectionReason { get; set; }
            public DateTime? ApprovedAt { get; set; }
            public string? ApprovedBy { get; set; }
            public DateTime? RejectedAt { get; set; }
            public string? RejectedBy { get; set; }

            public DateTime CreatedAt { get; set; }
            public string? CreatedBy { get; set; }
            public DateTime? ModifiedAt { get; set; }
            public string? ModifiedBy { get; set; }

            public string StatusText => GetStatusText();
            public string TimeAgo => GetTimeAgo();
            public bool CanBeApproved => !IsApproved && IsActive && !IsDeleted;
            public bool CanBeRejected => !IsApproved && IsActive && !IsDeleted;

            private string GetStatusText()
            {
                if (IsDeleted) return "Deleted";
                if (!IsActive) return "Rejected";
                if (IsApproved) return "Approved";
                return "Pending";
            }

            private string GetTimeAgo()
            {
                var timeSpan = DateTime.UtcNow - CreatedAt;
                if (timeSpan.TotalDays >= 365)
                    return $"{(int)(timeSpan.TotalDays / 365)} year{((int)(timeSpan.TotalDays / 365) != 1 ? "s" : "")} ago";
                if (timeSpan.TotalDays >= 30)
                    return $"{(int)(timeSpan.TotalDays / 30)} month{((int)(timeSpan.TotalDays / 30) != 1 ? "s" : "")} ago";
                if (timeSpan.TotalDays >= 1)
                    return $"{(int)timeSpan.TotalDays} day{((int)timeSpan.TotalDays != 1 ? "s" : "")} ago";
                if (timeSpan.TotalHours >= 1)
                    return $"{(int)timeSpan.TotalHours} hour{((int)timeSpan.TotalHours != 1 ? "s" : "")} ago";
                if (timeSpan.TotalMinutes >= 1)
                    return $"{(int)timeSpan.TotalMinutes} minute{((int)timeSpan.TotalMinutes != 1 ? "s" : "")} ago";
                return "Just now";
            }
        }
    }

