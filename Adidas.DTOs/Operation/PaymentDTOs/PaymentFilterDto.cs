namespace Adidas.DTOs.Operation.PaymentDTOs;

public class PaymentFilterDto
{
    public string? Status { get; set; }
    public string? PaymentMethod { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public decimal? MinAmount { get; set; }
    public decimal? MaxAmount { get; set; }
    public string? TransactionId { get; set; }
    public Guid? OrderId { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string SortBy { get; set; } = "ProcessedAt";
    public string SortDirection { get; set; } = "desc";
}