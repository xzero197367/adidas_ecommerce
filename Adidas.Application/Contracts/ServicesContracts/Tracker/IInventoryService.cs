
using Adidas.DTOs.CommonDTOs;
using Adidas.DTOs.Tracker;
using Adidas.Models.Tracker;

namespace Adidas.Application.Contracts.ServicesContracts.Tracker
{
    public interface IInventoryService
    {
        Task<OperationResult<bool>> ReserveStockAsync(Guid variantId, int quantity); 
        Task<OperationResult<bool>> ReleaseStockAsync(Guid variantId, int quantity);
        Task<OperationResult<bool>> UpdateStockAsync(Guid variantId, int newStock);
        Task<OperationResult<IEnumerable<LowStockAlertDto>>> GetLowStockAlertsAsync(int threshold = 10);
        Task<OperationResult<InventoryReportDto>> GenerateInventoryReportAsync();
        Task LogInventoryChangeAsync(Guid variantId, int oldQuantity, int newQuantity, string changeType, string userId, string? reason = null);
        Task<OperationResult<bool>> HasSufficientStockAsync(Guid variantId, int quantity);
    }
}
