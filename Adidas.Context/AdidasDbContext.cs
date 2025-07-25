using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System.Text.Json;
using Adidas.Context.Configurations.Feature;
using Adidas.Context.Configurations.People;
using Adidas.Models.Feature;
using Adidas.Models.Main;
using Adidas.Models.Operation;
using Adidas.Models.Separator;
using Adidas.Models.Tracker;
using Models.People;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

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
            modelBuilder.ApplyConfiguration(new ProductConfig());
            
            // Product Variant Configuration
            modelBuilder.ApplyConfiguration(new ProductVariantConfig());
     

            // Category Configuration
            modelBuilder.ApplyConfiguration(new CategoryConfig());

            // Product Images Configuration
            modelBuilder.ApplyConfiguration(new ProductImageConfig());
         
            #endregion

            #region User & Authentication Configuration
            // User Configuration
            modelBuilder.ApplyConfiguration(new UserConfig());

            // Address Configuration
            modelBuilder.ApplyConfiguration(new AddressConfig());
            
            #endregion

            #region Order Management Configuration
            // Order Configuration
            modelBuilder.ApplyConfiguration(new OrderConfig());
            
            // Order Item Configuration
            modelBuilder.ApplyConfiguration(new OrderItemConfig());
            
            // Shopping Cart Configuration
            modelBuilder.ApplyConfiguration(new ShoppingCartConfig());

            // Payment Configuration
            modelBuilder.ApplyConfiguration(new PaymentConfig());
            #endregion

            #region Customer Engagement Configuration
            // Review Configuration
            modelBuilder.ApplyConfiguration(new ReviewConfig());

            // Wishlist Configuration
            modelBuilder.ApplyConfiguration(new WishlistConfig());
            
            #endregion

            // Configure JSON conversions for complex properties
            ConfigureJsonProperties(modelBuilder);
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