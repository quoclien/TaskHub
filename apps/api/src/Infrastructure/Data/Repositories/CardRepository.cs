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

        public async Task<CardEntity> GetByIdOrThrowAsync(Guid id)
        {
            var card = await _context.Cards.FindAsync(id);
            if (card == null)
            {
                throw new KeyNotFoundException($"Card with ID {id} not found.");
            }
            return card;
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
            return await _context.Cards
                .Where(c => c.ListId == listId)
                .Select(c => c.Position)
                .DefaultIfEmpty(0)
                .MaxAsync();
        }

        public async Task ReorderCardsAsync(Guid listId, int fromPosition, int toPosition)
        {
            var cardsToUpdate = await _context.Cards
                .Where(c => c.ListId == listId &&
                            (c.Position >= fromPosition && c.Position <= toPosition ||
                             c.Position <= fromPosition && c.Position >= toPosition))
                .ToListAsync();

            foreach (var card in cardsToUpdate)
            {
                if (card.Position == fromPosition)
                {
                    card.Position = toPosition;
                }
                else if (fromPosition < toPosition)
                {
                    card.Position--;
                }
                else
                {
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
                .Where(c => c.ListId == listId && c.Position >= insertedPosition)
                .ToListAsync();

            foreach (var card in cardsToUpdate)
            {
                card.Position++;
            }
        }

        public async Task<bool> ExistsAsync(Guid id)
        {
            return await _context.Cards.AnyAsync(c => c.Id == id);
        }
    }
}
