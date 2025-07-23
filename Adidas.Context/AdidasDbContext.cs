using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System.Text.Json;
using Adidas.Models.Feature;
using Adidas.Models.Main;
using Adidas.Models.Operation;
using Adidas.Models.Separator;
using Adidas.Models.Tracker;
using Models.People;

namespace Adidas.Context
{
    public class AdidasDbContext : DbContext
    {
        public AdidasDbContext(DbContextOptions<AdidasDbContext> options) : base(options) { }

        #region Core Product Catalog
        public DbSet<Product> Products { get; set; }
        public DbSet<ProductVariant> ProductVariants { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Brand> Brands { get; set; }
        public DbSet<ProductImage> ProductImages { get; set; }
        public DbSet<ProductAttribute> ProductAttributes { get; set; }
        public DbSet<ProductAttributeValue> ProductAttributeValues { get; set; }
        public DbSet<InventoryLog> InventoryLogs { get; set; }
        #endregion

        #region User & Authentication
        public DbSet<User> Users { get; set; }
        public DbSet<Address> Addresses { get; set; }
        //public DbSet<UserSession> UserSessions { get; set; }
        //public DbSet<UserPreference> UserPreferences { get; set; }
        //public DbSet<SavedPaymentMethod> SavedPaymentMethods { get; set; }
        #endregion

        #region Order Management
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
        public DbSet<ShoppingCart> ShoppingCarts { get; set; }
        public DbSet<Payment> Payments { get; set; }
        //public DbSet<Discount> Discounts { get; set; }
        public DbSet<OrderCoupon> OrderCoupons { get; set; }
        //public DbSet<ShippingMethod> ShippingMethods { get; set; }
        //public DbSet<TaxRate> TaxRates { get; set; }
        #endregion

        #region Content & Marketing
        //public DbSet<Banner> Banners { get; set; }
        //public DbSet<Promotion> Promotions { get; set; }
        //public DbSet<ContentBlock> ContentBlocks { get; set; }
        //public DbSet<SeoMetadata> SeoMetadata { get; set; }
        //public DbSet<Localization> Localizations { get; set; }
        //public DbSet<Newsletter> Newsletters { get; set; }
        #endregion

        #region Customer Engagement
        public DbSet<Review> Reviews { get; set; }
        public DbSet<Wishlist> Wishlists { get; set; }
        //public DbSet<RecentlyViewed> RecentlyViewed { get; set; }
        //public DbSet<ProductRecommendation> ProductRecommendations { get; set; }
        #endregion

        #region Analytics & Logging
        //public DbSet<UserBehaviorLog> UserBehaviorLogs { get; set; }
        //public DbSet<SearchLog> SearchLogs { get; set; }
        //public DbSet<ProductAnalytics> ProductAnalytics { get; set; }
        //public DbSet<ConversionMetric> ConversionMetrics { get; set; }
        //public DbSet<WebhookLog> WebhookLogs { get; set; }
        //#endregion

        //#region API & Integrations
        //public DbSet<ApiKey> ApiKeys { get; set; }
        //public DbSet<IntegrationLog> IntegrationLogs { get; set; }
        //public DbSet<WebhookEndpoint> WebhookEndpoints { get; set; }
        //public DbSet<ExternalService> ExternalServices { get; set; }
        #endregion

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            #region Product Catalog Configuration
            // Product Configuration
            modelBuilder.Entity<Product>(entity =>
            {
                entity.HasKey(e => e.ProductId);
                entity.HasIndex(e => e.Sku).IsUnique();
                entity.HasIndex(e => new { e.CategoryId, e.IsActive });
                entity.HasIndex(e => new { e.BrandId, e.IsActive });
                entity.HasIndex(e => e.GenderTarget);
                entity.HasIndex(e => e.CreatedAt);

                entity.Property(e => e.Price).HasPrecision(18, 2);
                entity.Property(e => e.SalePrice).HasPrecision(18, 2);

                entity.HasOne(p => p.Category)
                    .WithMany(c => c.Products)
                    .HasForeignKey(p => p.CategoryId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(p => p.Brand)
                    .WithMany(b => b.Products)
                    .HasForeignKey(p => p.BrandId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Product Variant Configuration
            modelBuilder.Entity<ProductVariant>(entity =>
            {
                entity.HasKey(e => e.VariantId);
                entity.HasIndex(e => e.Sku).IsUnique();
                entity.HasIndex(e => new { e.ProductId, e.Size, e.Color }).IsUnique();
                entity.HasIndex(e => new { e.ProductId, e.IsActive });
                entity.HasIndex(e => e.StockQuantity);

                entity.Property(e => e.PriceAdjustment).HasPrecision(18, 2);

                entity.HasOne(pv => pv.Product)
                    .WithMany(p => p.Variants)
                    .HasForeignKey(pv => pv.ProductId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Category Configuration
            modelBuilder.Entity<Category>(entity =>
            {
                entity.HasKey(e => e.CategoryId);
                entity.HasIndex(e => e.Slug).IsUnique();
                entity.HasIndex(e => new { e.ParentCategoryId, e.IsActive });
                entity.HasIndex(e => e.SortOrder);

                entity.HasOne(c => c.ParentCategory)
                    .WithMany(c => c.SubCategories)
                    .HasForeignKey(c => c.ParentCategoryId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Product Images Configuration
            modelBuilder.Entity<ProductImage>(entity =>
            {
                entity.HasKey(e => e.ImageId);
                entity.HasIndex(e => new { e.ProductId, e.IsPrimary });
                entity.HasIndex(e => new { e.VariantId, e.SortOrder });

                entity.HasOne(pi => pi.Product)
                    .WithMany(p => p.Images)
                    .HasForeignKey(pi => pi.ProductId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(pi => pi.Variant)
                    .WithMany(pv => pv.Images)
                    .HasForeignKey(pi => pi.VariantId)
                    .OnDelete(DeleteBehavior.Cascade);
            });
            #endregion

            #region User & Authentication Configuration
            // User Configuration
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(e => e.UserId);
                entity.HasIndex(e => e.Email).IsUnique();
                entity.HasIndex(e => new { e.IsActive, e.Role });
                entity.HasIndex(e => e.CreatedAt);
            });

            // Address Configuration
            modelBuilder.Entity<Address>(entity =>
            {
                entity.HasKey(e => e.AddressId);
                entity.HasIndex(e => new { e.UserId, e.IsDefault });
                entity.HasIndex(e => new { e.Country, e.StateProvince, e.City });

                entity.HasOne(a => a.User)
                    .WithMany(u => u.Addresses)
                    .HasForeignKey(a => a.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            //// User Session Configuration
            //modelBuilder.Entity<UserSession>(entity =>
            //{
            //    entity.HasKey(e => e.SessionId);
            //    entity.HasIndex(e => e.UserId);
            //    entity.HasIndex(e => e.ExpiresAt);
            //    entity.HasIndex(e => e.IsActive);

            //    entity.HasOne(us => us.User)
            //        .WithMany()
            //        .HasForeignKey(us => us.UserId)
            //        .OnDelete(DeleteBehavior.Cascade);
            //});

            // User Preferences Configuration
            //modelBuilder.Entity<UserPreference>(entity =>
            //{
            //    entity.HasKey(e => e.PreferenceId);
            //    entity.HasIndex(e => new { e.UserId, e.PreferenceKey }).IsUnique();

            //    entity.HasOne(up => up.User)
            //        .WithMany()
            //        .HasForeignKey(up => up.UserId)
            //        .OnDelete(DeleteBehavior.Cascade);
            //});

            //// Saved Payment Methods Configuration
            //modelBuilder.Entity<SavedPaymentMethod>(entity =>
            //{
            //    entity.HasKey(e => e.PaymentMethodId);
            //    entity.HasIndex(e => new { e.UserId, e.IsDefault });

            //    entity.HasOne(spm => spm.User)
            //        .WithMany()
            //        .HasForeignKey(spm => spm.UserId)
            //        .OnDelete(DeleteBehavior.Cascade);
            //});
            #endregion

            #region Order Management Configuration
            // Order Configuration
            modelBuilder.Entity<Order>(entity =>
            {
                entity.HasKey(e => e.OrderId);
                entity.HasIndex(e => e.OrderNumber).IsUnique();
                entity.HasIndex(e => new { e.UserId, e.OrderDate });
                entity.HasIndex(e => e.OrderStatus);
                entity.HasIndex(e => e.OrderDate);

                entity.Property(e => e.Subtotal).HasPrecision(18, 2);
                entity.Property(e => e.TaxAmount).HasPrecision(18, 2);
                entity.Property(e => e.ShippingAmount).HasPrecision(18, 2);
                entity.Property(e => e.DiscountAmount).HasPrecision(18, 2);
                entity.Property(e => e.TotalAmount).HasPrecision(18, 2);

                entity.HasOne(o => o.User)
                    .WithMany(u => u.Orders)
                    .HasForeignKey(o => o.UserId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Order Item Configuration
            modelBuilder.Entity<OrderItem>(entity =>
            {
                entity.HasKey(e => e.OrderItemId);
                entity.HasIndex(e => e.OrderId);
                entity.HasIndex(e => e.VariantId);

                entity.Property(e => e.UnitPrice).HasPrecision(18, 2);
                entity.Property(e => e.TotalPrice).HasPrecision(18, 2);

                entity.HasOne(oi => oi.Order)
                    .WithMany(o => o.OrderItems)
                    .HasForeignKey(oi => oi.OrderId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(oi => oi.Variant)
                    .WithMany(pv => pv.OrderItems)
                    .HasForeignKey(oi => oi.VariantId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Shopping Cart Configuration
            modelBuilder.Entity<ShoppingCart>(entity =>
            {
                entity.HasKey(e => e.CartId);
                entity.HasIndex(e => new { e.UserId, e.VariantId }).IsUnique();
                entity.HasIndex(e => e.AddedAt);

                entity.HasOne(sc => sc.User)
                    .WithMany(u => u.CartItems)
                    .HasForeignKey(sc => sc.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(sc => sc.Variant)
                    .WithMany(pv => pv.CartItems)
                    .HasForeignKey(sc => sc.VariantId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Payment Configuration
            modelBuilder.Entity<Payment>(entity =>
            {
                entity.HasKey(e => e.PaymentId);
                entity.HasIndex(e => e.OrderId);
                entity.HasIndex(e => e.TransactionId);
                entity.HasIndex(e => e.PaymentStatus);
                entity.HasIndex(e => e.ProcessedAt);

                entity.Property(e => e.Amount).HasPrecision(18, 2);

                entity.HasOne(p => p.Order)
                    .WithMany(o => o.Payments)
                    .HasForeignKey(p => p.OrderId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Discount Configuration
            //modelBuilder.Entity<Discount>(entity =>
            //{
            //    entity.HasKey(e => e.CouponId);
            //    entity.HasIndex(e => e.Code).IsUnique();
            //    entity.HasIndex(e => new { e.ValidFrom, e.ValidTo, e.IsActive });
            //    entity.HasIndex(e => e.DiscountType);

            //    entity.Property(e => e.DiscountValue).HasPrecision(18, 2);
            //    entity.Property(e => e.MinimumAmount).HasPrecision(18, 2);
            //});

            //// Shipping Method Configuration
            //modelBuilder.Entity<ShippingMethod>(entity =>
            //{
            //    entity.HasKey(e => e.ShippingMethodId);
            //    entity.HasIndex(e => new { e.IsActive, e.SortOrder });

            //    entity.Property(e => e.Cost).HasPrecision(18, 2);
            //    entity.Property(e => e.FreeShippingThreshold).HasPrecision(18, 2);
            //});

            //// Tax Rate Configuration
            //modelBuilder.Entity<TaxRate>(entity =>
            //{
            //    entity.HasKey(e => e.TaxRateId);
            //    entity.HasIndex(e => new { e.Country, e.StateProvince, e.IsActive });

            //    entity.Property(e => e.Rate).HasPrecision(5, 4);
            //});
            //#endregion

            //#region Content & Marketing Configuration
            //// Banner Configuration
            //modelBuilder.Entity<Banner>(entity =>
            //{
            //    entity.HasKey(e => e.BannerId);
            //    entity.HasIndex(e => new { e.Position, e.IsActive, e.SortOrder });
            //    entity.HasIndex(e => new { e.ValidFrom, e.ValidTo });
            //});

            //// Promotion Configuration
            //modelBuilder.Entity<Promotion>(entity =>
            //{
            //    entity.HasKey(e => e.PromotionId);
            //    entity.HasIndex(e => new { e.IsActive, e.ValidFrom, e.ValidTo });
            //    entity.HasIndex(e => e.PromotionType);
            //});

            //// SEO Metadata Configuration
            //modelBuilder.Entity<SeoMetadata>(entity =>
            //{
            //    entity.HasKey(e => e.SeoId);
            //    entity.HasIndex(e => new { e.EntityType, e.EntityId }).IsUnique();
            //    entity.HasIndex(e => e.UrlSlug);
            //});

            //// Localization Configuration
            //modelBuilder.Entity<Localization>(entity =>
            //{
            //    entity.HasKey(e => e.LocalizationId);
            //    entity.HasIndex(e => new { e.LanguageCode, e.ResourceKey }).IsUnique();
            //    entity.HasIndex(e => e.LanguageCode);
            //});
            #endregion

            #region Customer Engagement Configuration
            // Review Configuration
            modelBuilder.Entity<Review>(entity =>
            {
                entity.HasKey(e => e.ReviewId);
                entity.HasIndex(e => new { e.ProductId, e.IsApproved });
                entity.HasIndex(e => new { e.UserId, e.CreatedAt });
                entity.HasIndex(e => e.Rating);

                entity.HasOne(r => r.Product)
                    .WithMany(p => p.Reviews)
                    .HasForeignKey(r => r.ProductId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(r => r.User)
                    .WithMany(u => u.Reviews)
                    .HasForeignKey(r => r.UserId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Wishlist Configuration
            modelBuilder.Entity<Wishlist>(entity =>
            {
                entity.HasKey(e => e.WishlistId);
                entity.HasIndex(e => new { e.UserId, e.ProductId }).IsUnique();
                entity.HasIndex(e => e.AddedAt);

                entity.HasOne(w => w.User)
                    .WithMany(u => u.Wishlists)
                    .HasForeignKey(w => w.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(w => w.Product)
                    .WithMany(p => p.Wishlists)
                    .HasForeignKey(w => w.ProductId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Recently Viewed Configuration
            //modelBuilder.Entity<RecentlyViewed>(entity =>
            //{
            //    entity.HasKey(e => e.RecentlyViewedId);
            //    entity.HasIndex(e => new { e.UserId, e.ViewedAt });
            //    entity.HasIndex(e => new { e.ProductId, e.ViewedAt });

            //    entity.HasOne(rv => rv.User)
            //        .WithMany()
            //        .HasForeignKey(rv => rv.UserId)
            //        .OnDelete(DeleteBehavior.Cascade);

            //    entity.HasOne(rv => rv.Product)
            //        .WithMany()
            //        .HasForeignKey(rv => rv.ProductId)
            //        .OnDelete(DeleteBehavior.Cascade);
            //});
            //#endregion

            //#region Analytics & Logging Configuration
            //// User Behavior Log Configuration
            //modelBuilder.Entity<UserBehaviorLog>(entity =>
            //{
            //    entity.HasKey(e => e.LogId);
            //    entity.HasIndex(e => new { e.UserId, e.Timestamp });
            //    entity.HasIndex(e => new { e.ActionType, e.Timestamp });
            //    entity.HasIndex(e => e.SessionId);
            //});

            //// Search Log Configuration
            //modelBuilder.Entity<SearchLog>(entity =>
            //{
            //    entity.HasKey(e => e.SearchLogId);
            //    entity.HasIndex(e => new { e.SearchTerm, e.Timestamp });
            //    entity.HasIndex(e => new { e.UserId, e.Timestamp });
            //    entity.HasIndex(e => e.ResultCount);
            //});

            //// Product Analytics Configuration
            //modelBuilder.Entity<ProductAnalytics>(entity =>
            //{
            //    entity.HasKey(e => e.AnalyticsId);
            //    entity.HasIndex(e => new { e.ProductId, e.Date }).IsUnique();
            //    entity.HasIndex(e => e.Date);
            //});
            //#endregion

            //#region API & Integrations Configuration
            //// API Key Configuration
            //modelBuilder.Entity<ApiKey>(entity =>
            //{
            //    entity.HasKey(e => e.ApiKeyId);
            //    entity.HasIndex(e => e.KeyHash).IsUnique();
            //    entity.HasIndex(e => new { e.IsActive, e.ExpiresAt });
            //});

            //// Webhook Endpoint Configuration
            //modelBuilder.Entity<WebhookEndpoint>(entity =>
            //{
            //    entity.HasKey(e => e.EndpointId);
            //    entity.HasIndex(e => new { e.IsActive, e.EventType });
            //});
            #endregion

            // Configure JSON conversions for complex properties
            ConfigureJsonProperties(modelBuilder);

            // Seed initial data
            SeedData(modelBuilder);
        }

        private void ConfigureJsonProperties(ModelBuilder modelBuilder)
        {
            var jsonStringConverter = new ValueConverter<Dictionary<string, object>, string>(
                v => JsonSerializer.Serialize(v, (JsonSerializerOptions)null),
                v => JsonSerializer.Deserialize<Dictionary<string, object>>(v, (JsonSerializerOptions)null)
            );

            // Configure JSON properties
            modelBuilder.Entity<Product>()
                .Property(e => e.Specifications)
                .HasConversion(jsonStringConverter);

            modelBuilder.Entity<Order>()
                .Property(e => e.ShippingAddress)
                .HasConversion(jsonStringConverter);

            modelBuilder.Entity<Order>()
                .Property(e => e.BillingAddress)
                .HasConversion(jsonStringConverter);

            ////modelBuilder.Entity<UserBehaviorLog>()
            ////    .Property(e => e.EventData)
            ////    .HasConversion(jsonStringConverter);

            ////modelBuilder.Entity<WebhookLog>()
            ////    .Property(e => e.Payload)
            ////    .HasConversion(jsonStringConverter);

            ////modelBuilder.Entity<WebhookLog>()
            ////    .Property(e => e.Response)
            ////    .HasConversion(jsonStringConverter);
        }

        private void SeedData(ModelBuilder modelBuilder)
        {
            // Seed Brands
            modelBuilder.Entity<Brand>().HasData(
                new Brand { BrandId = 1, Name = "Adidas", Description = "Impossible is Nothing", IsActive = true },
                new Brand { BrandId = 2, Name = "Adidas Originals", Description = "Original is Never Finished", IsActive = true },
                new Brand { BrandId = 3, Name = "Adidas Performance", Description = "Nothing is Impossible", IsActive = true }
            );

            // Seed Categories
            modelBuilder.Entity<Category>().HasData(
                new Category { CategoryId = 1, Name = "Footwear", Slug = "footwear", IsActive = true, SortOrder = 1 },
                new Category { CategoryId = 2, Name = "Clothing", Slug = "clothing", IsActive = true, SortOrder = 2 },
                new Category { CategoryId = 3, Name = "Accessories", Slug = "accessories", IsActive = true, SortOrder = 3 },
                new Category { CategoryId = 4, Name = "Running Shoes", Slug = "running-shoes", ParentCategoryId = 1, IsActive = true, SortOrder = 1 },
                new Category { CategoryId = 5, Name = "Lifestyle Shoes", Slug = "lifestyle-shoes", ParentCategoryId = 1, IsActive = true, SortOrder = 2 },
                new Category { CategoryId = 6, Name = "Football Boots", Slug = "football-boots", ParentCategoryId = 1, IsActive = true, SortOrder = 3 }
            );

            // Seed Shipping Methods
            //modelBuilder.Entity<ShippingMethod>().HasData(
            //    new ShippingMethod { ShippingMethodId = 1, Name = "Standard Delivery", Description = "3-5 business days", Cost = 4.95m, EstimatedDays = 4, IsActive = true, SortOrder = 1 },
            //    new ShippingMethod { ShippingMethodId = 2, Name = "Express Delivery", Description = "1-2 business days", Cost = 9.95m, EstimatedDays = 1, IsActive = true, SortOrder = 2 },
            //    new ShippingMethod { ShippingMethodId = 3, Name = "Free Standard Delivery", Description = "Free on orders over $75", Cost = 0m, EstimatedDays = 5, FreeShippingThreshold = 75m, IsActive = true, SortOrder = 3 }
            //);

            //// Seed Tax Rates
            //modelBuilder.Entity<TaxRate>().HasData(
            //    new TaxRate { TaxRateId = 1, Country = "US", StateProvince = "CA", TaxType = "Sales Tax", Rate = 0.0875m, IsActive = true },
            //    new TaxRate { TaxRateId = 2, Country = "US", StateProvince = "NY", TaxType = "Sales Tax", Rate = 0.08m, IsActive = true },
            //    new TaxRate { TaxRateId = 3, Country = "US", StateProvince = "TX", TaxType = "Sales Tax", Rate = 0.0625m, IsActive = true }
            //);
        }

        public override int SaveChanges()
        {
            UpdateTimestamps();
            return base.SaveChanges();
        }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            UpdateTimestamps();
            return await base.SaveChangesAsync(cancellationToken);
        }

        private void UpdateTimestamps()
        {
            var entries = ChangeTracker.Entries()
                .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified);

            foreach (var entry in entries)
            {
                if (entry.Property("UpdatedAt") != null)
                {
                    entry.Property("UpdatedAt").CurrentValue = DateTime.UtcNow;
                }

                if (entry.State == EntityState.Added && entry.Property("CreatedAt") != null)
                {
                    entry.Property("CreatedAt").CurrentValue = DateTime.UtcNow;
                }
            }
        }
    }
}