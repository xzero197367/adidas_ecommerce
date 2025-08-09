using Adidas.DTOs.CommonDTOs;

namespace Adidas.DTOs.Feature.WishLIstDTOS;

public class WishlistUpdateDto: BaseUpdateDto
{
    public Guid? ProductId { get; set; }
}