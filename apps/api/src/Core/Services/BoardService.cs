using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.Extensions.Logging;
using TaskHub.Api.Core.Interfaces;
using TaskHub.Api.Core.Interfaces.Repositories;
using TaskHub.Api.Shared.Models;
using TaskHub.Api.Shared.Models.Dtos;

namespace TaskHub.Api.Core.Services
{
    public class BoardService : IBoardService
    {
        private readonly IBoardRepository _boardRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<BoardService> _logger;

        public BoardService(
            IBoardRepository boardRepository,
            IMapper mapper,
            ILogger<BoardService> logger)
        {
            _boardRepository = boardRepository ?? throw new ArgumentNullException(nameof(boardRepository));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<BoardDto> CreateBoardAsync(CreateBoardDto createBoardDto, string userId)
        {
            try
            {
                var board = _mapper.Map<Board>(createBoardDto);
                board.CreatedBy = userId;
                board.Members.Add(new BoardMember
                {
                    UserId = userId,
                    Role = UserRole.Owner,
                    JoinedAt = DateTime.UtcNow
                });

                var createdBoard = await _boardRepository.CreateAsync(board);
                return _mapper.Map<BoardDto>(createdBoard);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating board for user {UserId}", userId);
                throw;
            }
        }

        public async Task<BoardDto?> GetBoardByIdAsync(string id, string userId)
        {
            try
            {
                var hasAccess = await _boardRepository.UserHasAccess(id, userId);
                if (!hasAccess)
                {
                    return null;
                }

                var board = await _boardRepository.GetByIdAsync(id);
                return board != null ? _mapper.Map<BoardDto>(board) : null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving board {BoardId} for user {UserId}", id, userId);
                throw;
            }
        }

        public async Task<IEnumerable<BoardDto>> GetUserBoardsAsync(string userId)
        {
            try
            {
                var boards = await _boardRepository.GetBoardsByUserAsync(userId);
                return _mapper.Map<IEnumerable<BoardDto>>(boards);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving boards for user {UserId}", userId);
                throw;
            }
        }

        public async Task<bool> UpdateBoardAsync(string id, UpdateBoardDto updateBoardDto, string userId)
        {
            try
            {
                var hasAccess = await _boardRepository.UserHasAccess(id, userId, UserRole.Owner);
                if (!hasAccess)
                {
                    return false;
                }

                var board = await _boardRepository.GetByIdAsync(id);
                if (board == null)
                {
                    return false;
                }

                _mapper.Map(updateBoardDto, board);
                board.UpdatedAt = DateTime.UtcNow;

                return await _boardRepository.UpdateAsync(board);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating board {BoardId} by user {UserId}", id, userId);
                throw;
            }
        }

        public async Task<bool> DeleteBoardAsync(string id, string userId)
        {
            try
            {
                var hasAccess = await _boardRepository.UserHasAccess(id, userId, UserRole.Owner);
                if (!hasAccess)
                {
                    return false;
                }

                return await _boardRepository.DeleteAsync(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting board {BoardId} by user {UserId}", id, userId);
                throw;
            }
        }

        public async Task<bool> AddBoardMemberAsync(string boardId, string userId, string currentUserId, UserRole role = UserRole.Member)
        {
            try
            {
                // Only board owners can add members
                var hasAccess = await _boardRepository.UserHasAccess(boardId, currentUserId, UserRole.Owner);
                if (!hasAccess)
                {
                    return false;
                }

                var member = new BoardMember
                {
                    UserId = userId,
                    Role = role,
                    JoinedAt = DateTime.UtcNow
                };

                return await _boardRepository.AddMemberAsync(boardId, member);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding member {UserId} to board {BoardId} by user {CurrentUserId}", 
                    userId, boardId, currentUserId);
                throw;
            }
        }

        public async Task<bool> RemoveBoardMemberAsync(string boardId, string userId, string currentUserId)
        {
            try
            {
                // Only board owners can remove members
                var hasAccess = await _boardRepository.UserHasAccess(boardId, currentUserId, UserRole.Owner);
                if (!hasAccess)
                {
                    return false;
                }

                // Prevent removing yourself if you're the last owner
                if (userId == currentUserId)
                {
                    var board = await _boardRepository.GetByIdAsync(boardId);
                    if (board != null && board.Members.Count(m => m.Role == UserRole.Owner) <= 1)
                    {
                        return false; // Can't remove the last owner
                    }
                }

                return await _boardRepository.RemoveMemberAsync(boardId, userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing member {UserId} from board {BoardId} by user {CurrentUserId}", 
                    userId, boardId, currentUserId);
                throw;
            }
        }

        public async Task<bool> UpdateBoardMemberRoleAsync(string boardId, string userId, string currentUserId, UserRole newRole)
        {
            try
            {
                // Only board owners can update member roles
                var hasAccess = await _boardRepository.UserHasAccess(boardId, currentUserId, UserRole.Owner);
                if (!hasAccess)
                {
                    return false;
                }

                // Prevent changing your own role if you're the last owner
                if (userId == currentUserId && newRole != UserRole.Owner)
                {
                    var board = await _boardRepository.GetByIdAsync(boardId);
                    if (board != null && board.Members.Count(m => m.Role == UserRole.Owner) <= 1)
                    {
                        return false; // Can't change role of the last owner
                    }
                }

                return await _boardRepository.UpdateMemberRoleAsync(boardId, userId, newRole);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating role for member {UserId} in board {BoardId} by user {CurrentUserId}", 
                    userId, boardId, currentUserId);
                throw;
            }
        }
    }
}
