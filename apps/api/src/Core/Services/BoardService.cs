using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.Extensions.Logging;
using TaskHub.Api.Core.Interfaces;
using TaskHub.Api.Core.Interfaces.Repositories;
using TaskHub.Api.Models.Dtos.Boards;
using TaskHub.Api.Models.Entities.Enums;
using BoardEntity = TaskHub.Api.Models.Entities.Boards.Board;
using BoardMemberEntity = TaskHub.Api.Models.Entities.Boards.BoardMember;

namespace TaskHub.Api.Core.Services
{
    public class BoardService : IBoardService
    {
        private readonly IBoardRepository _boardRepository;
        private readonly IUserRepository _userRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<BoardService> _logger;

        public BoardService(
            IBoardRepository boardRepository,
            IUserRepository userRepository,
            IMapper mapper,
            ILogger<BoardService> logger)
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

            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
            {
                throw new KeyNotFoundException($"User with ID {userId} not found.");
            }

            var board = _mapper.Map<BoardEntity>(createBoardDto);
            board.OwnerId = userId;

            board.BoardMembers.Add(new BoardMemberEntity
            {
                UserId = userId,
                Role = BoardRole.Owner,
                JoinedAt = DateTime.UtcNow
            });

            await _boardRepository.AddAsync(board);
            await _boardRepository.SaveChangesAsync();

            return _mapper.Map<BoardDto>(board);
        }

        public async Task<BoardDto?> GetBoardByIdAsync(Guid id, Guid userId)
        {
            var board = await _boardRepository.GetByIdWithDetailsAsync(id);
            if (board == null)
                return null;

            if (!board.IsPublic && !await _boardRepository.IsBoardMemberAsync(id, userId))
            {
                throw new UnauthorizedAccessException("You don't have access to this board.");
            }

            return _mapper.Map<BoardDto>(board);
        }

        public async Task<IEnumerable<BoardDto>> GetUserBoardsAsync(Guid userId)
        {
            var boards = await _boardRepository.GetUserBoardsAsync(userId);
            return _mapper.Map<IEnumerable<BoardDto>>(boards);
        }

        public async Task<bool> UpdateBoardAsync(Guid id, UpdateBoardDto updateBoardDto, Guid userId)
        {
            var hasAccess = await _boardRepository.UserHasAccess(id, userId, BoardRole.Owner);
            if (!hasAccess)
            {
                throw new UnauthorizedAccessException("You do not have permission to update this board.");
            }

            var board = await _boardRepository.GetByIdAsync(id);
            if (board == null)
            {
                return false;
            }

            _mapper.Map(updateBoardDto, board);
            board.UpdatedAt = DateTime.UtcNow;

            _boardRepository.Update(board);
            await _boardRepository.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteBoardAsync(Guid id, Guid userId)
        {
            var board = await _boardRepository.GetByIdAsync(id);
            if (board == null)
                return false;

            if (board.OwnerId != userId)
            {
                throw new UnauthorizedAccessException("Only the board owner can delete the board.");
            }

            _boardRepository.Delete(board);
            await _boardRepository.SaveChangesAsync();
            return true;
        }

        public async Task<bool> AddBoardMemberAsync(Guid boardId, Guid userId, Guid currentUserId, BoardRole role = BoardRole.Member)
        {
            var hasAccess = await _boardRepository.UserHasAccess(boardId, currentUserId, BoardRole.Owner);
            if (!hasAccess)
            {
                throw new UnauthorizedAccessException("Only board admins can add members.");
            }

            if (await _boardRepository.IsBoardMemberAsync(boardId, userId))
            {
                throw new InvalidOperationException("User is already a member of this board.");
            }

            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
            {
                throw new KeyNotFoundException($"User with ID {userId} not found.");
            }

            var member = new BoardMemberEntity
            {
                BoardId = boardId,
                UserId = userId,
                Role = role,
                JoinedAt = DateTime.UtcNow
            };

            await _boardRepository.AddBoardMemberAsync(member);
            await _boardRepository.SaveChangesAsync();
            return true;
        }

        public async Task<bool> RemoveBoardMemberAsync(Guid boardId, Guid userId, Guid currentUserId)
        {
            var hasAccess = await _boardRepository.UserHasAccess(boardId, currentUserId, BoardRole.Owner);
            if (!hasAccess)
            {
                throw new UnauthorizedAccessException("Only board admins can remove members.");
            }

            if (userId == currentUserId)
            {
                var board = await _boardRepository.GetByIdWithDetailsAsync(boardId);
                if (board != null && board.BoardMembers.Count(m => m.Role == BoardRole.Owner) <= 1)
                {
                    throw new InvalidOperationException("Cannot remove the last admin of the board.");
                }
            }

            var member = await _boardRepository.GetBoardMemberAsync(boardId, userId);
            if (member == null)
            {
                return false;
            }

            _boardRepository.RemoveBoardMember(member);
            await _boardRepository.SaveChangesAsync();
            return true;
        }

        public async Task<bool> UpdateBoardMemberRoleAsync(Guid boardId, Guid userId, Guid currentUserId, BoardRole newRole)
        {
            var hasAccess = await _boardRepository.UserHasAccess(boardId, currentUserId, BoardRole.Owner);
            if (!hasAccess)
            {
                throw new UnauthorizedAccessException("Only board admins can change member roles.");
            }

            if (userId == currentUserId)
            {
                throw new InvalidOperationException("Users cannot change their own role.");
            }

            var member = await _boardRepository.GetBoardMemberAsync(boardId, userId);
            if (member == null)
            {
                return false;
            }

            member.Role = newRole;
            _boardRepository.UpdateBoardMember(member);
            await _boardRepository.SaveChangesAsync();
            return true;
        }
    }
}
