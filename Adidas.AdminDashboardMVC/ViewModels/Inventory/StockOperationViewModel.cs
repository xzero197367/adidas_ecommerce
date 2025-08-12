using System.ComponentModel.DataAnnotations;

namespace Adidas.AdminDashboardMVC.ViewModels.Inventory
{
    public class StockOperationViewModel
    {
        [Required]
        public Guid VariantId { get; set; }

        [Display(Name = "Quantity")]
        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be greater than 0")]
        [Required(ErrorMessage = "Quantity is required")]
        public int Quantity { get; set; }

        [Display(Name = "Operation Type")]
        [Required(ErrorMessage = "Operation type is required")]
        public string OperationType { get; set; } = string.Empty; // RESERVE or RELEASE

        [Display(Name = "Reason")]
        [StringLength(500, ErrorMessage = "Reason cannot exceed 500 characters")]
        public string? Reason { get; set; }
    }
}
