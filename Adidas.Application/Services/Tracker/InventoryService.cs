
using Adidas.Application.Contracts.RepositoriesContracts.Main;
using Adidas.Application.Contracts.RepositoriesContracts.Tracker;
using Adidas.Application.Contracts.ServicesContracts.Tracker;
using Adidas.DTOs.CommonDTOs;
using Adidas.DTOs.Tracker;
using Adidas.Models.Tracker;
using MapsterMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Adidas.Application.Services.Tracker
{
    public class InventoryService : IInventoryService
    {
        private readonly IProductVariantRepository _variantRepository;
        private readonly IInventoryLogRepository _inventoryLogRepository;
        private readonly ILogger<InventoryService> _logger;

        public InventoryService(
            IProductVariantRepository variantRepository,
            IInventoryLogRepository inventoryLogRepository,
            ILogger<InventoryService> logger)
        {
            _variantRepository = variantRepository;
            _inventoryLogRepository = inventoryLogRepository;
            _logger = logger;
        }

        public async Task<OperationResult<bool>> ReserveStockAsync(Guid variantId, int quantity)
        {
            try
            {
                var variant = await _variantRepository.GetByIdAsync(variantId);
                if (variant == null || variant.StockQuantity < quantity)
                {
                    return OperationResult<bool>.Fail("Not enough stock");
                }

                var oldQuantity = variant.StockQuantity;
                variant.StockQuantity -= quantity;
                await _variantRepository.UpdateAsync(variant);

                await LogInventoryChangeAsync(variantId, oldQuantity, variant.StockQuantity, "RESERVE", "Guid.Empty", $"Reserved {quantity} units");
                return OperationResult<bool>.Success(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error reserving stock for variant {VariantId}", variantId);
                return OperationResult<bool>.Fail(ex.Message);
            }
        }

        public async Task<OperationResult<bool>> ReleaseStockAsync(Guid variantId, int quantity)
        {
            try
            {
                var variant = await _variantRepository.GetByIdAsync(variantId);
                if (variant == null)
                {
                    return OperationResult<bool>.Fail("Variant not found");
                }

                var oldQuantity = variant.StockQuantity;
                variant.StockQuantity += quantity;
                await _variantRepository.UpdateAsync(variant);

                await LogInventoryChangeAsync(variantId, oldQuantity, variant.StockQuantity, "RELEASE", "Guid.Empty", $"Released {quantity} units");
                return OperationResult<bool>.Success(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error releasing stock for variant {VariantId}", variantId);
                return OperationResult<bool>.Fail(ex.Message);
            }
        }

        public async Task<OperationResult<bool>> UpdateStockAsync(Guid variantId, int newStock)
        {
            try
            {
                var variant = await _variantRepository.GetByIdAsync(variantId);
                if (variant == null)
                {
                    return OperationResult<bool>.Fail("Variant not found");
                }

                var oldQuantity = variant.StockQuantity;
                variant.StockQuantity = newStock;
                await _variantRepository.UpdateAsync(variant);

                await LogInventoryChangeAsync(variantId, oldQuantity, newStock, "UPDATE", "Guid.Empty", "Manual stock update");
                return OperationResult<bool>.Success(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating stock for variant {VariantId}", variantId);
                return OperationResult<bool>.Fail(ex.Message);
            }
        }

        public async Task<OperationResult<IEnumerable<LowStockAlertDto>>> GetLowStockAlertsAsync(int threshold = 10)
        {
            try
            {
                var lowStockVariants = await _variantRepository.GetLowStockVariantsAsync(threshold);

                var result = lowStockVariants.Select(v => new LowStockAlertDto
                {
                    VariantId = v.Id,
                    ProductName = v.Product.Name,
                    VariantDetails = $"{v.Color} - {v.Size}",
                    CurrentStock = v.StockQuantity,
                    ReorderLevel = threshold
                });
                return OperationResult<IEnumerable<LowStockAlertDto>>.Success(result);
            }catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting low stock alerts");
                return OperationResult<IEnumerable<LowStockAlertDto>>.Fail(ex.Message);
            }
        }

        public async Task<OperationResult<InventoryReportDto>> GenerateInventoryReportAsync()
        {
            try
            {
                var allVariants = await _variantRepository.GetAll().ToListAsync();
                var variantsList = allVariants.ToList();

                var lowStockVariants = variantsList.Where(v => v.StockQuantity <= 10).Count();
                var outOfStockVariants = variantsList.Where(v => v.StockQuantity == 0).Count();

                var productStocks = variantsList
                    .GroupBy(v => v.Product)
                    .Select(g => new ProductStockDto
                    {
                        ProductId = g.Key.Id,
                        ProductName = g.Key.Name,
                        TotalStock = g.Sum(v => v.StockQuantity),
                        VariantCount = g.Count(),
                        InventoryValue = g.Sum(v => v.StockQuantity * (g.Key.SalePrice ?? g.Key.Price))
                    });

                var result = new InventoryReportDto
                {
                    TotalProducts = variantsList.Select(v => v.ProductId).Distinct().Count(),
                    TotalVariants = variantsList.Count,
                    LowStockVariants = lowStockVariants,
                    OutOfStockVariants = outOfStockVariants,
                    TotalInventoryValue = productStocks.Sum(ps => ps.InventoryValue),
                    ProductStocks = productStocks
                };
                return OperationResult<InventoryReportDto>.Success(result);
            }catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating inventory report");
                return OperationResult<InventoryReportDto>.Fail(ex.Message);
            }
        }

        public async Task LogInventoryChangeAsync(Guid variantId, int oldQuantity, int newQuantity, string changeType, string userId, string? reason = null)
        {
            var log = new InventoryLog
            {
                Id = Guid.NewGuid(),
                VariantId = variantId,
                PreviousStock = oldQuantity,
                NewStock = newQuantity,
                QuantityChange = newQuantity - oldQuantity,
                ChangeType = changeType,
                Reason = reason,
                AddedById = userId,
                CreatedAt = DateTime.UtcNow
            };

            await _inventoryLogRepository.AddAsync(log);
        }
    }
}
