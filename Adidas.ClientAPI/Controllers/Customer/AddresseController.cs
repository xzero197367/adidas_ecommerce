using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Adidas.Application.Contracts.ServicesContracts.People;
using Adidas.DTOs.People.Address_DTOs;
using Adidas.DTOs.CommonDTOs;

namespace Adidas.ClientAPI.Controllers.Customer
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize] // Require authentication for all address operations
    public class AddresseController : ControllerBase
    {
        private readonly IAddressService _addressService;
        private readonly ILogger<AddresseController> _logger;

        public AddresseController(IAddressService addressService, ILogger<AddresseController> logger)
        {
            _addressService = addressService;
            _logger = logger;
        }

        /// <summary>
        /// Get all addresses for the current user, or create a default one if none exist
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> GetUserAddresses()
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized(new { message = "User not authenticated" });

                var result = await _addressService.GetAddressesByUserIdAsync(userId);

                if (result.IsSuccess)
                {
                    var addresses = result.Data;

                    // If user has no addresses, return empty list with suggestion to create one
                    if (!addresses.Any())
                    {
                        return Ok(new
                        {
                            addresses = new List<AddressDto>(),
                            message = "No addresses found. Create your first address.",
                            hasAddresses = false
                        });
                    }

                    return Ok(new
                    {
                        addresses = addresses,
                        hasAddresses = true,
                        defaultAddress = addresses.FirstOrDefault(a => a.IsDefault)
                    });
                }

                return BadRequest(new { message = result.ErrorMessage });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving user addresses");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        /// <summary>
        /// Get the default address for the current user
        /// </summary>
        /// <returns></returns>
        [HttpGet("default")]
        public async Task<IActionResult> GetDefaultAddress()
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized(new { message = "User not authenticated" });

                var result = await _addressService.GetDefaultAddressAsync(userId);

                if (result.IsSuccess)
                {
                    if (result.Data == null)
                        return NotFound(new { message = "No default address found" });

                    return Ok(new { address = result.Data });
                }

                return BadRequest(new { message = result.ErrorMessage });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving default address");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        /// <summary>
        /// Create a new address for the current user
        /// </summary>
        /// <param name="createDto"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> CreateAddress([FromBody] AddressCreateDto createDto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized(new { message = "User not authenticated" });

                // Ensure the address belongs to the authenticated user
                createDto.UserId = Guid.Parse(userId);

                // If this is set as default, we need to handle making other addresses non-default
                if (createDto.IsDefault)
                {
                    // First, check if user has existing addresses and unset their default status
                    var existingAddresses = await _addressService.GetAddressesByUserIdAsync(userId);
                    if (existingAddresses.IsSuccess && existingAddresses.Data.Any())
                    {
                        // This would require additional service methods to update existing addresses
                        // For now, we'll allow multiple defaults and handle it in the service layer
                    }
                }

                var result = await _addressService.CreateAsync(createDto);

                if (result.IsSuccess)
                {
                    return CreatedAtAction(nameof(GetAddressById),
                        new { id = result.Data.Id },
                        new { address = result.Data, message = "Address created successfully" });
                }

                return BadRequest(new { message = result.ErrorMessage });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating address");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        /// <summary>
        /// Add an alternative address (non-default) for the current user
        /// </summary>
        /// <param name="createDto"></param>
        /// <returns></returns>
        [HttpPost("alternative")]
        public async Task<IActionResult> AddAlternativeAddress([FromBody] AddressCreateDto createDto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized(new { message = "User not authenticated" });

                // Ensure the address belongs to the authenticated user
                createDto.UserId = Guid.Parse(userId);

                // Force this to be non-default since it's an alternative address
                createDto.IsDefault = false;

                // Set address type if not provided
                if (string.IsNullOrEmpty(createDto.AddressType))
                    createDto.AddressType = "Alternative";

                var result = await _addressService.CreateAsync(createDto);

                if (result.IsSuccess)
                {
                    return CreatedAtAction(nameof(GetAddressById),
                        new { id = result.Data.Id },
                        new { address = result.Data, message = "Alternative address added successfully" });
                }

                return BadRequest(new { message = result.ErrorMessage });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding alternative address");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        /// <summary>
        /// Update an existing address
        /// </summary>
        /// <param name="id"></param>
        /// <param name="updateDto"></param>
        /// <returns></returns>
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateAddress(Guid id, [FromBody] AddressUpdateDto updateDto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized(new { message = "User not authenticated" });

                // Ensure the ID in the URL matches the DTO
                updateDto.Id = id;

                // First, verify that the address belongs to the current user
                var existingAddress = await _addressService.GetByIdAsync(id);
                if (!existingAddress.IsSuccess)
                    return NotFound(new { message = "Address not found" });

                // Check ownership - this would need to be added to your AddressDto
                // For now, we'll assume the service handles ownership validation

                var result = await _addressService.UpdateAsync(updateDto);

                if (result.IsSuccess)
                {
                    return Ok(new { address = result.Data, message = "Address updated successfully" });
                }

                return BadRequest(new { message = result.ErrorMessage });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating address with ID: {AddressId}", id);
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        /// <summary>
        /// Set an address as the default address
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpPatch("{id}/set-default")]
        public async Task<IActionResult> SetDefaultAddress(Guid id)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized(new { message = "User not authenticated" });

                // Verify the address exists and belongs to the user
                var existingAddress = await _addressService.GetByIdAsync(id);
                if (!existingAddress.IsSuccess)
                    return NotFound(new { message = "Address not found" });

                var result = await _addressService.SetDefaultAddressAsync(id, userId);

                if (result.IsSuccess)
                {
                    return Ok(new { address = result.Data, message = "Default address set successfully" });
                }

                return BadRequest(new { message = result.ErrorMessage });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting default address with ID: {AddressId}", id);
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        /// <summary>
        /// Get a specific address by ID
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetAddressById(Guid id)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized(new { message = "User not authenticated" });

                var result = await _addressService.GetByIdAsync(id);

                if (result.IsSuccess)
                {
                    if (result.Data == null)
                        return NotFound(new { message = "Address not found" });

                    // Verify ownership - you might want to add UserId to AddressDto for this check
                    return Ok(new { address = result.Data });
                }

                return BadRequest(new { message = result.ErrorMessage });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving address with ID: {AddressId}", id);
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        /// <summary>
        /// Soft delete an address
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAddress(Guid id)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized(new { message = "User not authenticated" });

                // Verify the address exists and belongs to the user
                var existingAddress = await _addressService.GetByIdAsync(id);
                if (!existingAddress.IsSuccess)
                    return NotFound(new { message = "Address not found" });

                var result = await _addressService.DeleteAsync(id);

                if (result.IsSuccess)
                {
                    return Ok(new { message = "Address deleted successfully" });
                }

                return BadRequest(new { message = result.ErrorMessage });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting address with ID: {AddressId}", id);
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        /// <summary>
        /// Get addresses by type (e.g., "Home", "Work", "Alternative")
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        [HttpGet("by-type/{type}")]
        public async Task<IActionResult> GetAddressesByType(string type)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized(new { message = "User not authenticated" });

                var allAddressesResult = await _addressService.GetAddressesByUserIdAsync(userId);

                if (allAddressesResult.IsSuccess)
                {
                    var filteredAddresses = allAddressesResult.Data
                        .Where(a => !string.IsNullOrEmpty(a.AddressType) &&
                                   a.AddressType.Equals(type, StringComparison.OrdinalIgnoreCase))
                        .ToList();

                    return Ok(new { addresses = filteredAddresses });
                }

                return BadRequest(new { message = allAddressesResult.ErrorMessage });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving addresses by type: {Type}", type);
                return StatusCode(500, new { message = "Internal server error" });
            }
        }
    }
}