//
//
//
// using Adidas.DTOs.Common_DTOs;
// using Adidas.DTOs.Operation.OrderDTOs;
// using Adidas.Models.Operation;
//
// namespace Adidas.AdminDashboardMVC.Controllers.Orders;
//
// public class FakeOrders
// {
//     public static PagedResultDto<OrderDto> GetFakePagedOrders(int pageNumber = 1, int pageSize = 10)
//     {
//         var pagedOrders = new PagedResultDto<OrderDto>();
//         pagedOrders.Items = new List<OrderDto>();
//         pagedOrders.PageNumber = pageNumber;
//         pagedOrders.PageSize = pageSize;
//
//         pagedOrders.Items = new List<OrderDto>()
//         {
//             new OrderDto()
//             {
//                 Id = Guid.NewGuid(),
//                 OrderDate = DateTime.UtcNow,
//                 OrderNumber = "123",
//                 OrderStatus = OrderStatus.Pending,
//                 TotalAmount = 100,
//                 UserId = "dfakljdsa",
//                 ShippingAddress = new Dictionary<string, object>()
//                 {
//                     { "address", "123 Main St, Anytown, USA" },
//                     { "city", "Anytown" },
//                     { "state", "CA" },
//                     { "zip", "12345" }
//                 },
//             },
//             new OrderDto()
//             {
//                 Id = Guid.NewGuid(),
//                 OrderDate = DateTime.UtcNow,
//                 OrderNumber = "123",
//                 OrderStatus = OrderStatus.Pending,
//                 TotalAmount = 100,
//                 UserId = "dfakljdsa",
//                 ShippingAddress = new Dictionary<string, object>()
//                 {
//                     { "address", "123 Main St, Anytown, USA" },
//                     { "city", "Anytown" },
//                     { "state", "CA" },
//                     { "zip", "12345" }
//                 },
//             },
//             new OrderDto()
//             {
//                 Id = Guid.NewGuid(),
//                 OrderDate = DateTime.UtcNow,
//                 OrderNumber = "123",
//                 OrderStatus = OrderStatus.Pending,
//                 TotalAmount = 100,
//                 UserId = "dfakljdsa",
//                 ShippingAddress = new Dictionary<string, object>()
//                 {
//                     { "address", "123 Main St, Anytown, USA" },
//                     { "city", "Anytown" },
//                     { "state", "CA" },
//                     { "zip", "12345" }
//                 },
//             },
//             new OrderDto()
//             {
//                 Id = Guid.NewGuid(),
//                 OrderDate = DateTime.UtcNow,
//                 OrderNumber = "123",
//                 OrderStatus = OrderStatus.Pending,
//                 TotalAmount = 100,
//                 UserId = "dfakljdsa",
//                 ShippingAddress = new Dictionary<string, object>()
//                 {
//                     { "address", "123 Main St, Anytown, USA" },
//                     { "city", "Anytown" },
//                     { "state", "CA" },
//                     { "zip", "12345" }
//                 },
//             },
//             new OrderDto()
//             {
//                 Id = Guid.NewGuid(),
//                 OrderDate = DateTime.UtcNow,
//                 OrderNumber = "123",
//                 OrderStatus = OrderStatus.Pending,
//                 TotalAmount = 100,
//                 UserId = "dfakljdsa",
//                 ShippingAddress = new Dictionary<string, object>()
//                 {
//                     { "address", "123 Main St, Anytown, USA" },
//                     { "city", "Anytown" },
//                     { "state", "CA" },
//                     { "zip", "12345" }
//                 },
//             },
//             new OrderDto()
//             {
//                 Id = Guid.NewGuid(),
//                 OrderDate = DateTime.UtcNow,
//                 OrderNumber = "123",
//                 OrderStatus = OrderStatus.Pending,
//                 TotalAmount = 100,
//                 UserId = "dfakljdsa",
//                 ShippingAddress = new Dictionary<string, object>()
//                 {
//                     { "address", "123 Main St, Anytown, USA" },
//                     { "city", "Anytown" },
//                     { "state", "CA" },
//                     { "zip", "12345" }
//                 },
//             },
//             new OrderDto()
//             {
//                 Id = Guid.NewGuid(),
//                 OrderDate = DateTime.UtcNow,
//                 OrderNumber = "123",
//                 OrderStatus = OrderStatus.Pending,
//                 TotalAmount = 100,
//                 UserId = "dfakljdsa",
//                 ShippingAddress = new Dictionary<string, object>()
//                 {
//                     { "address", "123 Main St, Anytown, USA" },
//                     { "city", "Anytown" },
//                     { "state", "CA" },
//                     { "zip", "12345" }
//                 },
//             },
//             new OrderDto()
//             {
//                 Id = Guid.NewGuid(),
//                 OrderDate = DateTime.UtcNow,
//                 OrderNumber = "123",
//                 OrderStatus = OrderStatus.Pending,
//                 TotalAmount = 100,
//                 UserId = "dfakljdsa",
//                 ShippingAddress = new Dictionary<string, object>()
//                 {
//                     { "address", "123 Main St, Anytown, USA" },
//                     { "city", "Anytown" },
//                     { "state", "CA" },
//                     { "zip", "12345" }
//                 },
//             },
//             new OrderDto()
//             {
//                 Id = Guid.NewGuid(),
//                 OrderDate = DateTime.UtcNow,
//                 OrderNumber = "123",
//                 OrderStatus = OrderStatus.Pending,
//                 TotalAmount = 100,
//                 UserId = "dfakljdsa",
//                 ShippingAddress = new Dictionary<string, object>()
//                 {
//                     { "address", "123 Main St, Anytown, USA" },
//                     { "city", "Anytown" },
//                     { "state", "CA" },
//                     { "zip", "12345" }
//                 },
//             },
//             new OrderDto()
//             {
//                 Id = Guid.NewGuid(),
//                 OrderDate = DateTime.UtcNow,
//                 OrderNumber = "123",
//                 OrderStatus = OrderStatus.Pending,
//                 TotalAmount = 100,
//                 UserId = "dfakljdsa",
//                 ShippingAddress = new Dictionary<string, object>()
//                 {
//                     { "address", "123 Main St, Anytown, USA" },
//                     { "city", "Anytown" },
//                     { "state", "CA" },
//                     { "zip", "12345" }
//                 },
//             },
//             new OrderDto()
//             {
//                 Id = Guid.NewGuid(),
//                 OrderDate = DateTime.UtcNow,
//                 OrderNumber = "123",
//                 OrderStatus = OrderStatus.Pending,
//                 TotalAmount = 100,
//                 UserId = "dfakljdsa",
//                 ShippingAddress = new Dictionary<string, object>()
//                 {
//                     { "address", "123 Main St, Anytown, USA" },
//                     { "city", "Anytown" },
//                     { "state", "CA" },
//                     { "zip", "12345" }
//                 },
//             },
//             new OrderDto()
//             {
//                 Id = Guid.NewGuid(),
//                 OrderDate = DateTime.UtcNow,
//                 OrderNumber = "123",
//                 OrderStatus = OrderStatus.Pending,
//                 TotalAmount = 100,
//                 UserId = "dfakljdsa",
//                 ShippingAddress = new Dictionary<string, object>()
//                 {
//                     { "address", "123 Main St, Anytown, USA" },
//                     { "city", "Anytown" },
//                     { "state", "CA" },
//                     { "zip", "12345" }
//                 },
//             },
//             new OrderDto()
//             {
//                 Id = Guid.NewGuid(),
//                 OrderDate = DateTime.UtcNow,
//                 OrderNumber = "123",
//                 OrderStatus = OrderStatus.Pending,
//                 TotalAmount = 100,
//                 UserId = "dfakljdsa",
//                 ShippingAddress = new Dictionary<string, object>()
//                 {
//                     { "address", "123 Main St, Anytown, USA" },
//                     { "city", "Anytown" },
//                     { "state", "CA" },
//                     { "zip", "12345" }
//                 },
//             },
//             new OrderDto()
//             {
//                 Id = Guid.NewGuid(),
//                 OrderDate = DateTime.UtcNow,
//                 OrderNumber = "123",
//                 OrderStatus = OrderStatus.Pending,
//                 TotalAmount = 100,
//                 UserId = "dfakljdsa",
//                 ShippingAddress = new Dictionary<string, object>()
//                 {
//                     { "address", "123 Main St, Anytown, USA" },
//                     { "city", "Anytown" },
//                     { "state", "CA" },
//                     { "zip", "12345" }
//                 },
//             },
//             new OrderDto()
//             {
//                 Id = Guid.NewGuid(),
//                 OrderDate = DateTime.UtcNow,
//                 OrderNumber = "123",
//                 OrderStatus = OrderStatus.Pending,
//                 TotalAmount = 100,
//                 UserId = "dfakljdsa",
//                 ShippingAddress = new Dictionary<string, object>()
//                 {
//                     { "address", "123 Main St, Anytown, USA" },
//                     { "city", "Anytown" },
//                     { "state", "CA" },
//                     { "zip", "12345" }
//                 },
//             },
//             new OrderDto()
//             {
//                 Id = Guid.NewGuid(),
//                 OrderDate = DateTime.UtcNow,
//                 OrderNumber = "123",
//                 OrderStatus = OrderStatus.Pending,
//                 TotalAmount = 100,
//                 UserId = "dfakljdsa",
//                 ShippingAddress = new Dictionary<string, object>()
//                 {
//                     { "address", "123 Main St, Anytown, USA" },
//                     { "city", "Anytown" },
//                     { "state", "CA" },
//                     { "zip", "12345" }
//                 },
//             },
//             new OrderDto()
//             {
//                 Id = Guid.NewGuid(),
//                 OrderDate = DateTime.UtcNow,
//                 OrderNumber = "123",
//                 OrderStatus = OrderStatus.Pending,
//                 TotalAmount = 100,
//                 UserId = "dfakljdsa",
//                 ShippingAddress = new Dictionary<string, object>()
//                 {
//                     { "address", "123 Main St, Anytown, USA" },
//                     { "city", "Anytown" },
//                     { "state", "CA" },
//                     { "zip", "12345" }
//                 },
//             },
//             new OrderDto()
//             {
//                 Id = Guid.NewGuid(),
//                 OrderDate = DateTime.UtcNow,
//                 OrderNumber = "123",
//                 OrderStatus = OrderStatus.Pending,
//                 TotalAmount = 100,
//                 UserId = "dfakljdsa",
//                 ShippingAddress = new Dictionary<string, object>()
//                 {
//                     { "address", "123 Main St, Anytown, USA" },
//                     { "city", "Anytown" },
//                     { "state", "CA" },
//                     { "zip", "12345" }
//                 },
//             },
//             new OrderDto()
//             {
//                 Id = Guid.NewGuid(),
//                 OrderDate = DateTime.UtcNow,
//                 OrderNumber = "123",
//                 OrderStatus = OrderStatus.Pending,
//                 TotalAmount = 100,
//                 UserId = "dfakljdsa",
//                 ShippingAddress = new Dictionary<string, object>()
//                 {
//                     { "address", "123 Main St, Anytown, USA" },
//                     { "city", "Anytown" },
//                     { "state", "CA" },
//                     { "zip", "12345" }
//                 },
//             },
//             new OrderDto()
//             {
//                 Id = Guid.NewGuid(),
//                 OrderDate = DateTime.UtcNow,
//                 OrderNumber = "123",
//                 OrderStatus = OrderStatus.Pending,
//                 TotalAmount = 100,
//                 UserId = "dfakljdsa",
//                 ShippingAddress = new Dictionary<string, object>()
//                 {
//                     { "address", "123 Main St, Anytown, USA" },
//                     { "city", "Anytown" },
//                     { "state", "CA" },
//                     { "zip", "12345" }
//                 },
//             },
//             new OrderDto()
//             {
//                 Id = Guid.NewGuid(),
//                 OrderDate = DateTime.UtcNow,
//                 OrderNumber = "123",
//                 OrderStatus = OrderStatus.Pending,
//                 TotalAmount = 100,
//                 UserId = "dfakljdsa",
//                 ShippingAddress = new Dictionary<string, object>()
//                 {
//                     { "address", "123 Main St, Anytown, USA" },
//                     { "city", "Anytown" },
//                     { "state", "CA" },
//                     { "zip", "12345" }
//                 },
//             },
//             new OrderDto()
//             {
//                 Id = Guid.NewGuid(),
//                 OrderDate = DateTime.UtcNow,
//                 OrderNumber = "123",
//                 OrderStatus = OrderStatus.Pending,
//                 TotalAmount = 100,
//                 UserId = "dfakljdsa",
//                 ShippingAddress = new Dictionary<string, object>()
//                 {
//                     { "address", "123 Main St, Anytown, USA" },
//                     { "city", "Anytown" },
//                     { "state", "CA" },
//                     { "zip", "12345" }
//                 },
//             },
//             new OrderDto()
//             {
//                 Id = Guid.NewGuid(),
//                 OrderDate = DateTime.UtcNow,
//                 OrderNumber = "123",
//                 OrderStatus = OrderStatus.Pending,
//                 TotalAmount = 100,
//                 UserId = "dfakljdsa",
//                 ShippingAddress = new Dictionary<string, object>()
//                 {
//                     { "address", "123 Main St, Anytown, USA" },
//                     { "city", "Anytown" },
//                     { "state", "CA" },
//                     { "zip", "12345" }
//                 },
//             },
//             new OrderDto()
//             {
//                 Id = Guid.NewGuid(),
//                 OrderDate = DateTime.UtcNow,
//                 OrderNumber = "123",
//                 OrderStatus = OrderStatus.Pending,
//                 TotalAmount = 100,
//                 UserId = "dfakljdsa",
//                 ShippingAddress = new Dictionary<string, object>()
//                 {
//                     { "address", "123 Main St, Anytown, USA" },
//                     { "city", "Anytown" },
//                     { "state", "CA" },
//                     { "zip", "12345" }
//                 },
//             },
//             new OrderDto()
//             {
//                 Id = Guid.NewGuid(),
//                 OrderDate = DateTime.UtcNow,
//                 OrderNumber = "123",
//                 OrderStatus = OrderStatus.Pending,
//                 TotalAmount = 100,
//                 UserId = "dfakljdsa",
//                 ShippingAddress = new Dictionary<string, object>()
//                 {
//                     { "address", "123 Main St, Anytown, USA" },
//                     { "city", "Anytown" },
//                     { "state", "CA" },
//                     { "zip", "12345" }
//                 },
//             },
//             new OrderDto()
//             {
//                 Id = Guid.NewGuid(),
//                 OrderDate = DateTime.UtcNow,
//                 OrderNumber = "123",
//                 OrderStatus = OrderStatus.Pending,
//                 TotalAmount = 100,
//                 UserId = "dfakljdsa",
//                 ShippingAddress = new Dictionary<string, object>()
//                 {
//                     { "address", "123 Main St, Anytown, USA" },
//                     { "city", "Anytown" },
//                     { "state", "CA" },
//                     { "zip", "12345" }
//                 },
//             },
//             new OrderDto()
//             {
//                 Id = Guid.NewGuid(),
//                 OrderDate = DateTime.UtcNow,
//                 OrderNumber = "123",
//                 OrderStatus = OrderStatus.Pending,
//                 TotalAmount = 100,
//                 UserId = "dfakljdsa",
//                 ShippingAddress = new Dictionary<string, object>()
//                 {
//                     { "address", "123 Main St, Anytown, USA" },
//                     { "city", "Anytown" },
//                     { "state", "CA" },
//                     { "zip", "12345" }
//                 },
//             },
//             new OrderDto()
//             {
//                 Id = Guid.NewGuid(),
//                 OrderDate = DateTime.UtcNow,
//                 OrderNumber = "123",
//                 OrderStatus = OrderStatus.Pending,
//                 TotalAmount = 100,
//                 UserId = "dfakljdsa",
//                 ShippingAddress = new Dictionary<string, object>()
//                 {
//                     { "address", "123 Main St, Anytown, USA" },
//                     { "city", "Anytown" },
//                     { "state", "CA" },
//                     { "zip", "12345" }
//                 },
//             },
//         };
//         pagedOrders.TotalCount = pagedOrders.Items.Count();
//         pagedOrders.TotalPages = (int)Math.Ceiling((double)pagedOrders.TotalCount / pageSize);
//         return pagedOrders;
//     }
// }