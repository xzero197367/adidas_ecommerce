using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Adidas.Models.Main;
using Models.People;

namespace Adidas.Models.Feature;


public class Wishlist : BaseEntity
{
    [Required]
    public string UserId { get; set; }

    [ForeignKey("UserId")]
    public User User { get; set; }

    [Required]
    public Guid ProductId { get; set; }

    [ForeignKey("ProductId")]
    public Product Product { get; set; }

    [Required]
    public DateTime AddedAt { get; set; } = DateTime.UtcNow;
}