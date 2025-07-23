using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Adidas.Models.Main;
using Models.People;

namespace Adidas.Models.Operation;

public class Review
{
    [Key]
    public int ReviewId { get; set; }

    [Required]
    public int ProductId { get; set; }

    [ForeignKey("ProductId")]
    public Product Product { get; set; }

    [Required]
    public int UserId { get; set; }

    [ForeignKey("UserId")]
    public User User { get; set; }

    [Required, Range(1, 5)]
    public int Rating { get; set; }

    [MaxLength(200)]
    public string Title { get; set; }

    public string ReviewText { get; set; }

    [Required]
    public bool IsVerifiedPurchase { get; set; }

    [Required]
    public bool IsApproved { get; set; }

    [Required]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }
}