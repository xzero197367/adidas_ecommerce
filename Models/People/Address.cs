using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Adidas.Models;

namespace Models.People;


public class Address : BaseAuditableEntity
{
<<<<<<< HEAD
    [Required]
    public string UserId { get; set; }
=======
    // fields
    public required string AddressType { get; set; }
    public required string StreetAddress { get; set; }
    public required string City { get; set; }
>>>>>>> origin/main

    public string StateProvince { get; set; }

    public required string PostalCode { get; set; }
    public required string Country { get; set; }

    public required bool IsDefault { get; set; }

    // foreign keys
    public required string UserId { get; set; }

    // navigations
    public User User { get; set; }



    
}