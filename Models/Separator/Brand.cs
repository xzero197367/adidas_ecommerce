using System.ComponentModel.DataAnnotations;
using Adidas.Models.Main;

namespace Adidas.Models.Separator;

public class Brand
{
    [Key]
    public int BrandId { get; set; }

    [Required, MaxLength(100)]
    public string Name { get; set; }

    [MaxLength(500)]
    public string LogoUrl { get; set; }

    public string Description { get; set; }

    [Required]
    public bool IsActive { get; set; }

    // Relationships
    public virtual ICollection<Product> Products { get; set; } = new List<Product>();
}