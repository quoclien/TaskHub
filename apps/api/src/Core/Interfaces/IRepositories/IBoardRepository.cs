using System.Collections.Generic;
using System.Threading.Tasks;
using TaskHub.Api.Shared.Models;

namespace TaskHub.Api.Core.Interfaces.Repositories
{
    public interface IBoardRepository
    {
        Task<Board?> GetByIdAsync(string id);
        Task<IEnumerable<Board>> GetBoardsByUserAsync(string userId);
        Task<Board> CreateAsync(Board board);
        Task<bool> UpdateAsync(Board board);
        Task<bool> DeleteAsync(string id);
        Task<bool> AddMemberAsync(string boardId, BoardMember member);
        Task<bool> RemoveMemberAsync(string boardId, string userId);
        Task<bool> UpdateMemberRoleAsync(string boardId, string userId, UserRole newRole);
        Task<bool> UserHasAccess(string boardId, string userId, UserRole? requiredRole = null);
    }
}
