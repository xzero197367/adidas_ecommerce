using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Adidas.Models.Main;
using Models.People;

namespace Adidas.Models.Operation;

public class Review : BaseAuditableEntity
{
    // fields
    public required int Rating { get; set; }
    public string Title { get; set; }
    public string ReviewText { get; set; }
    public required bool IsVerifiedPurchase { get; set; }
    public required bool IsApproved { get; set; }
    // foreign keys
    public required Guid ProductId { get; set; }
    public required string UserId { get; set; }
    // navigations
    public Product Product { get; set; }
    public User User { get; set; }
}