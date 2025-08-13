
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Models.People;
using Adidas.Models.Main;
using Adidas.Models.Separator;
using Adidas.Models.Feature;
using Adidas.Models.Operation;
using Adidas.Models.Tracker;
using Models.Feature;

namespace Adidas.Context.Seeds
{
    public static class SeedData2
    {
        public static void Seed(ModelBuilder modelBuilder)
        {
            // Admin Role
            var adminRoleId = Guid.NewGuid().ToString();
            modelBuilder.Entity<IdentityRole>().HasData(new IdentityRole
            {
                Id = adminRoleId,
                Name = "Admin",
                NormalizedName = "ADMIN"
            });

            // Admin User
            var adminUserId = Guid.NewGuid().ToString();
            var hasher = new PasswordHasher<User>();
            var adminUser = new User
            {
                Id = adminUserId,
                Email = "admin@adidas.com",
                NormalizedEmail = "ADMIN@ADIDAS.COM",
                UserName = "admin@adidas.com",
                NormalizedUserName = "ADMIN@ADIDAS.COM",
                EmailConfirmed = true,
                Phone = "0000000000",
                Role = UserRole.Admin,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                IsActive = true,
                IsDeleted = false,
                PreferredLanguage = "english",
                SecurityStamp = Guid.NewGuid().ToString()
            };
            adminUser.PasswordHash = hasher.HashPassword(adminUser, "Admin@123");
            modelBuilder.Entity<User>().HasData(adminUser);

            // Link Admin User to Admin Role
            modelBuilder.Entity<IdentityUserRole<string>>().HasData(new IdentityUserRole<string>
            {
                UserId = adminUserId,
                RoleId = adminRoleId
            });

            // Users (50 records)
            var users = new List<User>();
            var firstNames = new[] { "Ahmed", "Mohamed", "Ali", "Omar", "Hassan", "Mahmoud", "Khaled", "Amr", "Tamer", "Youssef",
                                   "Fatma", "Aisha", "Mariam", "Nour", "Sarah", "Dina", "Rana", "Heba", "Yasmin", "Lina" };
            var lastNames = new[] { "Mohamed", "Ali", "Hassan", "Ahmed", "Mahmoud", "Ibrahim", "Mostafa", "Abdel Rahman", "El Sayed", "Farouk" };
            for (int i = 1; i <= 50; i++)
            {
                var firstName = firstNames[(i - 1) % firstNames.Length];
                var lastName = lastNames[(i - 1) % lastNames.Length];
                var user = new User
                {
                    Id = Guid.NewGuid().ToString(),
                    Email = $"{firstName.ToLower()}.{lastName.ToLower()}{i}@example.com",
                    NormalizedEmail = $"{firstName.ToUpper()}.{lastName.ToUpper()}{i}@EXAMPLE.COM",
                    UserName = $"{firstName.ToLower()}{lastName.ToLower()}{i}",
                    NormalizedUserName = $"{firstName.ToUpper()}{lastName.ToUpper()}{i}",
                    EmailConfirmed = true,
                    FirstName = firstName,
                    LastName = lastName,
                    Phone = $"+2010{1000000 + i:D7}",
                    DateOfBirth = DateTime.UtcNow.AddYears(-25).AddDays(i * 10),
                    Gender = i % 4 == 1 ? Gender.Male : i % 4 == 2 ? Gender.Female : i % 4 == 3 ? Gender.Kids : Gender.Unisex,
                    CreatedAt = DateTime.UtcNow.AddDays(-i * 2),
                    UpdatedAt = DateTime.UtcNow.AddDays(-i * 2),
                    IsActive = true,
                    IsDeleted = false,
                    PreferredLanguage = i % 2 == 0 ? "english" : "arabic",
                    Role = i <= 3 ? UserRole.Admin : (i <= 8 ? UserRole.Employee : UserRole.Customer),
                    SecurityStamp = Guid.NewGuid().ToString()
                };
                user.PasswordHash = hasher.HashPassword(user, "Password@123");
                users.Add(user);
            }
            modelBuilder.Entity<User>().HasData(users);

            // Brands (50 records)
            var brands = new List<Brand>();
            var brandNames = new[] { "Adidas", "Adidas Originals", "Adidas Performance", "Adidas Y-3", "Stella McCartney",
                                   "Pharrell Williams", "Ivy Park", "Human Made", "Adidas Golf", "Adidas Skateboarding" };
            for (int i = 0; i < 50; i++)
            {
                brands.Add(new Brand
                {
                    Id = Guid.NewGuid(),
                    Name = $"{brandNames[i % brandNames.Length]}{(i >= brandNames.Length ? $" {(i / brandNames.Length) + 1}" : "")}",
                    LogoUrl = $"https://example.com/logos/brand{i + 1}.png",
                    Description = $"Premium {brandNames[i % brandNames.Length]} collection with innovative design and technology.",
                    IsDeleted = false,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow.AddDays(-i * 3),
                    UpdatedAt = DateTime.UtcNow.AddDays(-i * 3),
                    AddedById = adminUserId
                });
            }
            modelBuilder.Entity<Brand>().HasData(brands);

            // Categories (50 records)
            var categories = new List<Category>();
            var categoryNames = new[] { "Footwear", "Clothing", "Accessories", "Running", "Football",
                                      "Basketball", "Training", "Lifestyle", "Originals", "Performance" };
            for (int i = 0; i < 50; i++)
            {
                categories.Add(new Category
                {
                    Id = Guid.NewGuid(),
                    Name = $"{categoryNames[i % categoryNames.Length]}{(i >= categoryNames.Length ? $" {(i / categoryNames.Length) + 1}" : "")}",
                    Slug = $"{categoryNames[i % categoryNames.Length].ToLower().Replace(" ", "-")}-{i + 1}",
                    Description = $"High-quality {categoryNames[i % categoryNames.Length].ToLower()} collection for all your sporting needs.",
                    ImageUrl = $"https://example.com/categories/category{i + 1}.jpg",
                    SortOrder = i + 1,
                    ParentCategoryId = i > 10 && i % 5 == 0 ? categories[i % 10].Id : null,
                    IsDeleted = false,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow.AddDays(-i * 2),
                    UpdatedAt = DateTime.UtcNow.AddDays(-i * 2),
                    AddedById = adminUserId
                });
            }
            modelBuilder.Entity<Category>().HasData(categories);

            // ProductAttributes (50 records)
            var productAttributes = new List<ProductAttribute>();
            var attributeNames = new[] { "Size", "Color", "Material", "Technology", "Weight", "Fit", "Style", "Season", "Care", "Origin" };
            var dataTypes = new[] { "string", "number", "boolean", "list" };
            for (int i = 0; i < 50; i++)
            {
                productAttributes.Add(new ProductAttribute
                {
                    Id = Guid.NewGuid(),
                    Name = $"{attributeNames[i % attributeNames.Length]}{(i >= attributeNames.Length ? $" {(i / attributeNames.Length) + 1}" : "")}",
                    DataType = dataTypes[i % dataTypes.Length],
                    IsFilterable = i % 3 != 0,
                    IsRequired = i % 4 == 0,
                    SortOrder = i + 1,
                    IsDeleted = false,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow.AddDays(-i),
                    UpdatedAt = DateTime.UtcNow.AddDays(-i),
                    AddedById = adminUserId
                });
            }
            modelBuilder.Entity<ProductAttribute>().HasData(productAttributes);

            // Products (50 records)
            var products = new List<Product>();
            var productNames = new[] { "Ultra Boost", "Stan Smith", "Superstar", "Gazelle", "NMD",
                                     "Yeezy", "Forum", "Continental", "Samba", "Campus" };
            for (int i = 0; i < 50; i++)
            {
                var basePrice = 80 + (i * 5);
                products.Add(new Product
                {
                    Id = Guid.NewGuid(),
                    Name = $"Adidas {productNames[i % productNames.Length]}{(i >= productNames.Length ? $" {(i / productNames.Length) + 1}" : "")}",
                    Description = $"Experience the perfect blend of comfort and style with the {productNames[i % productNames.Length]}. Featuring premium materials and innovative technology for exceptional performance.",
                    ShortDescription = $"Premium {productNames[i % productNames.Length]} for ultimate comfort and style.",
                    Price = basePrice,
                    SalePrice = i % 3 == 0 ? basePrice - 20 : null,
                    GenderTarget = i % 4 == 0 ? Gender.Male : i % 4 == 1 ? Gender.Female : i % 4 == 2 ? Gender.Kids : Gender.Unisex,
                    MetaTitle = $"Adidas {productNames[i % productNames.Length]} | Official Store",
                    MetaDescription = $"Shop the latest {productNames[i % productNames.Length]} collection at Adidas official store.",
                    Sku = $"ADI-{productNames[i % productNames.Length].Replace(" ", "").ToUpper()}-{i + 1:D3}",
                    Specifications = "{\"material\": \"Premium synthetic\", \"sole\": \"Rubber outsole\", \"closure\": \"Lace-up\"}",
                    CategoryId = categories[i % categories.Count].Id,
                    BrandId = brands[i % brands.Count].Id,
                    IsDeleted = false,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow.AddDays(-i * 2),
                    UpdatedAt = DateTime.UtcNow.AddDays(-i * 2),
                    AddedById = adminUserId
                });
            }
            modelBuilder.Entity<Product>().HasData(products);

            // ProductVariants (50 records)
            var productVariants = new List<ProductVariant>();
            var sizes = new[] { "6", "6.5", "7", "7.5", "8", "8.5", "9", "9.5", "10", "10.5", "11", "11.5", "12", "S", "M", "L", "XL" };
            var colors = new[] { "Black", "White", "Red", "Blue", "Green", "Navy", "Gray", "Brown", "Pink", "Purple" };
            for (int i = 0; i < 50; i++)
            {
                productVariants.Add(new ProductVariant
                {
                    Id = Guid.NewGuid(),
                    Sku = $"VAR-{products[i % products.Count].Sku}-{sizes[i % sizes.Length]}-{colors[i % colors.Length]}",
                    Size = sizes[i % sizes.Length],
                    Color = colors[i % colors.Length],
                    StockQuantity = 50 + (i % 100),
                    PriceAdjustment = i % 5 == 0 ? 10 : 0,
                    ImageUrl = $"https://example.com/products/variants/variant{i + 1}.jpg",
                    ProductId = products[i % products.Count].Id,
                    IsDeleted = false,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow.AddDays(-i),
                    UpdatedAt = DateTime.UtcNow.AddDays(-i),
                    AddedById = adminUserId
                });
            }
            modelBuilder.Entity<ProductVariant>().HasData(productVariants);

            // ProductImages (50 records)
            var productImages = new List<ProductImage>();
            for (int i = 0; i < 50; i++)
            {
                productImages.Add(new ProductImage
                {
                    Id = Guid.NewGuid(),
                    ImageUrl = $"https://example.com/products/images/product{i + 1}_main.jpg",
                    AltText = $"{products[i % products.Count].Name} - Main Image",
                    SortOrder = i + 1,
                    IsPrimary = i % 10 == 0,
                    ProductId = products[i % products.Count].Id,
                    VariantId = i % 2 == 0 ? productVariants[i % productVariants.Count].Id : null,
                    IsDeleted = false,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow.AddDays(-i),
                    UpdatedAt = DateTime.UtcNow.AddDays(-i),
                    AddedById = adminUserId
                });
            }
            modelBuilder.Entity<ProductImage>().HasData(productImages);

            // ProductAttributeValues (50 records)
            var productAttributeValues = new List<ProductAttributeValue>();
            var attributeValues = new[] { "Cotton", "Leather", "Synthetic", "Mesh", "Premium", "Standard", "Lightweight", "Durable", "Comfortable", "Breathable" };
            for (int i = 0; i < 50; i++)
            {
                productAttributeValues.Add(new ProductAttributeValue
                {
                    Id = Guid.NewGuid(),
                    Value = attributeValues[i % attributeValues.Length],
                    ProductId = products[i % products.Count].Id,
                    AttributeId = productAttributes[i % productAttributes.Count].Id,
                    IsDeleted = false,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow.AddDays(-i),
                    UpdatedAt = DateTime.UtcNow.AddDays(-i),
                    AddedById = adminUserId
                });
            }
            modelBuilder.Entity<ProductAttributeValue>().HasData(productAttributeValues);

            // Addresses (50 records)
            var addresses = new List<Address>();
            var cities = new[] { "Cairo", "Alexandria", "Giza", "Luxor", "Aswan", "Mansoura", "Tanta", "Ismailia", "Suez", "Hurghada" };
            var streets = new[] { "Tahrir Square", "Nile Corniche", "Zamalek St", "Heliopolis Ave", "Maadi Rd",
                                "Garden City", "Downtown", "New Cairo", "6th October", "Nasr City" };
            for (int i = 0; i < 50; i++)
            {
                addresses.Add(new Address
                {
                    Id = Guid.NewGuid(),
                    AddressType = i % 3 == 0 ? "Home" : i % 3 == 1 ? "Work" : "Other",
                    StreetAddress = $"{i + 1} {streets[i % streets.Length]}, Apt {i + 100}",
                    City = cities[i % cities.Length],
                    StateProvince = $"{cities[i % cities.Length]} Governorate",
                    PostalCode = $"{11000 + i:D5}",
                    Country = "Egypt",
                    IsDefault = i % 5 == 0,
                    UserId = users[i % users.Count].Id,
                    IsDeleted = false,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow.AddDays(-i),
                    UpdatedAt = DateTime.UtcNow.AddDays(-i),
                    AddedById = adminUserId
                });
            }
            modelBuilder.Entity<Address>().HasData(addresses);

            // Coupons (50 records)
            var coupons = new List<Coupon>();
            var couponCodes = new[] { "WELCOME10", "SAVE20", "SUMMER25", "WINTER30", "SPORT15", "NEW50", "VIP40", "SALE35", "FIRST20", "MEMBER25" };
            for (int i = 0; i < 50; i++)
            {
                var discountValue = (i % 3) switch
                {
                    0 => 10 + (i % 5), // Percentage: 10-14%
                    1 => 50 + (i % 10) * 10, // Amount: 50-140
                    _ => 100 + (i % 5) * 20 // Fixed: 100-180
                };
                coupons.Add(new Coupon
                {
                    Id = Guid.NewGuid(),
                    Code = $"{couponCodes[i % couponCodes.Length]}{(i >= couponCodes.Length ? (i / couponCodes.Length).ToString() : "")}",
                    Name = $"Special Discount {i + 1}",
                    DiscountType = i % 3 == 0 ? DiscountType.Percentage : i % 3 == 1 ? DiscountType.Amount : DiscountType.FixedAmount,
                    DiscountValue = discountValue,
                    MinimumAmount = i % 4 == 0 ? 0 : 100 + (i * 10),
                    ValidFrom = DateTime.UtcNow.AddDays(-30 - (i % 10)),
                    ValidTo = DateTime.UtcNow.AddDays(30 + (i % 20)),
                    UsageLimit = i % 2 == 0 ? 100 + (i * 5) : 0, // 0 means unlimited
                    UsedCount = i % 15,
                    IsDeleted = false,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow.AddDays(-i),
                    UpdatedAt = DateTime.UtcNow.AddDays(-i),
                    AddedById = adminUserId
                });
            }
            modelBuilder.Entity<Coupon>().HasData(coupons);

            // Orders (50 records)
            var orders = new List<Order>();
            var currencies = new[] { "EGP", "USD", "SAR", "AED" };
            for (int i = 0; i < 50; i++)
            {
                var orderDate = DateTime.UtcNow.AddDays(-(i + 1) * 5);
                var subtotal = 150 + (i * 25);
                var taxAmount = Math.Round(subtotal * 0.14m, 2); // 14% tax
                var shippingAmount = 30;
                var discountAmount = i % 4 == 0 ? 25 : 0;
                var totalAmount = subtotal + taxAmount + shippingAmount - discountAmount;
                orders.Add(new Order
                {
                    Id = Guid.NewGuid(),
                    OrderNumber = $"ADI-{DateTime.Now.Year}-{(i + 1):D6}",
                    OrderStatus = i % 5 == 0 ? OrderStatus.Pending : i % 5 == 1 ? OrderStatus.Processing : i % 5 == 2 ? OrderStatus.Shipped : i % 5 == 3 ? OrderStatus.Delivered : OrderStatus.Cancelled,
                    Subtotal = subtotal,
                    TaxAmount = taxAmount,
                    ShippingAmount = shippingAmount,
                    DiscountAmount = discountAmount,
                    TotalAmount = totalAmount,
                    Currency = currencies[i % currencies.Length],
                    OrderDate = orderDate,
                    ShippedDate = i % 5 >= 2 ? orderDate.AddDays(2) : null,
                    DeliveredDate = i % 5 == 3 ? orderDate.AddDays(5) : null,
                    ShippingAddress = $"{{\"street\": \"{addresses[i % addresses.Count].StreetAddress}\", \"city\": \"{addresses[i % addresses.Count].City}\", \"country\": \"Egypt\"}}",
                    BillingAddress = $"{{\"street\": \"{addresses[i % addresses.Count].StreetAddress}\", \"city\": \"{addresses[i % addresses.Count].City}\", \"country\": \"Egypt\"}}",
                    Notes = i % 3 == 0 ? $"Special delivery instructions for order {i + 1}" : "no special instructions",
                    UserId = users[i % users.Count].Id,
                    IsDeleted = false,
                    IsActive = true,
                    CreatedAt = orderDate,
                    UpdatedAt = orderDate,
                    AddedById = adminUserId
                });
            }
            modelBuilder.Entity<Order>().HasData(orders);

            // OrderItems (50 records)
            var orderItems = new List<OrderItem>();
            for (int i = 0; i < 50; i++)
            {
                var quantity = (i % 3) + 1;
                var unitPrice = products[i % products.Count].Price + productVariants[i % productVariants.Count].PriceAdjustment;
                var totalPrice = quantity * unitPrice;
                orderItems.Add(new OrderItem
                {
                    Id = Guid.NewGuid(),
                    Quantity = quantity,
                    UnitPrice = unitPrice,
                    TotalPrice = totalPrice,
                    ProductName = products[i % products.Count].Name,
                    VariantDetails = $"Size: {productVariants[i % productVariants.Count].Size}, Color: {productVariants[i % productVariants.Count].Color}",
                    OrderId = orders[i % orders.Count].Id,
                    VariantId = productVariants[i % productVariants.Count].Id,
                    IsDeleted = false,
                    IsActive = true,
                    CreatedAt = orders[i % orders.Count].OrderDate,
                    UpdatedAt = orders[i % orders.Count].OrderDate,
                    AddedById = adminUserId
                });
            }
            modelBuilder.Entity<OrderItem>().HasData(orderItems);

            // OrderCoupons (50 records)
            var orderCoupons = new List<OrderCoupon>();
            for (int i = 0; i < 50; i++)
            {
                var coupon = coupons[i % coupons.Count];
                var order = orders[i % orders.Count];
                var discountApplied = coupon.DiscountType switch
                {
                    DiscountType.Percentage => Math.Round(order.Subtotal * (coupon.DiscountValue / 100), 2),
                    DiscountType.Amount => Math.Min(coupon.DiscountValue, order.Subtotal),
                    DiscountType.FixedAmount => coupon.DiscountValue,
                    _ => 0
                };
                orderCoupons.Add(new OrderCoupon
                {
                    Id = Guid.NewGuid(),
                    DiscountApplied = discountApplied,
                    CouponId = coupon.Id,
                    OrderId = order.Id,
                    IsDeleted = false,
                    IsActive = true,
                    CreatedAt = order.OrderDate,
                    UpdatedAt = order.OrderDate,
                    AddedById = adminUserId
                });
            }
            modelBuilder.Entity<OrderCoupon>().HasData(orderCoupons);

            // Payments (50 records)
            var payments = new List<Payment>();
            var paymentMethods = new[] { "Credit Card", "Debit Card", "PayPal", "Cash on Delivery", "Bank Transfer" };
            var paymentStatuses = new[] { "Completed", "Pending", "Failed", "Refunded" };
            for (int i = 0; i < 50; i++)
            {
                var order = orders[i % orders.Count];
                var status = i % 10 == 9 ? "Failed" : (i % 20 == 19 ? "Refunded" : (i % 5 == 4 ? "Pending" : "Completed"));
                payments.Add(new Payment
                {
                    Id = Guid.NewGuid(),
                    PaymentMethod = paymentMethods[i % paymentMethods.Length],
                    PaymentStatus = status,
                    Amount = order.TotalAmount,
                    TransactionId = $"TXN-{DateTime.Now.Year}-{(i + 1):D8}",
                    GatewayResponse = status == "Completed" ? "Payment processed successfully" :
                                      status == "Failed" ? "Payment failed - insufficient funds" :
                                      status == "Refunded" ? "Payment refunded successfully" : "Payment pending processing",
                    ProcessedAt = order.OrderDate.AddMinutes(5 + (i % 30)),
                    OrderId = order.Id,
                    IsDeleted = false,
                    IsActive = true,
                    CreatedAt = order.OrderDate,
                    UpdatedAt = order.OrderDate.AddMinutes(5 + (i % 30)),
                    AddedById = adminUserId
                });
            }
            modelBuilder.Entity<Payment>().HasData(payments);

            // Reviews (50 records)
            var reviews = new List<Review>();
            var reviewTitles = new[] { "Excellent Quality!", "Great Product", "Highly Recommended", "Perfect Fit",
                                     "Outstanding Design", "Good Value", "Love It!", "Amazing Comfort", "Stylish Choice", "Top Quality" };
            var reviewTexts = new[] {
                "This product exceeded my expectations. The quality is outstanding and fits perfectly.",
                "Really happy with this purchase. Great design and comfortable to wear.",
                "Excellent product with premium materials. Highly recommend to everyone.",
                "Perfect for daily use. The quality and comfort are exceptional.",
                "Amazing product! The style and comfort are exactly what I was looking for."
            };
            for (int i = 0; i < 50; i++)
            {
                reviews.Add(new Review
                {
                    Id = Guid.NewGuid(),
                    Rating = (i % 5) + 1, // 1-5 stars
                    Title = reviewTitles[i % reviewTitles.Length],
                    ReviewText = $"{reviewTexts[i % reviewTexts.Length]} Review #{i + 1}",
                    IsVerifiedPurchase = i % 3 != 0, // 2/3 are verified purchases
                    IsApproved = i % 4 != 0, // 3/4 are approved
                    ProductId = products[i % products.Count].Id,
                    UserId = users[i % users.Count].Id,
                    IsDeleted = false,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow.AddDays(-(i + 1) * 3),
                    UpdatedAt = DateTime.UtcNow.AddDays(-(i + 1) * 3),
                    AddedById = adminUserId
                });
            }
            modelBuilder.Entity<Review>().HasData(reviews);

            // ShoppingCarts (50 records)
            var shoppingCarts = new List<ShoppingCart>();
            for (int i = 0; i < 50; i++)
            {
                var addedDate = DateTime.UtcNow.AddDays(-(i % 30)); // Items added in last 30 days
                shoppingCarts.Add(new ShoppingCart
                {
                    Id = Guid.NewGuid(),
                    Quantity = (i % 5) + 1, // 1-5 items
                    AddedAt = addedDate,
                    UserId = users[i % users.Count].Id,
                    VariantId = productVariants[i % productVariants.Count].Id,
                    IsDeleted = false,
                    IsActive = true,
                    CreatedAt = addedDate,
                    UpdatedAt = addedDate,
                    AddedById = adminUserId
                });
            }
            modelBuilder.Entity<ShoppingCart>().HasData(shoppingCarts);

            // Wishlists (50 records)
            var wishlists = new List<Wishlist>();
            for (int i = 0; i < 50; i++)
            {
                var addedDate = DateTime.UtcNow.AddDays(-(i % 60)); // Items added in last 60 days
                wishlists.Add(new Wishlist
                {
                    Id = Guid.NewGuid(),
                    AddedAt = addedDate,
                    UserId = users[i % users.Count].Id,
                    ProductId = products[i % products.Count].Id,
                    IsDeleted = false,
                    IsActive = true,
                    CreatedAt = addedDate,
                    UpdatedAt = addedDate,
                    AddedById = adminUserId
                });
            }
            modelBuilder.Entity<Wishlist>().HasData(wishlists);

            // InventoryLogs (50 records)
            var inventoryLogs = new List<InventoryLog>();
            var changeTypes = new[] { "Restock", "Sale", "Adjustment", "Return", "Damaged" };
            for (int i = 0; i < 50; i++)
            {
                var quantityChange = i % 2 == 0 ? (i % 10 + 1) * 5 : -(i % 10 + 1) * 5;
                var previousStock = 100 + (i % 50);
                var newStock = previousStock + quantityChange;
                inventoryLogs.Add(new InventoryLog
                {
                    Id = Guid.NewGuid(),
                    QuantityChange = quantityChange,
                    PreviousStock = previousStock,
                    NewStock = newStock,
                    ChangeType = changeTypes[i % changeTypes.Length],
                    Reason = $"Inventory {changeTypes[i % changeTypes.Length].ToLower()} for variant {i + 1}",
                    VariantId = productVariants[i % productVariants.Count].Id,
                    IsDeleted = false,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow.AddDays(-(i + 1) * 3),
                    UpdatedAt = DateTime.UtcNow.AddDays(-(i + 1) * 3),
                    AddedById = adminUserId
                });
            }
            modelBuilder.Entity<InventoryLog>().HasData(inventoryLogs);
        }
    }
}