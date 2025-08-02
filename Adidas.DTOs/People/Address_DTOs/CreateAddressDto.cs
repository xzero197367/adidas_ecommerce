using System.ComponentModel.DataAnnotations;

namespace Adidas.DTOs.People.Address_DTOs;

public class CreateAddressDto
{
    [Required] public Guid UserId { get; set; }

    [Required] public string FirstName { get; set; }

    [Required] public string LastName { get; set; }

    public string Company { get; set; }

    [Required] public string Street { get; set; }

    public string Street2 { get; set; }

    [Required] public string City { get; set; }

    [Required] public string State { get; set; }

    [Required] public string PostalCode { get; set; }

    [Required] public string Country { get; set; }

    [Phone] public string PhoneNumber { get; set; }

    public bool IsDefault { get; set; }
    public string AddressType { get; set; } = "";
}