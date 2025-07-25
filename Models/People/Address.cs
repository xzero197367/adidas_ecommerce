using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Adidas.Models;

namespace Models.People;


public class Address : BaseAuditableEntity
{
    [Required]
    public Guid UserId { get; set; }

    [ForeignKey("UserId")]
    public User User { get; set; }

    [Required, MaxLength(50)]
    public string AddressType { get; set; }

    [Required, MaxLength(200)]
    public string StreetAddress { get; set; }

    [Required, MaxLength(100)]
    public string City { get; set; }

    [MaxLength(100)]
    public string StateProvince { get; set; }

    [Required, MaxLength(20)]
    public string PostalCode { get; set; }

    [Required, MaxLength(100)]
    public string Country { get; set; }

    [Required]
    public bool IsDefault { get; set; }
}