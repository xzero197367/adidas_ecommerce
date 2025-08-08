using Adidas.DTOs.Main.Product_Variant_DTOs;
using Adidas.DTOs.Main.ProductImageDTOs;
using Adidas.Models.Main;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Adidas.Application.Contracts.ServicesContracts.Main
{
    public interface IProductVariantService :IGenericService<ProductVariant, ProductVariantDto, CreateProductVariantDto, UpdateProductVariantDto>
    {

        Task<bool> AddImageAsync(Guid variantId, IFormFile imageFile);
        Task<ProductVariantDto?> GetBySkuAsync(string sku);



    }
}
