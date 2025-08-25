using Models.Main;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Adidas.Application.Contracts.RepositoriesContracts.Main
{
    public interface IUserProductViewRepository
    {
        Task AddAsync(UserProductView view);
        Task<bool> ExistsAsync(string userId, Guid productId);
        Task<List<UserProductView>> GetByProductIdAsync(Guid productId);
    }
}
