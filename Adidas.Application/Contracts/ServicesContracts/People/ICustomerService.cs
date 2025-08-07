
using Adidas.DTOs.Common_DTOs;
using Adidas.DTOs.CommonDTOs;
using Adidas.DTOs.Operation.ReviewDTOs.Query;
using Adidas.DTOs.People.Customer_DTOs;
using Models.People;

namespace Adidas.Application.Contracts.ServicesContracts.People
{
    public interface ICustomerService
    {
        Task<OperationResult<PagedResult<CustomerDto>>> GetCustomersAsync(CustomerFilterDto filter);
        Task<OperationResult<CustomerDetailsDto>> GetCustomerByIdAsync(string id);
        Task<OperationResult<bool>> UpdateCustomerAsync(string id, CustomerUpdateDto customerUpdateDto);
        Task<OperationResult<bool>> ToggleCustomerStatusAsync(string id);
        Task<OperationResult<byte[]>> ExportCustomersAsync(CustomerFilterDto filter);
        Task<OperationResult<bool>> DeleteCustomerAsync(string id);
        
        Task<OperationResult<IEnumerable<Address>>> GetAddressesByUserIdAsync(string userId);

        Task<OperationResult<IEnumerable<ReviewDto>>> GetReviewsByUserIdAsync(string userId);  

        Task<OperationResult<IEnumerable<Order>>> GetOrdersByUserIdAsync(string userId);
    }
}
