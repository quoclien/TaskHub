using AutoMapper;
using TaskHub.Api.Shared.Models;
using TaskHub.Api.Shared.Models.Dtos;

namespace TaskHub.Api.Shared.Mapping
{
    public class BoardProfile : Profile
    {
        public BoardProfile()
        {
            // Map from Board to BoardDto
            CreateMap<Board, BoardDto>()
                .ForMember(dest => dest.Members, opt => opt.MapFrom(src => src.Members));

            // Map from BoardMember to BoardMemberDto
            CreateMap<BoardMember, BoardMemberDto>()
                .ForMember(dest => dest.Role, opt => opt.MapFrom(src => src.Role.ToString()));

            // Map from CreateBoardDto to Board
            CreateMap<CreateBoardDto, Board>();
            
            // Map from UpdateBoardDto to Board
            CreateMap<UpdateBoardDto, Board>()
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));
        }
    }
}
