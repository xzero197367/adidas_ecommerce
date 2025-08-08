

using Adidas.DTOs.Tracker;
using Adidas.Models.Tracker;
using Mapster;
namespace Adidas.Application.Mapping.Tracker;

public class InventoryLogMapConfig
{
    public static void Configure()
    {
        // Get DTO to Model
        TypeAdapterConfig.GlobalSettings.NewConfig<InventoryReportDto, InventoryLog>()
            .IgnoreNullValues(true);

        // Model to Get DTO
        TypeAdapterConfig.GlobalSettings.NewConfig<InventoryLog, InventoryReportDto>();

        // // Create DTO to Model
        // TypeAdapterConfig.GlobalSettings.NewConfig<InventoryLogCreateDto, InventoryLog>()
        //     .IgnoreNullValues(true);
        //
        // // Update DTO to Model
        // TypeAdapterConfig.GlobalSettings.NewConfig<InventoryLogUpdateDto, InventoryLog>()
        //     .IgnoreNullValues(true);
    }
}