using System.ComponentModel.DataAnnotations;

namespace Adidas.DTOs.Feature.OrderCouponDTOs;

public class OrderCouponCreateDto
{
    // fields
    [Required]
    public decimal DiscountApplied { get; set; }
    
    // foreign key
    [Required]
    public Guid CouponId { get; set; }
    [Required]
    public Guid OrderId { get; set; }
}