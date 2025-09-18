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

        public async Task<double> GetMaxPositionAsync(Guid listId)
        {
            // Materialize then compute max to avoid provider translation issues and
            // return -1 when there are no cards so first insert becomes position 0
            var positions = await _context.Cards
                .Where(c => c.ListId == listId)
                .Select(c => c.Position)
                .ToListAsync();

            return positions.Count == 0 ? -1.0 : positions.Max();
        }

        public async Task UpdateCardPositionAsync(Guid cardId, double newPosition)
        {
            var card = await _context.Cards.FindAsync(cardId);
            if (card != null)
            {
                card.Position = newPosition;
            }
        }

        public async Task ReorderCardsAfterDeletion(Guid listId, double deletedPosition)
        {
            var cardsToUpdate = await _context.Cards
                .Where(c => c.ListId == listId && c.Position > deletedPosition)
                .ToListAsync();

            foreach (var card in cardsToUpdate)
            {
                card.Position--;
            }
        }

        public async Task ReorderCardsAfterInsertion(Guid listId, double insertedPosition)
        {
            // Shift cards at and after the inserted position up by 1 to make room
            var cardsToUpdate = await _context.Cards
                .Where(c => c.ListId == listId && c.Position >= insertedPosition)
                .ToListAsync();

            foreach (var card in cardsToUpdate)
            {
                card.Position++;
            }
        }
        public async Task DeleteCardsByListIdAsync(Guid listId)
        {
            var cardsToDelete = await _context.Cards
                .Where(c => c.ListId == listId)
                .ToListAsync();

            if (cardsToDelete.Any())
            {
                _context.Cards.RemoveRange(cardsToDelete);
            }
        }

    }
}
