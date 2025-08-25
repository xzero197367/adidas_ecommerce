using Adidas.Application.Contracts.RepositoriesContracts.Main;
using Adidas.Models.Main;
using Models.Main;
using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Adidas.Infra.Main
{
    public class UserProductViewRepository : IUserProductViewRepository
    {
        private readonly AdidasDbContext _context;
        public UserProductViewRepository(AdidasDbContext context) => _context = context;

        public async Task AddAsync(UserProductView view)
        {
            _context.UserProductViews.Add(view);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> ExistsAsync(string userId, Guid productId)
        {
            
            return await _context.UserProductViews
                                 .AnyAsync(v => v.UserId == userId && v.ProductId == productId);
        }

        public async Task<List<UserProductView>> GetByProductIdAsync(Guid productId) =>
            await _context.UserProductViews
                .Where(v => v.ProductId == productId)
                .ToListAsync();

        Task<IEnumerable<UserProductView>> IUserProductViewRepository.GetByUserIdAsync(string userId)
        {
            throw new NotImplementedException();
        }

        public async Task<List<UserProductView>> GetByUserIdsAsync(IEnumerable<string> userIds)
        {
            return await _context.UserProductViews
                                 .Where(v => userIds.Contains(v.UserId))
                                 .ToListAsync();
        }
    }


}
