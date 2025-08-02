using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Adidas.DTOs.Tracker;

namespace Adidas.Application.Contracts.ServicesContracts.Tracker
{
    public interface IInventoryService
    {
        Task<bool> ReserveStockAsync(Guid variantId, int quantity);
        Task<bool> ReleaseStockAsync(Guid variantId, int quantity);
        Task<bool> UpdateStockAsync(Guid variantId, int newStock);
        Task<IEnumerable<LowStockAlertDto>> GetLowStockAlertsAsync(int threshold = 10);
        Task<InventoryReportDto> GenerateInventoryReportAsync();
        Task LogInventoryChangeAsync(Guid variantId, int oldQuantity, int newQuantity, string changeType, string userId, string? reason = null);
    }
}
