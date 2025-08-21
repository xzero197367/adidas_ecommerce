using Adidas.DTOs.CommonDTOs;

namespace Adidas.DTOs.People.Address_DTOs;

public class AddressUpdateDto: BaseUpdateDto
{ 
    public string FirstName { get; set; }
    public string? UserId { get; set; }
    public string LastName { get; set; }
    public string Company { get; set; }
    public string Street { get; set; }
    public string Street2 { get; set; }
    public string City { get; set; }
    public string State { get; set; }
    public string PostalCode { get; set; }
    public string Country { get; set; }
    public string PhoneNumber { get; set; }
    public bool? IsDefault { get; set; }
    public string? AddressType { get; set; }
}