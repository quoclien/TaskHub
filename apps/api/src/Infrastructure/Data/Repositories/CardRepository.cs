using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TaskHub.Api.Core.Interfaces;
using TaskHub.Api.Core.Interfaces.Repositories;
using TaskHub.Api.Infrastructure.Data;
using CardEntity = TaskHub.Api.Models.Entities.Cards.Card;

namespace TaskHub.Api.Infrastructure.Data.Repositories
{
    public class CardRepository : Repository<CardEntity>, ICardRepository
    {
        public CardRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
        {
        }



        public async Task<IEnumerable<CardEntity>> GetCardsByListIdAsync(Guid listId)
        {
            return await _context.Cards
                .Where(c => c.ListId == listId)
                .OrderBy(c => c.Position)
                .ToListAsync();
        }

        public async Task<bool> ListExistsAsync(Guid listId)
        {
            return await _context.Lists.AnyAsync(l => l.Id == listId);
        }

        public async Task<int> GetMaxPositionAsync(Guid listId)
        {
            var positions = await _context.Cards
                .Where(c => c.ListId == listId)
                .Select(c => c.Position)
                .ToListAsync(); // Materialize the query before Max()

            return positions.DefaultIfEmpty(0).Max();
        }

        public async Task ReorderCardsAsync(Guid listId, int fromPosition, int toPosition)
        {
            // Don't include cards at the target position since the moved card is already there
            var cardsToUpdate = await _context.Cards
                .Where(c => c.ListId == listId && c.Position != toPosition &&
                            ((fromPosition < toPosition && c.Position > fromPosition && c.Position <= toPosition) ||
                             (fromPosition > toPosition && c.Position >= toPosition && c.Position < fromPosition)))
                .ToListAsync();

            foreach (var card in cardsToUpdate)
            {
                if (fromPosition < toPosition)
                {
                    // Moving down: shift cards between fromPosition and toPosition down by 1
                    card.Position--;
                }
                else
                {
                    // Moving up: shift cards between toPosition and fromPosition up by 1
                    card.Position++;
                }
            }
        }

        public async Task ReorderCardsAfterDeletion(Guid listId, int deletedPosition)
        {
            var cardsToUpdate = await _context.Cards
                .Where(c => c.ListId == listId && c.Position > deletedPosition)
                .ToListAsync();

            foreach (var card in cardsToUpdate)
            {
                card.Position--;
            }
        }

        public async Task ReorderCardsAfterInsertion(Guid listId, int insertedPosition)
        {
            var cardsToUpdate = await _context.Cards
                .Where(c => c.ListId == listId && c.Position > insertedPosition)
                .ToListAsync();

            foreach (var card in cardsToUpdate)
            {
                card.Position++;
            }
        }


    }
}
