using AutoMapper;
using TaskHub.Api.Models.Dtos.Boards;
using BoardEntity = TaskHub.Api.Models.Entities.Boards.Board;

namespace TaskHub.Api.Infrastructure.Mapping
{
    public class BoardProfile : Profile
    {
        public BoardProfile()
        {
                        CreateMap<BoardEntity, BoardDto>()
                .ForMember(dest => dest.Members, opt => opt.MapFrom(src => src.BoardMembers));
            CreateMap<CreateBoardDto, BoardEntity>();
            CreateMap<UpdateBoardDto, BoardEntity>()
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));
        }
    }
}
