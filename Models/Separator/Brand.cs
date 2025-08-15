using System.ComponentModel.DataAnnotations;
using Adidas.Models.Main;

namespace Adidas.Models.Separator;

public class Brand : BaseAuditableEntity
{
    public required string Name { get; set; }
    public string? LogoUrl { get; set; }
    public string Description { get; set; }

    // Relationships
    public virtual ICollection<Product> Products { get; set; } 
}