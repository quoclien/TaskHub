using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TaskHub.Api.Core.Interfaces;
using TaskHub.Api.Core.Interfaces.Repositories;
using TaskHub.Api.Models.Entities.Enums;
using BoardEntity = TaskHub.Api.Models.Entities.Boards.Board;
using BoardMemberEntity = TaskHub.Api.Models.Entities.Boards.BoardMember;

namespace TaskHub.Api.Infrastructure.Data.Repositories
{
    public class BoardRepository : Repository<BoardEntity>, IBoardRepository
    {
        public BoardRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
        {
        }

        public async Task<BoardEntity?> GetByIdWithDetailsAsync(Guid id)
        {
            // EF Core's translation of OrderBy inside Include is not supported by all providers (e.g., Npgsql).
            // We fetch related entities without explicit ordering and let application code sort where necessary.
            return await _context.Boards
                .Include(b => b.Lists)
                    .ThenInclude(l => l.Cards)
                .Include(b => b.BoardMembers)
                    .ThenInclude(bm => bm.User)
                .AsSplitQuery()
                .FirstOrDefaultAsync(b => b.Id == id);
        }

        public async Task<IEnumerable<BoardEntity>> GetUserBoardsAsync(Guid userId)
        {
            return await _context.Boards
                .Where(b => b.BoardMembers.Any(m => m.UserId == userId))
                .ToListAsync();
        }

        public async Task<bool> IsBoardMemberAsync(Guid boardId, Guid userId)
        {
            return await _context.BoardMembers
                .AnyAsync(bm => bm.BoardId == boardId && bm.UserId == userId);
        }

        public async Task<bool> UserHasAccess(Guid boardId, Guid userId, BoardRole requiredRole)
        {
            return await _context.BoardMembers
                .AnyAsync(bm => bm.BoardId == boardId && bm.UserId == userId && bm.Role >= requiredRole);
        }

        public async Task<BoardRole?> GetUserRoleInBoardAsync(Guid userId, Guid boardId)
        {
            var boardMember = await _context.BoardMembers
                .FirstOrDefaultAsync(bm => bm.UserId == userId && bm.BoardId == boardId);
            
            return boardMember?.Role;
        }

        public async Task<BoardMemberEntity?> GetBoardMemberAsync(Guid boardId, Guid userId)
        {
            return await _context.BoardMembers
                .FirstOrDefaultAsync(bm => bm.BoardId == boardId && bm.UserId == userId);
        }

        public async Task AddBoardMemberAsync(BoardMemberEntity member)
        {
            await _context.BoardMembers.AddAsync(member);
        }

        public void RemoveBoardMember(BoardMemberEntity member)
        {
            _context.BoardMembers.Remove(member);
        }

        public void UpdateBoardMember(BoardMemberEntity member)
        {
            _context.BoardMembers.Update(member);
        }


    }
}
