using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Adidas.Application.Contracts.ServicesContracts.Feature
{
    public interface IDiscountService : IGenericService<Discount, DiscountDto, CreateDiscountDto, UpdateDiscountDto>
    {
        Task<DiscountDto?> GetDiscountByCodeAsync(string code);
        Task<IEnumerable<DiscountDto>> GetActiveDiscountsAsync();
        Task<DiscountValidationResultDto> ValidateDiscountAsync(string code, decimal orderAmount);
        Task<decimal> CalculateDiscountAmountAsync(string code, decimal orderAmount);
        Task<bool> ApplyDiscountAsync(string code);
    }
}
