// Adidas.Application.Contracts.ServicesContracts.Operation/IOrderFilterService.cs
using Adidas.DTOs.Common_DTOs;
using Adidas.DTOs.Operation.OrderDTOs;
using System.Threading.Tasks;

namespace Adidas.Application.Contracts.ServicesContracts.Operation
{
    public interface IOrderFilterService
    {
        Task<PagedResultDto<OrderDto>> GetFilteredOrdersAsync(
            int pageNumber, int pageSize, ExtendedOrderFilterDto filter);
    }
}