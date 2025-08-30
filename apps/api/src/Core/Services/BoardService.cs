using System;
using System.Collections.Generic;

using System.Threading.Tasks;
using AutoMapper;
using Microsoft.Extensions.Logging;
using TaskHub.Api.Core.Exceptions;
using TaskHub.Api.Core.Interfaces;
using TaskHub.Api.Core.Interfaces.Repositories;
using TaskHub.Api.Models.Dtos.Boards;
using TaskHub.Api.Models.Entities.Enums;
using BoardEntity = TaskHub.Api.Models.Entities.Boards.Board;
using BoardMemberEntity = TaskHub.Api.Models.Entities.Boards.BoardMember;

namespace TaskHub.Api.Core.Services
{
    public class BoardService : BaseService, IBoardService
    {
        private const string BoardNotFound = "Board not found";
        private const string UserNotFound = "User not found";
        private const string UnauthorizedAccess = "You do not have permission to perform this action";
        private const string MemberAlreadyExists = "User is already a member of this board";
        private const string LastAdminRemoval = "Cannot remove the last admin of the board";
        private const string SelfRoleChange = "Users cannot change their own role";

        private readonly IBoardRepository _boardRepository;
        private readonly IUserRepository _userRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<BoardService> _logger;

        public BoardService(
            IBoardRepository boardRepository,
            IUserRepository userRepository,
            IMapper mapper,
            ILogger<BoardService> logger,
            IUnitOfWork unitOfWork)
            : base(unitOfWork)
        {
            _boardRepository = boardRepository ?? throw new ArgumentNullException(nameof(boardRepository));
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<BoardDto> CreateBoardAsync(CreateBoardDto createBoardDto, Guid userId)
        {
            if (createBoardDto == null)
                throw new ArgumentNullException(nameof(createBoardDto));

            var user = await _userRepository.GetByIdAsync(userId)
                ?? throw new NotFoundException(UserNotFound);

            var board = _mapper.Map<BoardEntity>(createBoardDto);
            board.OwnerId = userId;
            board.CreatedAt = DateTime.UtcNow;

            try
            {
                // First save the board to get the ID
                await _boardRepository.AddAsync(board);
                await SaveChangesAsync();

                // Now add the board member with the correct BoardId
                var boardMember = new BoardMemberEntity
                {
                    BoardId = board.Id,
                    UserId = userId,
                    User = user, // ensure User navigation is populated to avoid null mapping issues
                    Role = BoardRole.Owner,
                    JoinedAt = DateTime.UtcNow
                };

                await _boardRepository.AddBoardMemberAsync(boardMember);
                await SaveChangesAsync();

                // Return the board with the member information
                var boardWithDetails = await _boardRepository.GetByIdWithDetailsAsync(board.Id);

                // Fallback to the original entity if the detailed load failed for any reason.
                var entityToMap = boardWithDetails ?? board;
                return _mapper.Map<BoardDto>(entityToMap);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating board for user {UserId}", userId);
                throw new ApplicationException("An error occurred while creating the board", ex);
            }
        }

        public async Task<BoardDto?> GetBoardByIdAsync(Guid id, Guid userId)
        {
            if (id == Guid.Empty)
                throw new ArgumentException("Board ID cannot be empty", nameof(id));

            var board = await _boardRepository.GetByIdWithDetailsAsync(id);
            if (board == null)
                return null;

            if (!board.IsPublic && !await _boardRepository.IsBoardMemberAsync(id, userId))
            {
                _logger.LogWarning("Unauthorized access attempt to board {BoardId} by user {UserId}", id, userId);
                throw new UnauthorizedException(UnauthorizedAccess);
            }

            return _mapper.Map<BoardDto>(board);
        }

        public async Task<IEnumerable<BoardDto>> GetUserBoardsAsync(Guid userId)
        {
            if (userId == Guid.Empty)
                throw new ArgumentException("User ID cannot be empty", nameof(userId));

            try
            {
                var boards = await _boardRepository.GetUserBoardsAsync(userId);
                return _mapper.Map<IEnumerable<BoardDto>>(boards);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving boards for user {UserId}", userId);
                throw new ApplicationException("An error occurred while retrieving boards", ex);
            }
        }

        public async Task<bool> UpdateBoardAsync(Guid id, UpdateBoardDto updateBoardDto, Guid userId)
        {
            if (id == Guid.Empty)
                throw new ArgumentException("Board ID cannot be empty", nameof(id));
            if (updateBoardDto == null)
                throw new ArgumentNullException(nameof(updateBoardDto));

            var hasAccess = await _boardRepository.UserHasAccess(id, userId, BoardRole.Owner);
            if (!hasAccess)
            {
                _logger.LogWarning("Unauthorized update attempt on board {BoardId} by user {UserId}", id, userId);
                throw new UnauthorizedException(UnauthorizedAccess);
            }

            var board = await _boardRepository.GetByIdAsync(id);
            if (board == null)
            {
                _logger.LogWarning("Board {BoardId} not found for update", id);
                return false;
            }

            try
            {
                _mapper.Map(updateBoardDto, board);
                board.UpdatedAt = DateTime.UtcNow;

                _boardRepository.Update(board);
                await SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating board {BoardId}", id);
                throw new ApplicationException("An error occurred while updating the board", ex);
            }
        }

        public async Task<bool> DeleteBoardAsync(Guid id, Guid userId)
        {
            if (id == Guid.Empty)
                throw new ArgumentException("Board ID cannot be empty", nameof(id));

            var board = await _boardRepository.GetByIdAsync(id);
            if (board == null)
            {
                _logger.LogWarning("Board {BoardId} not found for deletion", id);
                return false;
            }

            if (board.OwnerId != userId)
            {
                _logger.LogWarning("Unauthorized delete attempt on board {BoardId} by user {UserId}", id, userId);
                throw new UnauthorizedException("Only the board owner can delete the board.");
            }

            try
            {
                _boardRepository.Delete(board);
                await SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting board {BoardId}", id);
                throw new ApplicationException("An error occurred while deleting the board", ex);
            }
        }

        public async Task<bool> AddBoardMemberAsync(Guid boardId, Guid userId, Guid currentUserId, BoardRole role = BoardRole.Member)
        {
            if (boardId == Guid.Empty)
                throw new ArgumentException("Board ID cannot be empty", nameof(boardId));
            if (userId == Guid.Empty)
                throw new ArgumentException("User ID cannot be empty", nameof(userId));
            if (currentUserId == Guid.Empty)
                throw new ArgumentException("Current user ID cannot be empty", nameof(currentUserId));

            var hasAccess = await _boardRepository.UserHasAccess(boardId, currentUserId, BoardRole.Owner);
            if (!hasAccess)
            {
                _logger.LogWarning("Unauthorized member add attempt on board {BoardId} by user {UserId}", boardId, currentUserId);
                throw new UnauthorizedException("Only board admins can add members.");
            }

            if (await _boardRepository.IsBoardMemberAsync(boardId, userId))
            {
                _logger.LogWarning("Attempt to add existing member {UserId} to board {BoardId}", userId, boardId);
                throw new InvalidOperationException(MemberAlreadyExists);
            }

            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
            {
                _logger.LogWarning("User {UserId} not found when adding to board {BoardId}", userId, boardId);
                throw new NotFoundException(UserNotFound);
            }

            try
            {
                var member = new BoardMemberEntity
                {
                    BoardId = boardId,
                    UserId = userId,
                    Role = role,
                    JoinedAt = DateTime.UtcNow
                };

                await _boardRepository.AddBoardMemberAsync(member);
                await SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding user {UserId} to board {BoardId}", userId, boardId);
                throw new ApplicationException("An error occurred while adding the member to the board", ex);
            }
        }

        public async Task<bool> RemoveBoardMemberAsync(Guid boardId, Guid userId, Guid currentUserId)
        {
            if (boardId == Guid.Empty)
                throw new ArgumentException("Board ID cannot be empty", nameof(boardId));
            if (userId == Guid.Empty)
                throw new ArgumentException("User ID cannot be empty", nameof(userId));
            if (currentUserId == Guid.Empty)
                throw new ArgumentException("Current user ID cannot be empty", nameof(currentUserId));

            var hasAccess = await _boardRepository.UserHasAccess(boardId, currentUserId, BoardRole.Owner);
            if (!hasAccess)
            {
                _logger.LogWarning("Unauthorized member removal attempt on board {BoardId} by user {UserId}", boardId, currentUserId);
                throw new UnauthorizedException("Only board admins can remove members.");
            }

            if (userId == currentUserId)
            {
                var board = await _boardRepository.GetByIdWithDetailsAsync(boardId);
                if (board?.BoardMembers?.Count(m => m.Role == BoardRole.Owner) <= 1)
                {
                    _logger.LogWarning("Attempt to remove last admin from board {BoardId}", boardId);
                    throw new InvalidOperationException(LastAdminRemoval);
                }
            }

            var member = await _boardRepository.GetBoardMemberAsync(boardId, userId);
            if (member == null)
            {
                _logger.LogWarning("Member {UserId} not found in board {BoardId}", userId, boardId);
                return false;
            }

            try
            {
                _boardRepository.RemoveBoardMember(member);
                await SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing user {UserId} from board {BoardId}", userId, boardId);
                throw new ApplicationException("An error occurred while removing the member from the board", ex);
            }
        }

        public async Task<bool> UpdateBoardMemberRoleAsync(Guid boardId, Guid userId, Guid currentUserId, BoardRole newRole)
        {
            if (boardId == Guid.Empty)
                throw new ArgumentException("Board ID cannot be empty", nameof(boardId));
            if (userId == Guid.Empty)
                throw new ArgumentException("User ID cannot be empty", nameof(userId));
            if (currentUserId == Guid.Empty)
                throw new ArgumentException("Current user ID cannot be empty", nameof(currentUserId));

            var hasAccess = await _boardRepository.UserHasAccess(boardId, currentUserId, BoardRole.Owner);
            if (!hasAccess)
            {
                _logger.LogWarning("Unauthorized role update attempt on board {BoardId} by user {UserId}", boardId, currentUserId);
                throw new UnauthorizedException("Only board admins can change member roles.");
            }

            if (userId == currentUserId)
            {
                _logger.LogWarning("User {UserId} attempted to change their own role in board {BoardId}", currentUserId, boardId);
                throw new InvalidOperationException(SelfRoleChange);
            }

            var member = await _boardRepository.GetBoardMemberAsync(boardId, userId);
            if (member == null)
            {
                _logger.LogWarning("Member {UserId} not found in board {BoardId} for role update", userId, boardId);
                return false;
            }

            try
            {
                member.Role = newRole;
                _boardRepository.UpdateBoardMember(member);
                await SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating role for user {UserId} in board {BoardId}", userId, boardId);
                throw new ApplicationException("An error occurred while updating the member role", ex);
            }
        }
    }
}
