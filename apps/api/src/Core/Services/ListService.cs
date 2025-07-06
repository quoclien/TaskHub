using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.Extensions.Logging;
using TaskHub.Api.Core.Interfaces;
using TaskHub.Api.Core.Interfaces.Repositories;
using TaskHub.Api.Models.Dtos.Lists;
using TaskHub.Api.Models.Entities.Boards;
using TaskHub.Api.Models.Entities.Cards;
using ListEntity = TaskHub.Api.Models.Entities.Lists.List;

namespace TaskHub.Api.Core.Services
{
    public class ListService : IListService
    {
        private readonly IListRepository _listRepository;
        private readonly IBoardRepository _boardRepository;
        private readonly ICardRepository _cardRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<ListService> _logger;

        public ListService(
            IListRepository listRepository,
            IBoardRepository boardRepository,
            ICardRepository cardRepository,
            IMapper mapper,
            ILogger<ListService> logger)
        {
            _listRepository = listRepository ?? throw new ArgumentNullException(nameof(listRepository));
            _boardRepository = boardRepository ?? throw new ArgumentNullException(nameof(boardRepository));
            _cardRepository = cardRepository ?? throw new ArgumentNullException(nameof(cardRepository));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<IEnumerable<ListDto>> GetListsByBoardIdAsync(Guid boardId)
        {
            var lists = await _listRepository.GetListsByBoardIdAsync(boardId);
            return _mapper.Map<IEnumerable<ListDto>>(lists);
        }

        public async Task<ListDto?> GetListByIdAsync(Guid id)
        {
            var list = await _listRepository.GetByIdAsync(id);
            if (list == null)
                return null;

            return _mapper.Map<ListDto>(list);
        }

        public async Task<ListDto> CreateListAsync(Guid boardId, CreateListDto createListDto)
        {
            if (createListDto == null)
                throw new ArgumentNullException(nameof(createListDto));

            // Get the board to satisfy the required navigation property
            var board = await _boardRepository.GetByIdAsync(boardId);
            if (board == null)
            {
                throw new KeyNotFoundException($"Board with ID {boardId} not found.");
            }

            // Get the max position for the new list
            var maxPosition = await _listRepository.GetMaxPositionAsync(boardId);
            var position = createListDto.Position >= 0 ? createListDto.Position : maxPosition + 1;

            // Create the list
            var list = new ListEntity
            {
                Title = createListDto.Title,
                BoardId = boardId,
                Board = board, // Set the required navigation property
                Position = position,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            // Save the list
            await _listRepository.AddAsync(list);
            await _listRepository.UnitOfWork.SaveChangesAsync();

            // Reorder other lists if necessary
            if (position <= maxPosition)
            {
                await _listRepository.ReorderListsAfterInsertion(boardId, position);
                await _listRepository.UnitOfWork.SaveChangesAsync();
            }

            return _mapper.Map<ListDto>(list);
        }

        public async Task UpdateListAsync(Guid id, UpdateListDto updateListDto)
        {
            if (updateListDto == null)
                throw new ArgumentNullException(nameof(updateListDto));

            var list = await _listRepository.GetByIdOrThrowAsync(id);
            var originalPosition = list.Position;
            var originalBoardId = list.BoardId;
            var positionChanged = false;
            var boardChanged = false;

            // Update properties if provided
            if (!string.IsNullOrWhiteSpace(updateListDto.Title))
                list.Title = updateListDto.Title;

            // Handle position change
            if (updateListDto.Position.HasValue && updateListDto.Position.Value != list.Position)
            {
                list.Position = updateListDto.Position.Value;
                positionChanged = true;
            }

            // Handle board change if needed
            if (updateListDto.BoardId.HasValue && updateListDto.BoardId.Value != list.BoardId)
            {
                var newBoardExists = await _boardRepository.ExistsAsync(b => b.Id == updateListDto.BoardId.Value);
                if (!newBoardExists)
                    throw new KeyNotFoundException($"Target board with ID {updateListDto.BoardId} not found.");
                
                list.BoardId = updateListDto.BoardId.Value;
                boardChanged = true;
            }

            // Update timestamps
            list.UpdatedAt = DateTime.UtcNow;

            // Save changes
            _listRepository.Update(list);
            
            // Handle reordering if position or board changed
            if (positionChanged || boardChanged)
            {
                if (boardChanged)
                {
                    // Reorder lists in the original board
                    await _listRepository.ReorderListsAfterDeletion(originalBoardId, originalPosition);
                    
                    // Get new position in the target board
                    var maxPosition = await _listRepository.GetMaxPositionAsync(list.BoardId);
                    if (list.Position > maxPosition + 1)
                        list.Position = maxPosition + 1;
                    
                    // Reorder lists in the target board
                    await _listRepository.ReorderListsAfterInsertion(list.BoardId, list.Position);
                }
                else
                {
                    // Just reorder within the same board
                    await _listRepository.ReorderListsAsync(list.BoardId, originalPosition, list.Position);
                }
            }

            await _listRepository.UnitOfWork.SaveChangesAsync();
        }

        public async Task DeleteListAsync(Guid id)
        {
            var list = await _listRepository.GetByIdOrThrowAsync(id);
            var boardId = list.BoardId;
            var position = list.Position;

            // First, delete all cards in the list
            var cards = await _cardRepository.GetCardsByListIdAsync(id);
            foreach (var card in cards)
            {
                _cardRepository.Delete(card);
            }

            // Delete the list
            _listRepository.Delete(list);
            await _listRepository.UnitOfWork.SaveChangesAsync();

            // Reorder remaining lists
            await _listRepository.ReorderListsAfterDeletion(boardId, position);
            await _listRepository.UnitOfWork.SaveChangesAsync();
        }

        private async Task ReorderLists(Guid boardId, int oldPosition, int newPosition)
        {
            var lists = await _listRepository.GetListsByBoardIdAsync(boardId);
            var listToMove = lists.FirstOrDefault(l => l.Position == oldPosition);
            if (listToMove == null) return;

            foreach (var list in lists)
            {
                if (oldPosition < newPosition)
                {
                    // Moving down
                    if (list.Position > oldPosition && list.Position <= newPosition)
                    {
                        list.Position--;
                        _listRepository.Update(list);
                    }
                }
                else
                {
                    // Moving up
                    if (list.Position >= newPosition && list.Position < oldPosition)
                    {
                        list.Position++;
                        _listRepository.Update(list);
                    }
                }
            }

            await _listRepository.UnitOfWork.SaveChangesAsync();
        }

        private async Task ReorderListsAfterDeletion(Guid boardId, int deletedPosition)
        {
            var lists = await _listRepository.GetListsByBoardIdAsync(boardId);
            foreach (var list in lists.Where(l => l.Position > deletedPosition))
            {
                list.Position--;
                _listRepository.Update(list);
            }

            await _listRepository.UnitOfWork.SaveChangesAsync();
        }
    }
}
