using Adidas.DTOs.Common_DTOs;
using Adidas.DTOs.Main.Product_DTOs;
using Adidas.DTOs.Main.ProductDTOs;

namespace Adidas.AdminDashboardMVC.ViewModels.Products
{
    public class ProductManagementViewModel
    {
        public ProductFilterDto Filters { get; set; } = new();
        public PagedResultDto<ProductDto> Products { get; set; } = new();
    }
}
