using Adidas.Models.Operation;
using System.Text.Json;
using Models.Feature;

namespace Adidas.Context.Seeds
{
    public static class AdidasDbContextSeed
    {
        public static async Task SeedAsync(AdidasDbContext context)
        {
            if (!context.Orders.Any())
            {
                var ordersData = await File.ReadAllTextAsync("../Adidas.Context/Data/Orders.json");
                var orders = JsonSerializer.Deserialize<List<Order>>(ordersData);
                if (orders?.Count > 0)
                {
                    await context.Orders.AddRangeAsync(orders);
                    await context.SaveChangesAsync();
                }

            }
            if (!context.OrderItems.Any())
            {
                var orderItemsData = await File.ReadAllTextAsync("../Adidas.Context/Data/OrderItems.json");
                var orderItems = JsonSerializer.Deserialize<List<OrderItem>>(orderItemsData);
                if (orderItems?.Count > 0)
                {
                    await context.OrderItems.AddRangeAsync(orderItems);
                    await context.SaveChangesAsync();
                }
            }
            if (!context.Payments.Any())
            {
                var paymentsData = await File.ReadAllTextAsync("../Adidas.Context/Data/Payments.json");
                var payments = JsonSerializer.Deserialize<List<Payment>>(paymentsData);
                if (payments?.Count > 0)
                {
                    await context.Payments.AddRangeAsync(payments);
                    await context.SaveChangesAsync();
                }
            }
            if (!context.Reviews.Any())
            {
                var reviewsData = await File.ReadAllTextAsync("../Adidas.Context/Data/Reviews.json");
                var reviews = JsonSerializer.Deserialize<List<Review>>(reviewsData);
                if (reviews?.Count > 0)
                {
                    await context.Reviews.AddRangeAsync(reviews);
                    await context.SaveChangesAsync();
                }
            }
            if (!context.Coupons.Any())
            {
                var couponsData = await File.ReadAllTextAsync("../Adidas.Context/Data/Coupons.json");
                var coupons = JsonSerializer.Deserialize<List<Coupon>>(couponsData);
                if (coupons?.Count > 0)
                {
                    await context.Coupons.AddRangeAsync(coupons);
                    await context.SaveChangesAsync();
                } 
            }
        }
            
    }   
}
