using Adidas.Application.Contracts.RepositoriesContracts.Operation;
using Adidas.Application.Contracts.ServicesContracts.Operation;
using Adidas.DTOs.CommonDTOs;
using Adidas.DTOs.Operation.OrderDTOs;
using Adidas.Models.Operation;
using Mapster;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace Adidas.Application.Services.Operation
{
    public class OrderEditService : IOrderEditService
    {
        private readonly IOrderRepository _orderRepository;
        private readonly ILogger<OrderEditService> _logger;

        public OrderEditService(IOrderRepository orderRepository, ILogger<OrderEditService> logger)
        {
            _orderRepository = orderRepository;
            _logger = logger;
        }

        //public async Task<OperationResult<OrderDto>> EditAsync(OrderUpdateDto dto)
        //{
        //    try
        //    {
        //        var order = await _orderRepository.GetByIdAsync(dto.Id);
        //        if (order == null)
        //            return OperationResult<OrderDto>.Fail("Order not found.");

        //        // Map only non-null fields
        //        if (dto.OrderStatus.HasValue) order.OrderStatus = dto.OrderStatus.Value;
        //        if (dto.Subtotal.HasValue) order.Subtotal = dto.Subtotal.Value;
        //        if (dto.TaxAmount.HasValue) order.TaxAmount = dto.TaxAmount.Value;
        //        if (dto.ShippingAmount.HasValue) order.ShippingAmount = dto.ShippingAmount.Value;
        //        if (dto.DiscountAmount.HasValue) order.DiscountAmount = dto.DiscountAmount.Value;
        //        if (dto.TotalAmount.HasValue) order.TotalAmount = dto.TotalAmount.Value;
        //        if (!string.IsNullOrEmpty(dto.Currency)) order.Currency = dto.Currency;
        //        if (dto.OrderDate.HasValue) order.OrderDate = dto.OrderDate.Value;
        //        if (dto.ShippedDate.HasValue) order.ShippedDate = dto.ShippedDate;
        //        if (dto.DeliveredDate.HasValue) order.DeliveredDate = dto.DeliveredDate;
        //        if (!string.IsNullOrEmpty(dto.Notes)) order.Notes = dto.Notes;

        //        // Ensure JSON string is stored
        //        if (!string.IsNullOrWhiteSpace(dto.ShippingAddress))
        //            order.ShippingAddress = dto.ShippingAddress;
        //        if (!string.IsNullOrWhiteSpace(dto.BillingAddress))
        //            order.BillingAddress = dto.BillingAddress;

        //        await _orderRepository.UpdateAsync(order);
        //        await _orderRepository.SaveChangesAsync();

        //        return OperationResult<OrderDto>.Success(order.Adapt<OrderDto>());
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, "Error editing order");
        //        return OperationResult<OrderDto>.Fail("An error occurred while editing the order.");
        //    }
        //}


            public async Task<OperationResult<OrderDto>> EditAsync(OrderUpdateDto dto, string changedByUser)
            {
                try
                {
                    var order = await _orderRepository.GetByIdAsync(dto.Id);
                    if (order == null)
                        return OperationResult<OrderDto>.Fail("Order not found.");

                    // ✅ Status change tracking
                    if (dto.OrderStatus.HasValue && order.OrderStatus != dto.OrderStatus.Value)
                    {
                        order.OrderStatus = dto.OrderStatus.Value;
                        order.LastStatusUpdatedBy = changedByUser;
                        //order.LastStatusUpdatedAt = DateTime.UtcNow;
                    }

                    // ✅ Update other fields if provided
                    if (!string.IsNullOrWhiteSpace(dto.OrderNumber))
                        order.OrderNumber = dto.OrderNumber;

                    if (dto.Subtotal.HasValue)
                        order.Subtotal = dto.Subtotal.Value;

                    if (dto.TaxAmount.HasValue)
                        order.TaxAmount = dto.TaxAmount.Value;

                    if (dto.ShippingAmount.HasValue)
                        order.ShippingAmount = dto.ShippingAmount.Value;

                    if (dto.DiscountAmount.HasValue)
                        order.DiscountAmount = dto.DiscountAmount.Value;

                    if (dto.TotalAmount.HasValue)
                        order.TotalAmount = dto.TotalAmount.Value;

                    if (!string.IsNullOrWhiteSpace(dto.Currency))
                        order.Currency = dto.Currency;

                    if (dto.OrderDate.HasValue)
                        order.OrderDate = dto.OrderDate.Value;

                    if (dto.ShippedDate.HasValue)
                        order.ShippedDate = dto.ShippedDate;

                    if (dto.DeliveredDate.HasValue)
                        order.DeliveredDate = dto.DeliveredDate;

                    if (!string.IsNullOrWhiteSpace(dto.Notes))
                        order.Notes = dto.Notes;

                    // ✅ Validate and store JSON addresses
                    if (!string.IsNullOrWhiteSpace(dto.ShippingAddress))
                    {
                        try { JsonDocument.Parse(dto.ShippingAddress); }
                        catch { return OperationResult<OrderDto>.Fail("Invalid Shipping Address JSON."); }
                        order.ShippingAddress = dto.ShippingAddress;
                    }

                    if (!string.IsNullOrWhiteSpace(dto.BillingAddress))
                    {
                        try { JsonDocument.Parse(dto.BillingAddress); }
                        catch { return OperationResult<OrderDto>.Fail("Invalid Billing Address JSON."); }
                        order.BillingAddress = dto.BillingAddress;
                    }

                    await _orderRepository.UpdateAsync(order);
                    await _orderRepository.SaveChangesAsync();

                    return OperationResult<OrderDto>.Success(order.Adapt<OrderDto>());
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error editing order {OrderId}", dto.Id);
                    return OperationResult<OrderDto>.Fail("An error occurred while editing the order.");
                }
            }
        }
    }



