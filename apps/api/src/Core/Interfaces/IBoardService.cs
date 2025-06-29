using System.Collections.Generic;
using System.Threading.Tasks;
using TaskHub.Api.Shared.Models;
using TaskHub.Api.Shared.Models.Dtos;

namespace TaskHub.Api.Core.Interfaces
{
    public interface IBoardService
    {
        Task<BoardDto> CreateBoardAsync(CreateBoardDto createBoardDto, string userId);
        Task<BoardDto?> GetBoardByIdAsync(string id, string userId);
        Task<IEnumerable<BoardDto>> GetUserBoardsAsync(string userId);
        Task<bool> UpdateBoardAsync(string id, UpdateBoardDto updateBoardDto, string userId);
        Task<bool> DeleteBoardAsync(string id, string userId);
        Task<bool> AddBoardMemberAsync(string boardId, string userId, string currentUserId, UserRole role = UserRole.Member);
        Task<bool> RemoveBoardMemberAsync(string boardId, string userId, string currentUserId);
        Task<bool> UpdateBoardMemberRoleAsync(string boardId, string userId, string currentUserId, UserRole newRole);
    }
}
