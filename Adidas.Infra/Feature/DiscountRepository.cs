using Adidas.Application.Contracts.RepositoriesContracts.Feature;
using Adidas.Context;
using Models.Feature;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Adidas.Infra.Feature
{
    public class DiscountRepository : GenericRepository<Discount>, IDiscountRepository
    {
        public DiscountRepository(AdidasDbContext context) : base(context)
        {
        }
        public async Task<Discount> GetDiscountByCodeAsync(string code)
        {
            return await _dbSet
                .FirstOrDefaultAsync(d => d.Code == code && !d.IsDeleted);
        }

        public async Task<IEnumerable<Discount>> GetActiveDiscountsAsync()
        {
            var currentDate = DateTime.UtcNow;

            return await _dbSet
                .Where(d =>
                    d.ValidFrom <= currentDate &&
                    d.ValidTo >= currentDate &&
                    !d.IsDeleted)
                .OrderByDescending(d => d.ValidTo)
                .ToListAsync();
        }

        public async Task<bool> IsDiscountValidAsync(string code)
        {
            var currentDate = DateTime.UtcNow;

            return await _dbSet.AnyAsync(d =>
                d.Code == code &&
                d.ValidFrom <= currentDate &&
                d.ValidTo >= currentDate &&
                !d.IsDeleted);
        }

        public async Task<bool> CanUseDiscountAsync(string code)
        {
            var discount = await _dbSet.FirstOrDefaultAsync(d => d.Code == code && !d.IsDeleted);

            if (discount == null)
                return false;

             var now = DateTime.UtcNow;
            if (discount.ValidFrom > now || discount.ValidTo < now)
                return false;

             if (discount.UsageLimit > 0 && discount.UsedCount >= discount.UsageLimit)
                return false;

            return true;
        }
        public async Task<bool> UseDiscountAsync(string code)
        {
            var discount = await GetDiscountByCodeAsync(code);
            if (discount == null || !await CanUseDiscountAsync(code))
                return false;

            discount.UsedCount += 1;
            _context.Update(discount);
            //await _context.SaveChangesAsync();
            return true;
        }
    }
}
 