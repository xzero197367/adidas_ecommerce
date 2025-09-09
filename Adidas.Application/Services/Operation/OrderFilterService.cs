//// Adidas.Application.Services.Operation/OrderFilterService.cs
//using Adidas.Application.Contracts.RepositoriesContracts.Operation;
//using Adidas.Application.Contracts.ServicesContracts.Operation;
//using Adidas.DTOs.Common_DTOs;
//using Adidas.DTOs.Operation.OrderDTOs;
//using Adidas.Models.Operation;
//using Microsoft.EntityFrameworkCore;
//using System;
//using System.Linq;
//using System.Linq.Expressions;
//using System.Threading.Tasks;

//namespace Adidas.Application.Services.Operation
//{
//    public class OrderFilterService : IOrderFilterService
//    {
//        private readonly IOrderRepository _orderRepository;

//        public OrderFilterService(IOrderRepository orderRepository)
//        {
//            _orderRepository = orderRepository;
//        }

//        public async Task<PagedResultDto<OrderDto>> GetFilteredOrdersAsync(
//            int pageNumber, int pageSize, ExtendedOrderFilterDto filter)
//        {
//            Expression<Func<Order, bool>> predicate = o =>
//                (string.IsNullOrEmpty(filter.OrderNumber) || o.OrderNumber.Contains(filter.OrderNumber)) &&
//                (!filter.OrderStatus.HasValue || o.OrderStatus == filter.OrderStatus.Value) &&
//                (!filter.StartDate.HasValue || o.OrderDate >= filter.StartDate.Value) &&
//                (!filter.EndDate.HasValue || o.OrderDate <= filter.EndDate.Value) &&
//                (!filter.IsGuest.HasValue || (
//                    filter.IsGuest.Value
//                        ? (o.User == null || string.IsNullOrEmpty(o.User.Email))   // guest
//                        : (o.User != null && !string.IsNullOrEmpty(o.User.Email))  // customer
//                ));

//            var (orders, totalCount) = await _orderRepository.GetPagedOrdersAsync(pageNumber, pageSize, predicate);

//            var items = orders.Select(o => new OrderDto
//            {
//                Id = o.Id,
//                OrderNumber = o.OrderNumber,
//                OrderStatus = o.OrderStatus,
//                TotalAmount = o.TotalAmount,
//                Currency = o.Currency,
//                OrderDate = o.OrderDate,
//                UserId = o.UserId,
//                UserName = o.User?.UserName ?? string.Empty,
//                UserEmail = o.User?.Email ?? string.Empty
//            }).ToList();

//            return new PagedResultDto<OrderDto>
//            {
//                Items = items,
//                TotalCount = totalCount
//            };
//        }
//    }
//}
// Fixed Adidas.Application.Services.Operation/OrderFilterService.cs
//using Adidas.Application.Contracts.RepositoriesContracts.Operation;
//using Adidas.Application.Contracts.ServicesContracts.Operation;
//using Adidas.DTOs.Common_DTOs;
//using Adidas.DTOs.Operation.OrderDTOs;
//using Adidas.Models.Operation;
//using Microsoft.EntityFrameworkCore;
//using System;
//using System.Linq;
//using System.Linq.Expressions;
//using System.Threading.Tasks;

//namespace Adidas.Application.Services.Operation
//{
//    public class OrderFilterService : IOrderFilterService
//    {
//        private readonly IOrderRepository _orderRepository;

//        public OrderFilterService(IOrderRepository orderRepository)
//        {
//            _orderRepository = orderRepository;
//        }

//        public async Task<PagedResultDto<OrderDto>> GetFilteredOrdersAsync(
//            int pageNumber, int pageSize, ExtendedOrderFilterDto filter)
//        {
//            Expression<Func<Order, bool>> predicate = o => !o.IsDeleted && // Always exclude deleted orders
//                (string.IsNullOrEmpty(filter.OrderNumber) ||
//                 o.OrderNumber.Contains(filter.OrderNumber) ||
//                 o.Id.ToString().Contains(filter.OrderNumber)) && // Search by ID as well
//                (!filter.OrderStatus.HasValue || o.OrderStatus == filter.OrderStatus.Value) &&
//                (!filter.StartDate.HasValue || o.OrderDate.Date >= filter.StartDate.Value.Date) &&
//                (!filter.EndDate.HasValue || o.OrderDate.Date <= filter.EndDate.Value.Date) &&
//                (!filter.IsGuest.HasValue || (
//                    filter.IsGuest.Value
//                        ? (o.User == null || string.IsNullOrEmpty(o.User.Email) || o.User.IsDeleted)   // guest or deleted user
//                        : (o.User != null && !string.IsNullOrEmpty(o.User.Email) && !o.User.IsDeleted)  // active customer
//                ));

