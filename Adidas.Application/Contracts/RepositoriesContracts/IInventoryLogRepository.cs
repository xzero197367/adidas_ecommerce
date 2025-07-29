using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Adidas.Models.Tracker;

namespace Adidas.Application.Contracts.RepositoriesContracts
{
    public interface IInventoryLogRepository : IGenericRepository<InventoryLog>
    {
        Task<IEnumerable<InventoryLog>> GetLogsByVariantIdAsync(Guid variantId);
        Task<IEnumerable<InventoryLog>> GetLogsByDateRangeAsync(DateTime startDate, DateTime endDate);
        Task<IEnumerable<InventoryLog>> GetLogsByChangeTypeAsync(string changeType);
    }
}
