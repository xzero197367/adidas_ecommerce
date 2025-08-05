using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Adidas.DTOs.Common_DTOs;
using Adidas.DTOs.Operation.ReviewDTOs.Query;
using Adidas.DTOs.People.Customer_DTOs;
using Models.People;

namespace Adidas.Application.Contracts.ServicesContracts.People
{
    public interface ICustomerService
    {
        Task<PagedResult<CustomerDto>> GetCustomersAsync(CustomerFilterDto filter);
        Task<CustomerDetailsDto?> GetCustomerByIdAsync(string id);
        Task<bool> UpdateCustomerAsync(string id, UpdateCustomerDto updateDto);
        Task<bool> ToggleCustomerStatusAsync(string id);
        Task<byte[]> ExportCustomersAsync(CustomerFilterDto filter);
        Task<bool> DeleteCustomerAsync(string id);
        
        Task<IEnumerable<Address>> GetAddressesByUserIdAsync(string userId);

        Task<IEnumerable<ReviewDto>> GetReviewsByUserIdAsync(string userId);  

        Task<IEnumerable<Order>> GetOrdersByUserIdAsync(string userId);
    }
}
