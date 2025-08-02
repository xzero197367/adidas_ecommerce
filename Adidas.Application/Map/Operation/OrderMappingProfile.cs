namespace Adidas.Application.Map.Operation;

public class OrderMappingProfile: BaseMappingProfile
{
    public OrderMappingProfile()
    {
        
        //CreateMap<Order, OrderSummaryDto>()
        //    .ForMember(dest => dest.TotalSales, opt => opt.Ignore()) // Requires aggregation
        //    .ForMember(dest => dest.TotalOrders, opt => opt.Ignore())
        //    .ForMember(dest => dest.AverageOrderValue, opt => opt.Ignore())
        //    .ForMember(dest => dest.OrdersByStatus, opt => opt.Ignore());
        
    }
}