//            var (orders, totalCount) = await _orderRepository.GetPagedOrdersAsync(pageNumber, pageSize, predicate);

//            var items = orders.Select(o => new OrderDto
//            {
//                Id = o.Id,
//                OrderNumber = o.OrderNumber,
//                OrderStatus = o.OrderStatus,
//                Subtotal = o.Subtotal,
//                TaxAmount = o.TaxAmount,
//                ShippingAmount = o.ShippingAmount,
//                DiscountAmount = o.DiscountAmount,
//                TotalAmount = o.TotalAmount,
//                Currency = o.Currency,
//                OrderDate = o.OrderDate,
//                ShippedDate = o.ShippedDate,
//                DeliveredDate = o.DeliveredDate,
//                Notes = o.Notes,
//                UserId = o.UserId,
//                UserName = o.User?.UserName ?? "Guest",
//                UserEmail = o.User?.Email ?? "N/A",
//                CreatedDate = o.CreatedDate,
//                ModifiedDate = o.ModifiedDate
//            }).ToList();

//            return new PagedResultDto<OrderDto>
//            {
//                Items = items,
//                TotalCount = totalCount
//            };
//        }
//    }
//}
// Fixed Adidas.Application.Services.Operation/OrderFilterService.cs
using Adidas.Application.Contracts.RepositoriesContracts.Operation;
using Adidas.Application.Contracts.ServicesContracts.Operation;
using Adidas.DTOs.Common_DTOs;
using Adidas.DTOs.Operation.OrderDTOs;
using Adidas.Models.Operation;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Adidas.Application.Services.Operation
{
    public class OrderFilterService : IOrderFilterService
    {
        private readonly IOrderRepository _orderRepository;

        public OrderFilterService(IOrderRepository orderRepository)
        {
            _orderRepository = orderRepository;
        }

        public async Task<PagedResultDto<OrderDto>> GetFilteredOrdersAsync(
            int pageNumber, int pageSize, ExtendedOrderFilterDto filter)
        {
            Expression<Func<Order, bool>> predicate = o => !o.IsDeleted && // Always exclude deleted orders
                (string.IsNullOrEmpty(filter.OrderNumber) ||
                 o.OrderNumber.Contains(filter.OrderNumber) ||
                 o.Id.ToString().Contains(filter.OrderNumber)) && // Search by ID as well
                (!filter.OrderStatus.HasValue || o.OrderStatus == filter.OrderStatus.Value) &&
                (!filter.StartDate.HasValue || o.OrderDate.Date >= filter.StartDate.Value.Date) &&
                (!filter.EndDate.HasValue || o.OrderDate.Date <= filter.EndDate.Value.Date) &&
                (!filter.IsGuest.HasValue || (
                    filter.IsGuest.Value
                        ? (o.User == null || string.IsNullOrEmpty(o.User.Email) || o.User.IsDeleted)   // guest or deleted user
                        : (o.User != null && !string.IsNullOrEmpty(o.User.Email) && !o.User.IsDeleted)  // active customer
                ));

            var (orders, totalCount) = await _orderRepository.GetPagedOrdersAsync(pageNumber, pageSize, predicate);

            var items = orders.Select(o => new OrderDto
            {
                Id = o.Id,
                OrderNumber = o.OrderNumber,
                OrderStatus = o.OrderStatus,
                Subtotal = o.Subtotal,
                TaxAmount = o.TaxAmount,
                ShippingAmount = o.ShippingAmount,
                DiscountAmount = o.DiscountAmount,
                TotalAmount = o.TotalAmount,
                Currency = o.Currency,
                OrderDate = o.OrderDate,
                ShippedDate = o.ShippedDate,
                DeliveredDate = o.DeliveredDate,
                Notes = o.Notes,
                UserId = o.UserId,
                UserName = o.User?.UserName ?? "Guest",
                UserEmail = o.User?.Email ?? "N/A"
                // Note: CreatedDate and ModifiedDate removed as they don't exist on Order entity
                // They would come from BaseAuditableEntity if it has them
            }).ToList();

            return new PagedResultDto<OrderDto>
            {
                Items = items,
                TotalCount = totalCount
            };
        }
    }
}