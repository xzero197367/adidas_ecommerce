using Adidas.Application.Contracts.RepositoriesContracts.Feature;
using Adidas.Application.Services.Feature;
using Adidas.Models.Feature;
using Models.Feature;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Adidas.Application.Services.Feature
{
    
public class OrderCouponService : IDiscountService
{
    private readonly IDiscountRepository _discountRepository;
    private readonly IOrderCouponRepository _orderCouponRepository;
    private readonly IMapper _mapper;

    public OrderCouponService(IDiscountRepository discountRepo, IOrderCouponRepository orderCouponRepo, IMapper mapper)
    {
        _discountRepository = discountRepo;
        _orderCouponRepository = orderCouponRepo;
        _mapper = mapper;
    }

    public async Task<DiscountDto?> GetDiscountByCodeAsync(string code)
    {
        var discount = await _discountRepository.GetDiscountByCodeAsync(code);
        return discount is null ? null : _mapper.Map<DiscountDto>(discount);
    }

    public async Task<IEnumerable<DiscountDto>> GetActiveDiscountsAsync()
    {
        var active = await _discountRepository.GetActiveDiscountsAsync();
        return active.Select(d => _mapper.Map<DiscountDto>(d));
    }

    public async Task<IEnumerable<DiscountDto>> GetDiscountsByUserAsync(Guid userId)
    {
        // Business rule required: depends on order history or assigned user discounts
        throw new NotImplementedException();
    }

    public async Task<IEnumerable<DiscountDto>> GetApplicableDiscountsAsync(Guid userId, decimal orderAmount)
    {
        var discounts = await _discountRepository.GetActiveDiscountsAsync();
        return discounts
            .Where(d => (d.MinOrderAmount ?? 0) <= orderAmount)
            .Select(d => _mapper.Map<DiscountDto>(d));
    }

    public async Task<DiscountValidationDto> ValidateDiscountAsync(ValidateDiscountDto validateDto)
    {
        var discount = await _discountRepository.GetDiscountByCodeAsync(validateDto.Code);
        var errors = new List<string>();

        if (discount == null)
        {
            return new DiscountValidationDto
            {
                IsValid = false,
                ErrorMessage = "Invalid discount code",
                ValidationErrors = new[] { "Code not found" }
            };
        }

        if (!discount.IsActive || DateTime.UtcNow < discount.StartDate || DateTime.UtcNow > discount.EndDate)
            errors.Add("Discount not active");

        if (discount.MinOrderAmount.HasValue && validateDto.OrderAmount < discount.MinOrderAmount.Value)
            errors.Add("Minimum order amount not met");

        return new DiscountValidationDto
        {
            IsValid = errors.Count == 0,
            ErrorMessage = errors.Count > 0 ? "Validation failed" : null,
            ValidationErrors = errors,
            Discount = _mapper.Map<DiscountDto>(discount),
            ApplicableAmount = validateDto.OrderAmount
        };
    }

    public async Task<DiscountResultDto> CalculateDiscountAmountAsync(CalculateDiscountDto calcDto)
    {
        var discount = await _discountRepository.GetDiscountByCodeAsync(calcDto.Code);
        if (discount == null) throw new Exception("Invalid discount");

        decimal discountAmount = discount.DiscountType switch
        {
            DiscountType.Percentage => calcDto.OrderAmount * (discount.Value / 100),
            DiscountType.FixedAmount => discount.Value,
            _ => 0
        };

        if (discount.MaxDiscountAmount.HasValue)
            discountAmount = Math.Min(discountAmount, discount.MaxDiscountAmount.Value);

        return new DiscountResultDto
        {
            DiscountAmount = discountAmount,
            FinalAmount = calcDto.OrderAmount - discountAmount,
            DiscountCode = discount.Code,
            DiscountType = discount.DiscountType,
            OriginalAmount = calcDto.OrderAmount,
            SavingsPercentage = (discountAmount / calcDto.OrderAmount) * 100
        };
    }

    public async Task<bool> ApplyDiscountAsync(Guid orderId, string discountCode)
    {
        var discount = await _discountRepository.GetDiscountByCodeAsync(discountCode);
        if (discount == null) return false;

        var orderCoupon = new OrderCoupon
        {
            OrderId = orderId,
            CouponId = discount.Id,
            DiscountValue = discount.Value,
            AppliedAt = DateTime.UtcNow
        };

        await _orderCouponRepository.AddAsync(orderCoupon);
        return true;
    }

    public Task<bool> IncrementDiscountUsageAsync(Guid discountId)
    {
        // Usually involves updating usage count
        throw new NotImplementedException();
    }

    public Task<bool> DecrementDiscountUsageAsync(Guid discountId)
    {
        // Rollback case
        throw new NotImplementedException();
    }
}
