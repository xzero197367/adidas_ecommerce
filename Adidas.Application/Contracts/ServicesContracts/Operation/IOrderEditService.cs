using Adidas.DTOs.CommonDTOs;
using Adidas.DTOs.Operation.OrderDTOs;

namespace Adidas.Application.Contracts.ServicesContracts.Operation
{
    public interface IOrderEditService
    {
        //Task<OperationResult<OrderDto>> EditAsync(OrderUpdateDto dto);
        Task<OperationResult<OrderDto>> EditAsync(OrderUpdateDto dto, string changedByUserId);

    }
}
