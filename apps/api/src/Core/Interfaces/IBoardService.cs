using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TaskHub.Api.Models.Dtos.Boards;
using TaskHub.Api.Models.Entities.Enums;

namespace TaskHub.Api.Core.Interfaces
{
    public interface IBoardService
    {
        Task<BoardDto> CreateBoardAsync(CreateBoardDto createBoardDto, Guid userId);
        Task<BoardDto?> GetBoardByIdAsync(Guid id, Guid userId);
        Task<IEnumerable<BoardDto>> GetUserBoardsAsync(Guid userId);
        Task<bool> UpdateBoardAsync(Guid id, UpdateBoardDto updateBoardDto, Guid userId);
        Task<bool> DeleteBoardAsync(Guid id, Guid userId);
        Task<bool> AddBoardMemberAsync(Guid boardId, Guid userId, Guid currentUserId, BoardRole role = BoardRole.Member);
        Task<bool> RemoveBoardMemberAsync(Guid boardId, Guid userId, Guid currentUserId);
        Task<bool> UpdateBoardMemberRoleAsync(Guid boardId, Guid userId, Guid currentUserId, BoardRole newRole);
    }
}
