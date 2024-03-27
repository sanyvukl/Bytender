using AutoMapper;

namespace SearchService.RequestHelpers;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<Contracts.AuctionCreated, Models.Item>().ReverseMap();
        CreateMap<Contracts.AuctionUpdated, Models.Item>().ReverseMap();
    }
}
