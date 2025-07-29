using Microsoft.EntityFrameworkCore;
using Adidas.Models.Main;
using Adidas.Models.Feature;
using System;
using Adidas.Models.Separator;
using Models.People;
using Microsoft.AspNetCore.Identity;

namespace Adidas.Context.Seeds
{
    public static class AdidasDbContextSeed
    {
        public static void Seed(ModelBuilder modelBuilder)
        {
            // ✅ Admin User Seed
            var adminId = "aaaaaaaa-1111-2222-3333-444444444444";
            var hasher = new PasswordHasher<User>();
            var adminUser = new User
            {
                Id = adminId,
                Email = "admin@adidas.com",
                UserName = "admin@adidas.com",
                NormalizedEmail = "ADMIN@ADIDAS.COM",
                NormalizedUserName = "ADMIN@ADIDAS.COM",
                EmailConfirmed = true,
                IsActive = true,
                Phone = "0000000000",
                Role = UserRole.Admin,
                CreatedAt = DateTime.UtcNow,
                SecurityStamp = Guid.NewGuid().ToString()
            };
            adminUser.PasswordHash = hasher.HashPassword(adminUser, "Admin@123");
            modelBuilder.Entity<User>().HasData(adminUser);

            // ✅ Brands
            modelBuilder.Entity<Brand>().HasData(
                new Brand { Id = Guid.Parse("11111111-1111-1111-1111-111111111111"), Description = "Adidas Samba is one of the most iconic Adidas shoe lines.", Name = "Adidas", LogoUrl = "/images/brands/adidas-samba.png", IsActive = true },
                new Brand { Id = Guid.Parse("22222222-2222-2222-2222-222222222222"), Description = "Adidas Samba is one of the most iconic Adidas shoe lines.", Name = "Adidas Samba", LogoUrl = "/images/brands/adidas-samba.png", IsActive = true },
                new Brand { Id = Guid.Parse("33333333-3333-3333-3333-333333333333"), Description = "Adidas Samba is one of the most iconic Adidas shoe lines.", Name = "Adidas Originals", LogoUrl = "/images/brands/adidas-samba.png", IsActive = true }
            );

            // ✅ Categories
            // ✅ Seed Categories
            // ✅ Seed Categories
            modelBuilder.Entity<Category>().HasData(
                new Category
                {
                    Id = Guid.Parse("44444444-4444-4444-4444-444444444444"),
                    Name = "Shoes",
                    Slug = "shoes",
                    IsActive = true,
                    Description = "All types of Adidas shoes for men and women",
                    ImageUrl = "/images/categories/shoes.jpg"
                },
                new Category
                {
                    Id = Guid.Parse("55555555-5555-5555-5555-555555555555"),
                    Name = "Clothes",
                    Slug = "clothes",
                    IsActive = true,
                    Description = "Adidas clothing line including t-shirts, jackets and pants",
                    ImageUrl = "/images/categories/clothes.jpg"
                }
            );



            // ✅ Products
            modelBuilder.Entity<Product>().HasData(
                new Product
                {
                    Id = Guid.Parse("66666666-6666-6666-6666-666666666666"),
                    Name = "Adidas Ultraboost",
                    Description = "High performance running shoes",
                    ShortDescription = "Running Shoes",
                    Price = 120,
                    CategoryId = Guid.Parse("44444444-4444-4444-4444-444444444444"),
                    BrandId = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                    GenderTarget = Gender.Unisex,
                    IsActive = true,
                    Sku = "UB-001",
                    CreatedAt = DateTime.UtcNow
                },
                new Product
                {
                    Id = Guid.Parse("77777777-7777-7777-7777-777777777777"),
                    Name = "Adidas Samba Classic",
                    Description = "Iconic indoor soccer shoes",
                    ShortDescription = "Samba Shoes",
                    Price = 85,
                    CategoryId = Guid.Parse("44444444-4444-4444-4444-444444444444"),
                    BrandId = Guid.Parse("22222222-2222-2222-2222-222222222222"),
                    GenderTarget = Gender.Unisex,
                    IsActive = true,
                    Sku = "SB-001",
                    CreatedAt = DateTime.UtcNow
                }
            );

            // ✅ Variants
            modelBuilder.Entity<ProductVariant>().HasData(
                new ProductVariant
                {
                    Id = Guid.Parse("88888888-8888-8888-8888-888888888888"),
                    ProductId = Guid.Parse("66666666-6666-6666-6666-666666666666"),
                    Sku = "UB-001-BLK-42",
                    Size = "42",
                    Color = "Black",
                    StockQuantity = 50,
                    PriceAdjustment = 0,
                    IsActive = true,
                    ImageUrl = "/images/products/samba-black-42.jpg"
                },
                new ProductVariant
                {
                    Id = Guid.Parse("99999999-9999-9999-9999-999999999999"),
                    ProductId = Guid.Parse("77777777-7777-7777-7777-777777777777"),
                    Sku = "SB-001-WHT-41",
                    Size = "41",
                    Color = "White",
                    StockQuantity = 30,
                    PriceAdjustment = 0,
                    IsActive = true,
                    ImageUrl = "/images/products/samba-black-42.jpg"
                }
            );

            // ✅ Images
            modelBuilder.Entity<ProductImage>().HasData(
                new ProductImage
                {
                    Id = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"),
                    ProductId = Guid.Parse("66666666-6666-6666-6666-666666666666"),
                    ImageUrl = "/images/products/ultraboost-black.jpg",
                    IsPrimary = true
                },
                new ProductImage
                {
                    Id = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"),
                    ProductId = Guid.Parse("77777777-7777-7777-7777-777777777777"),
                    ImageUrl = "/images/products/samba-white.jpg",
                    IsPrimary = true
                }
            );
        }
    }
}
