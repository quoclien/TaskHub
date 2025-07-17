using AutoMapper;
using TaskHub.Api.Models.Dtos;
using TaskHub.Api.Models.Dtos.Boards;
using BoardEntity = TaskHub.Api.Models.Entities.Boards.Board;
using BoardMemberEntity = TaskHub.Api.Models.Entities.Boards.BoardMember;

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

            // BoardMember mapping
            CreateMap<BoardMemberEntity, BoardMemberDto>()
                .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.User.Username))
                .ForMember(dest => dest.UserEmail, opt => opt.MapFrom(src => src.User.Email))
                .ForMember(dest => dest.UserAvatar, opt => opt.MapFrom(src => src.User.AvatarUrl));
        }
    }
}
