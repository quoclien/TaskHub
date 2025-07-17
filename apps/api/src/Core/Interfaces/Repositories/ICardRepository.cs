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
        Task<int> GetMaxPositionAsync(Guid listId);
        Task ReorderCardsAsync(Guid listId, int fromPosition, int toPosition);
        Task ReorderCardsAfterDeletion(Guid listId, int deletedPosition);
        Task ReorderCardsAfterInsertion(Guid listId, int insertedPosition);
    }
}
