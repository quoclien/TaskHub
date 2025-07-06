using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.Extensions.Logging;
using TaskHub.Api.Core.Interfaces;
using TaskHub.Api.Core.Interfaces.Repositories;
using TaskHub.Api.Models.Dtos.Cards;
using CardEntity = TaskHub.Api.Models.Entities.Cards.Card;

namespace TaskHub.Api.Core.Services
{
    public class CardService : ICardService
    {
        private readonly ICardRepository _cardRepository;
        private readonly IListRepository _listRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<CardService> _logger;

        public CardService(
            ICardRepository cardRepository,
            IListRepository listRepository,
            IMapper mapper,
            ILogger<CardService> logger)
        {
            _cardRepository = cardRepository ?? throw new ArgumentNullException(nameof(cardRepository));
            _listRepository = listRepository ?? throw new ArgumentNullException(nameof(listRepository));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<IEnumerable<CardDto>> GetCardsByListIdAsync(Guid listId)
        {
            try
            {
                var cards = await _cardRepository.GetCardsByListIdAsync(listId);
                return _mapper.Map<IEnumerable<CardDto>>(cards);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving cards for list {ListId}", listId);
                throw;
            }
        }

        public async Task<CardDto?> GetCardByIdAsync(Guid id)
        {
            try
            {
                var card = await _cardRepository.GetByIdAsync(id);
                return card != null ? _mapper.Map<CardDto>(card) : null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving card {CardId}", id);
                throw;
            }
        }

        public async Task<CardDto> CreateCardAsync(Guid listId, CreateCardDto createCardDto)
        {
            if (createCardDto == null)
                throw new ArgumentNullException(nameof(createCardDto));

            // Get the list to satisfy the required navigation property
            var list = await _listRepository.GetByIdOrThrowAsync(listId);

            // Get the max position for the new card
            var maxPosition = await _cardRepository.GetMaxPositionAsync(listId);
            var position = createCardDto.Position >= 0 ? createCardDto.Position : maxPosition + 1;

            // Create the card
            var card = new CardEntity
            {
                Title = createCardDto.Title,
                Description = createCardDto.Description ?? string.Empty,
                ListId = listId,
                List = list, // Set the required navigation property
                Position = position,
                AssignedTo = createCardDto.AssignedTo,
                DueDate = createCardDto.DueDate,
                IsArchived = false,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            // Save the card
            await _cardRepository.AddAsync(card);
            await _cardRepository.UnitOfWork.SaveChangesAsync();

            // Reorder other cards if necessary
            if (position <= maxPosition)
            {
                await _cardRepository.ReorderCardsAfterInsertion(listId, position);
                await _cardRepository.UnitOfWork.SaveChangesAsync();
            }

            // Map and return the result
            return _mapper.Map<CardDto>(card);
        }

        public async Task UpdateCardAsync(Guid id, UpdateCardDto updateCardDto)
        {
            if (updateCardDto == null)
                throw new ArgumentNullException(nameof(updateCardDto));

            try
            {
                var card = await _cardRepository.GetByIdOrThrowAsync(id);
                var originalPosition = card.Position;
                var originalListId = card.ListId;
                var positionChanged = false;
                var listChanged = false;

                // Update properties if provided
                if (updateCardDto.Title != null)
                {
                    card.Title = updateCardDto.Title;
                }

                if (updateCardDto.Description != null)
                {
                    card.Description = updateCardDto.Description;
                }

                if (updateCardDto.DueDate.HasValue)
                {
                    card.DueDate = updateCardDto.DueDate;
                }

                if (updateCardDto.Metadata != null)
                {
                    card.MetadataJson = updateCardDto.Metadata;
                }
                
                if (updateCardDto.AssignedTo.HasValue)
                {
                    // [AI_ASSUMPTION] Assuming we have an AssignedTo property on the Card model
                    // If not, we'll need to add it
                    card.AssignedTo = updateCardDto.AssignedTo.Value;
                }
                
                if (updateCardDto.IsArchived.HasValue)
                {
                    // [AI_ASSUMPTION] Assuming we have an IsArchived property on the Card model
                    // If not, we'll need to add it
                    card.IsArchived = updateCardDto.IsArchived.Value;
                }

                // Handle list change if needed
                if (updateCardDto.ListId.HasValue && updateCardDto.ListId.Value != card.ListId)
                {
                    // Verify the target list exists
                    var listExists = await _listRepository.ExistsAsync(l => l.Id == updateCardDto.ListId.Value);
                    if (!listExists)
                    {
                        throw new KeyNotFoundException($"List with ID {updateCardDto.ListId} not found.");
                    }
                    
                    card.ListId = updateCardDto.ListId.Value;
                    listChanged = true;
                }

                // Handle position change if needed
                if (updateCardDto.Position.HasValue && updateCardDto.Position.Value != card.Position)
                {
                    card.Position = updateCardDto.Position.Value;
                    positionChanged = true;
                }

                card.UpdatedAt = DateTime.UtcNow;

                // Save changes
                _cardRepository.Update(card);
                await _cardRepository.UnitOfWork.SaveChangesAsync();

                // Handle reordering if needed
                if (listChanged)
                {
                    // If list changed, reorder both the old and new lists
                    await _cardRepository.ReorderCardsAfterDeletion(originalListId, originalPosition);
                    await _cardRepository.ReorderCardsAfterInsertion(card.ListId, card.Position);
                }
                else if (positionChanged)
                {
                    // If only position changed, reorder within the same list
                    await _cardRepository.ReorderCardsAsync(card.ListId, originalPosition, card.Position);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating card {CardId}", id);
                throw;
            }
        }

        public async Task MoveCardAsync(Guid cardId, Guid targetListId, int newPosition)
        {
            try
            {
                var card = await _cardRepository.GetByIdAsync(cardId);
                if (card == null)
                {
                    throw new KeyNotFoundException($"Card with ID {cardId} not found");
                }

                // If moving to a different list, verify the target list exists
                if (card.ListId != targetListId && !await _listRepository.AnyAsync(l => l.Id == targetListId))
                {
                    throw new KeyNotFoundException($"Target list with ID {targetListId} not found");
                }

                var originalListId = card.ListId;
                var originalPosition = card.Position;

                // If moving to a different list, update the list ID
                if (card.ListId != targetListId)
                {
                    card.ListId = targetListId;
                }

                // Update position
                card.Position = newPosition;
                card.UpdatedAt = DateTime.UtcNow;

                // Save changes
                _cardRepository.Update(card);
                await _cardRepository.UnitOfWork.SaveChangesAsync();

                // Reorder cards in both the original and target lists if needed
                if (originalListId != targetListId)
                {
                    // Reorder original list
                    await _cardRepository.ReorderCardsAfterDeletion(originalListId, originalPosition);
                    
                    // Reorder target list
                    await _cardRepository.ReorderCardsAfterInsertion(targetListId, newPosition);
                }
                else
                {
                    // Just reorder within the same list
                    await _cardRepository.ReorderCardsAsync(targetListId, originalPosition, newPosition);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error moving card {CardId} to list {TargetListId} at position {Position}", 
                    cardId, targetListId, newPosition);
                throw;
            }
        }

        public async Task DeleteCardAsync(Guid id)
        {
            try
            {
                var card = await _cardRepository.GetByIdAsync(id);
                if (card == null)
                {
                    throw new KeyNotFoundException($"Card with ID {id} not found");
                }

                var listId = card.ListId;
                var position = card.Position;

                _cardRepository.Delete(card);
                await _cardRepository.UnitOfWork.SaveChangesAsync();

                // Reorder remaining cards
                await _cardRepository.ReorderCardsAfterDeletion(listId, position);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting card {CardId}", id);
                throw;
            }
        }
    }
}
