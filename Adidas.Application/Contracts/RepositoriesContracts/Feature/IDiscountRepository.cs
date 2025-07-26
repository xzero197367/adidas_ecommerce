using Models.Feature;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Adidas.Application.Contracts.RepositoriesContracts.Feature
{
    public interface IDiscountRepository : IGenericRepository<Discount>
    {
        Task<Discount> GetDiscountByCodeAsync(string code);
        Task<IEnumerable<Discount>> GetActiveDiscountsAsync();
        Task<bool> IsDiscountValidAsync(string code);
        Task<bool> CanUseDiscountAsync(string code);
        Task<bool> UseDiscountAsync(string code);
      
    }
}
