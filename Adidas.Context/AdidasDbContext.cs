using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System.Text.Json;
using Adidas.Models.Feature;
using Adidas.Models.Main;
using Adidas.Models.Operation;
using Adidas.Models.Separator;
using Adidas.Models.Tracker;
using Models.People;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Models.Feature;

namespace Adidas.Context
{
    public class AdidasDbContext : IdentityDbContext<User>
    {
        public AdidasDbContext(DbContextOptions<AdidasDbContext> options) : base(options) { }

        #region Core Product Catalog
        public DbSet<Product> Products { get; set; }
        public DbSet<ProductVariant> ProductVariants { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Brand> Brands { get; set; }
        public DbSet<ProductImage> ProductImages { get; set; }
        public DbSet<ProductAttribute> ProductAttributes { get; set; }
        public DbSet<AttributeValue> ProductAttributeValues { get; set; }
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
        public DbSet<Discount> Discounts { get; set; }
        public DbSet<OrderCoupon> OrderCoupons { get; set; }
        public DbSet<Coupon> Coupons { get; set; }
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
                entity.HasKey(e => e.Id);
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
                entity.HasKey(e => e.Id);
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
                entity.HasKey(e => e.Id);
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
                entity.HasKey(e => e.Id);
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
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => new { e.UserId, e.IsDefault });
                entity.HasIndex(e => new { e.Country, e.StateProvince, e.City });

                entity.HasOne(a => a.User)
                    .WithMany(u => u.Addresses)
                    .HasForeignKey(a => a.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });


            #endregion

            #region Order Management Configuration
            // Order Configuration
            modelBuilder.Entity<Order>(entity =>
            {
                entity.HasKey(e => e.Id);
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
                entity.HasKey(e => e.Id);
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
                entity.HasKey(e => e.Id);
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
                entity.HasKey(e => e.Id);
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


            #endregion

            #region Customer Engagement Configuration
            // Review Configuration
            modelBuilder.Entity<Review>(entity =>
            {
                entity.HasKey(e => e.Id);
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
                entity.HasKey(e => e.Id);
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
            //modelBuilder.Entity<Brand>().HasData(
            //    new Brand { Id = 1, Name = "Adidas", Description = "Impossible is Nothing", IsActive = true },
            //    new Brand { Id = 2, Name = "Adidas Originals", Description = "Original is Never Finished", IsActive = true },
            //    new Brand { Id = 3, Name = "Adidas Performance", Description = "Nothing is Impossible", IsActive = true }
            //);

            // Seed Categories
            //modelBuilder.Entity<Category>().HasData(
            //    new Category { Id = 1, Name = "Footwear", Slug = "footwear", IsActive = true, SortOrder = 1 },
            //    new Category { Id = 2, Name = "Clothing", Slug = "clothing", IsActive = true, SortOrder = 2 },
            //    new Category { Id = 3, Name = "Accessories", Slug = "accessories", IsActive = true, SortOrder = 3 },
            //    new Category { Id = 4, Name = "Running Shoes", Slug = "running-shoes", ParentCategoryId = 1, IsActive = true, SortOrder = 1 },
            //    new Category { Id = 5, Name = "Lifestyle Shoes", Slug = "lifestyle-shoes", ParentCategoryId = 1, IsActive = true, SortOrder = 2 },
            //    new Category { Id = 6, Name = "Football Boots", Slug = "football-boots", ParentCategoryId = 1, IsActive = true, SortOrder = 3 }
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