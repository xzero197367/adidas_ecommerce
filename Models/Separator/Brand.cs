using System.ComponentModel.DataAnnotations;
using Adidas.Models.Main;

namespace Adidas.Models.Separator;

public class Brand : BaseEntity
{
    [Required, MaxLength(100)]
    public string Name { get; set; }

    [MaxLength(500)]
    public string LogoUrl { get; set; }

    public string Description { get; set; }

    // Relationships
    public virtual ICollection<Product> Products { get; set; } = new List<Product>();
}