using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Adidas.Application.Contracts.RepositoriesContracts.Tracker;
using Adidas.Models.Tracker;
using Microsoft.EntityFrameworkCore;

namespace Adidas.Infra.Tracker
{
    public class InventoryLogRepository : GenericRepository<InventoryLog>, IInventoryLogRepository
    {
        public InventoryLogRepository(AdidasDbContext context) : base(context) { }

        public async Task<IEnumerable<InventoryLog>> GetLogsByVariantIdAsync(Guid variantId)
        {
            return await _dbSet
                .Include(l => l.AddedBy)
                .Where(l => !l.IsDeleted && l.VariantId == variantId)
                .OrderByDescending(l => l.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<InventoryLog>> GetLogsByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            return await _dbSet
                .Include(l => l.Variant)
                .ThenInclude(v => v.Product)
                .Include(l => l.AddedBy)
                .Where(l => !l.IsDeleted && l.CreatedAt >= startDate && l.CreatedAt <= endDate)
                .OrderByDescending(l => l.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<InventoryLog>> GetLogsByChangeTypeAsync(string changeType)
        {
            return await _dbSet
                .Include(l => l.Variant)
                .ThenInclude(v => v.Product)
                .Where(l => !l.IsDeleted && l.ChangeType == changeType)
                .OrderByDescending(l => l.CreatedAt)
                .ToListAsync();
        }
    }
}
