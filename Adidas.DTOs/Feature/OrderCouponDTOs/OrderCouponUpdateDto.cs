using System.ComponentModel.DataAnnotations;

namespace Adidas.DTOs.Feature.OrderCouponDTOs;

public class OrderCouponUpdateDto
{
    public Guid Id { get; set; }
    // fields
    public decimal? DiscountApplied { get; set; }
}