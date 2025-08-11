using System.ComponentModel.DataAnnotations;
using Adidas.DTOs.CommonDTOs;
using Models.People;

namespace Adidas.DTOs.Main.ProductDTOs;

public class ProductUpdateDto : BaseUpdateDto
{
    public string? Name { get; set; }
    public string? Description { get; set; }
    public string? ShortDescription { get; set; }
    public decimal? Price { get; set; }
    public decimal? SalePrice { get; set; }
    public Gender? GenderTarget { get; set; }
    public Guid? CategoryId { get; set; }
    public Guid? BrandId { get; set; }
    public int? SortOrder { get; set; }
    public string? MetaTitle { get; set; }
    public string? MetaDescription { get; set; }
    public string? Keywords { get; set; }
    public bool? InStock { get; set; }
   
}