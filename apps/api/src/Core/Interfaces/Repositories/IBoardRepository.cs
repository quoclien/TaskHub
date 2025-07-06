using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TaskHub.Api.Models.Entities.Enums;
using BoardEntity = TaskHub.Api.Models.Entities.Boards.Board;
using BoardMemberEntity = TaskHub.Api.Models.Entities.Boards.BoardMember;

namespace TaskHub.Api.Core.Interfaces.Repositories
{
    public interface IBoardRepository : IRepository<BoardEntity>
    {
        Task<BoardEntity?> GetByIdWithDetailsAsync(Guid id);
        Task<IEnumerable<BoardEntity>> GetUserBoardsAsync(Guid userId);
        Task<bool> IsBoardMemberAsync(Guid boardId, Guid userId);
        Task<bool> UserHasAccess(Guid boardId, Guid userId, BoardRole requiredRole);
        Task<BoardMemberEntity?> GetBoardMemberAsync(Guid boardId, Guid userId);
        Task AddBoardMemberAsync(BoardMemberEntity member);
        void RemoveBoardMember(BoardMemberEntity member);
        void UpdateBoardMember(BoardMemberEntity member);
        Task<int> SaveChangesAsync();
    }
}
