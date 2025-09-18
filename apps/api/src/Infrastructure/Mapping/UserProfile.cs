using AutoMapper;
using TaskHub.Api.Models.Dtos.Users;
using TaskHub.Api.Models.Entities.Users;

namespace TaskHub.Api.Infrastructure.Mapping
{
    public class UserProfile : Profile
    {
        public UserProfile()
        {
            CreateMap<User, UserDto>();
        }
    }
}