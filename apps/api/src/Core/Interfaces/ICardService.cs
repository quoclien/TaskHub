using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TaskHub.Api.Models.Dtos.Cards;

namespace TaskHub.Api.Core.Interfaces
{
    public interface ICardService
    {
        Task<IEnumerable<CardDto>> GetCardsByListIdAsync(Guid listId);
        Task<CardDto?> GetCardByIdAsync(Guid id);
        Task<CardDto> CreateCardAsync(Guid listId, CreateCardDto createCardDto);
        Task UpdateCardAsync(Guid id, UpdateCardDto updateCardDto);
        Task MoveCardAsync(Guid cardId, Guid targetListId, double newPosition);
        Task DeleteCardAsync(Guid id);
    }
}
