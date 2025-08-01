using AutoMapper;

namespace Adidas.Application.Map
{
    public abstract class BaseMappingProfile : Profile
    {
        protected BaseMappingProfile()
        {
            // Common mappings can be placed here
            // SourceMemberNamingConvention = new LowerUnderscoreNamingConvention();
            // DestinationMemberNamingConvention = new PascalCaseNamingConvention();
        }
    }
}
