using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CardEntity = TaskHub.Api.Models.Entities.Cards.Card;

namespace TaskHub.Api.Core.Interfaces.Repositories
{
    public interface ICardRepository : IRepository<CardEntity>
    {
        Task<IEnumerable<CardEntity>> GetCardsByListIdAsync(Guid listId);
        Task<bool> ListExistsAsync(Guid listId);
        Task<double> GetMaxPositionAsync(Guid listId);
        Task UpdateCardPositionAsync(Guid cardId, double newPosition);
        Task ReorderCardsAfterDeletion(Guid listId, double deletedPosition);
        Task ReorderCardsAfterInsertion(Guid listId, double insertedPosition);
        Task DeleteCardsByListIdAsync(Guid listId);
    }
}
