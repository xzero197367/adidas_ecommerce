using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Adidas.Models.Main;
using Models.People;

namespace Adidas.Models.Feature;


public class Wishlist : BaseAuditableEntity
{
    // fields
    public required DateTime AddedAt { get; set; } = DateTime.UtcNow;
    
    // foreign keys
    public required string UserId { get; set; }
    public required Guid ProductId { get; set; }
    // navigation properties
    public User User { get; set; }
    public Product Product { get; set; }
}