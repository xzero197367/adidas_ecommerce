using Adidas.Models.Main;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Adidas.Application.Contracts.RepositoriesContracts.Main
{
    public interface IProductImageRepository : IGenericRepository<ProductImage>
    {
        Task<IEnumerable<ProductImage>> GetImagesByProductIdAsync(Guid productId);
        Task<IEnumerable<ProductImage>> GetImagesByVariantIdAsync(Guid variantId);
        Task<ProductImage?> GetPrimaryImageAsync(Guid productId);
    }
}
