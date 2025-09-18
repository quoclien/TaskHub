using AutoMapper;
using TaskHub.Api.Models.Dtos.Lists;
using ListEntity = TaskHub.Api.Models.Entities.Lists.List;

namespace TaskHub.Api.Infrastructure.Mapping
{
    public class ListProfile : Profile
    {
        public ListProfile()
        {
            CreateMap<ListEntity, ListDto>();
            CreateMap<CreateListDto, ListEntity>();
            CreateMap<UpdateListDto, ListEntity>()
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));
        }
    }
}
