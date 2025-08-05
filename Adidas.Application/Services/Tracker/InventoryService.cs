
using Adidas.Application.Contracts.RepositoriesContracts;
using Adidas.Application.Contracts.RepositoriesContracts.Main;
using Adidas.Application.Contracts.ServicesContracts.Tracker;
using Adidas.DTOs.Tracker;
using Adidas.Models.Tracker;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Adidas.Application.Services.Tracker
{
    public class InventoryService : IInventoryService
    {
        private readonly IProductVariantRepository _variantRepository;
        private readonly IInventoryLogRepository _inventoryLogRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<InventoryService> _logger;

        public InventoryService(
            IProductVariantRepository variantRepository,
            IInventoryLogRepository inventoryLogRepository,
            IMapper mapper,
            ILogger<InventoryService> logger)
        {
            _variantRepository = variantRepository;
            _inventoryLogRepository = inventoryLogRepository;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<bool> ReserveStockAsync(Guid variantId, int quantity)
        {
            try
            {
                var variant = await _variantRepository.GetByIdAsync(variantId);
                if (variant == null || variant.StockQuantity < quantity)
                    return false;

                var oldQuantity = variant.StockQuantity;
                variant.StockQuantity -= quantity;
                await _variantRepository.UpdateAsync(variant);

                await LogInventoryChangeAsync(variantId, oldQuantity, variant.StockQuantity, "RESERVE", "Guid.Empty", $"Reserved {quantity} units");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error reserving stock for variant {VariantId}", variantId);
                return false;
            }
        }

        public async Task<bool> ReleaseStockAsync(Guid variantId, int quantity)
        {
            try
            {
                var variant = await _variantRepository.GetByIdAsync(variantId);
                if (variant == null) return false;

                var oldQuantity = variant.StockQuantity;
                variant.StockQuantity += quantity;
                await _variantRepository.UpdateAsync(variant);

                await LogInventoryChangeAsync(variantId, oldQuantity, variant.StockQuantity, "RELEASE", "Guid.Empty", $"Released {quantity} units");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error releasing stock for variant {VariantId}", variantId);
                return false;
            }
        }

        public async Task<bool> UpdateStockAsync(Guid variantId, int newStock)
        {
            try
            {
                var variant = await _variantRepository.GetByIdAsync(variantId);
                if (variant == null) return false;

                var oldQuantity = variant.StockQuantity;
                variant.StockQuantity = newStock;
                await _variantRepository.UpdateAsync(variant);

                await LogInventoryChangeAsync(variantId, oldQuantity, newStock, "UPDATE", "Guid.Empty", "Manual stock update");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating stock for variant {VariantId}", variantId);
                return false;
            }
        }

        public async Task<IEnumerable<LowStockAlertDto>> GetLowStockAlertsAsync(int threshold = 10)
        {
            var lowStockVariants = await _variantRepository.GetLowStockVariantsAsync(threshold);

            return lowStockVariants.Select(v => new LowStockAlertDto
            {
                VariantId = v.Id,
                ProductName = v.Product.Name,
                VariantDetails = $"{v.Color} - {v.Size}",
                CurrentStock = v.StockQuantity,
                ReorderLevel = threshold
            });
        }

        public async Task<InventoryReportDto> GenerateInventoryReportAsync()
        {
            var variantsList = _variantRepository.GetAll();

            var lowStockVariants = await variantsList.Where(v => v.StockQuantity <= 10).CountAsync();
            var outOfStockVariants = await variantsList.Where(v => v.StockQuantity == 0).CountAsync();

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

            return new InventoryReportDto
            {
                TotalProducts = variantsList.Select(v => v.ProductId).Distinct().Count(),
                TotalVariants = await variantsList.CountAsync(),
                LowStockVariants = lowStockVariants,
                OutOfStockVariants = outOfStockVariants,
                TotalInventoryValue = productStocks.Sum(ps => ps.InventoryValue),
                ProductStocks = productStocks
            };
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
