using System.ComponentModel.DataAnnotations;

namespace Adidas.AdminDashboardMVC.ViewModels.Inventory
{
    public class UpdateStockViewModel
    {
        [Required]
        public Guid VariantId { get; set; }

        [Display(Name = "New Stock Quantity")]
        [Range(0, int.MaxValue, ErrorMessage = "Stock quantity must be a non-negative number")]
        [Required(ErrorMessage = "New stock quantity is required")]
        public int NewStock { get; set; }

        [Display(Name = "Reason for Update")]
        [StringLength(500, ErrorMessage = "Reason cannot exceed 500 characters")]
        public string? Reason { get; set; }

        // Additional properties for display
        public string ProductName { get; set; } = string.Empty;
        public string VariantDetails { get; set; } = string.Empty;
        public int CurrentStock { get; set; }
    }
}
