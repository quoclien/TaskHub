using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TaskHub.Api.Core.Interfaces;
using TaskHub.Api.Core.Interfaces.Repositories;
using TaskHub.Api.Infrastructure.Data;
using ListEntity = TaskHub.Api.Models.Entities.Lists.List;

namespace TaskHub.Api.Infrastructure.Data.Repositories
{
    public class ListRepository : Repository<ListEntity>, IListRepository
    {
        public ListRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
        {
        }

        public async Task<ListEntity> GetByIdOrThrowAsync(Guid id)
        {
            var list = await _context.Lists.FindAsync(id);
            if (list == null)
            {
                throw new KeyNotFoundException($"List with ID {id} not found.");
            }
            return list;
        }

        public async Task<IEnumerable<ListEntity>> GetListsByBoardIdAsync(Guid boardId)
        {
            return await _context.Lists
                .Where(l => l.BoardId == boardId)
                .OrderBy(l => l.Position)
                .ToListAsync();
        }

        public async Task<bool> BoardExistsAsync(Guid boardId)
        {
            return await _context.Boards.AnyAsync(b => b.Id == boardId);
        }

        public async Task<int> GetMaxPositionAsync(Guid boardId)
        {
            return await _context.Lists
                .Where(l => l.BoardId == boardId)
                .Select(l => l.Position)
                .DefaultIfEmpty(0)
                .MaxAsync();
        }

        public async Task ReorderListsAsync(Guid boardId, int fromPosition, int toPosition)
        {
            var listsToUpdate = await _context.Lists
                .Where(l => l.BoardId == boardId &&
                            (l.Position >= fromPosition && l.Position <= toPosition ||
                             l.Position <= fromPosition && l.Position >= toPosition))
                .ToListAsync();

            foreach (var list in listsToUpdate)
            {
                if (list.Position == fromPosition)
                {
                    list.Position = toPosition;
                }
                else if (fromPosition < toPosition)
                {
                    list.Position--;
                }
                else
                {
                    list.Position++;
                }
            }
        }

        public async Task ReorderListsAfterDeletion(Guid boardId, int deletedPosition)
        {
            var listsToUpdate = await _context.Lists
                .Where(l => l.BoardId == boardId && l.Position > deletedPosition)
                .ToListAsync();

            foreach (var list in listsToUpdate)
            {
                list.Position--;
            }
        }

        public async Task ReorderListsAfterInsertion(Guid boardId, int insertedPosition)
        {
            var listsToUpdate = await _context.Lists
                .Where(l => l.BoardId == boardId && l.Position >= insertedPosition)
                .ToListAsync();

            foreach (var list in listsToUpdate)
            {
                list.Position++;
            }
        }
    }
}
