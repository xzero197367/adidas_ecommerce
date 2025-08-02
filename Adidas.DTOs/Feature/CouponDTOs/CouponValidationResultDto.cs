namespace Adidas.DTOs.Feature.CouponDTOs;

public class CouponValidationResultDto
{
    public bool IsValid { get; set; }
    public string? ErrorMessage { get; set; }
    public string? Code { get; set; }
    public string? Name { get; set; }
    public decimal DiscountValue { get; set; }
    public string? DiscountType { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal OriginalAmount { get; set; }
    public decimal FinalAmount { get; set; }

    public static CouponValidationResultDto Success(string code, string name, decimal discountValue, string discountType, 
        decimal discountAmount, decimal originalAmount, decimal finalAmount)
    {
        return new CouponValidationResultDto
        {
            IsValid = true,
            Code = code,
            Name = name,
            DiscountValue = discountValue,
            DiscountType = discountType,
            DiscountAmount = discountAmount,
            OriginalAmount = originalAmount,
            FinalAmount = finalAmount
        };
    }

    public static CouponValidationResultDto Failure(string errorMessage)
    {
        return new CouponValidationResultDto
        {
            IsValid = false,
            ErrorMessage = errorMessage
        };
    }
}