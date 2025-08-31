using AutoMapper;
using System.Collections.Generic;
using TaskHub.Api.Models.Dtos.Cards;
using CardEntity = TaskHub.Api.Models.Entities.Cards.Card;

namespace TaskHub.Api.Infrastructure.Mapping
{
    public class CardProfile : Profile
    {
        public CardProfile()
        {
            CreateMap<CardEntity, CardDto>();
            CreateMap<CreateCardDto, CardEntity>()
                .ForMember(dest => dest.Metadata, opt => opt.MapFrom(src => new Dictionary<string, object>()));
            CreateMap<UpdateCardDto, CardEntity>()
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));
        }
    }
}
