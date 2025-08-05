
using System.ComponentModel.DataAnnotations;
using Models.Feature;

namespace Adidas.DTOs.Feature.CouponDTOs;

public class CouponCreateDto
{
    [Required]
    public string Code { get; set; }
    [Required]
    public string Name { get; set; }
    [Required]
    public DiscountType DiscountType { get; set; }
    [Required]
    public decimal DiscountValue { get; set; }
    public decimal MinimumAmount { get; set; }
    [Required]
    public DateTime ValidFrom { get; set; }
    [Required]
    public DateTime ValidTo { get; set; }
    public int UsageLimit { get; set; }
}