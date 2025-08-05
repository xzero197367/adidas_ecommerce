using System.ComponentModel.DataAnnotations;

namespace Adidas.DTOs.Feature.WishLIstDTOS;

public class WishlistCreateDto
{
    [Required]
    public string UserId { get; set; }
    [Required]
    public Guid ProductId { get; set; }
}