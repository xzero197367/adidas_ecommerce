using Adidas.Models.Operation;
using System;
using Adidas.DTOs.Common_DTOs;

namespace Adidas.Application.Contracts.RepositoriesContracts.Operation
{
    public interface IOrderItemRepository : IGenericRepository<OrderItem>
    {
        Task<IEnumerable<OrderItem>> GetItemsByOrderIdAsync(Guid orderId);
        Task<IEnumerable<OrderItem>> GetItemsByVariantIdAsync(Guid variantId);
        Task<decimal> GetTotalSalesForVariantAsync(Guid variantId);
        Task<IEnumerable<OrderItem>> GetBestSellingItemsAsync(int count);
        Task<int> GetTotalQuantitySoldAsync(Guid variantId);
        Task<PagedResultDto<OrderItem>> GetOrderItemsPagedAsync(Guid orderId, int pageNumber, int pageSize);
    }
}
 