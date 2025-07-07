using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.Extensions.Logging;
using TaskHub.Api.Core.Interfaces;
using TaskHub.Api.Core.Interfaces.Repositories;
using TaskHub.Api.Models.Dtos.Cards;
using CardEntity = TaskHub.Api.Models.Entities.Cards.Card;

namespace TaskHub.Api.Core.Services
{
    /// <summary>
    /// Service for managing card-related operations
    /// </summary>
    public class CardService : BaseService, ICardService
    {
        private const string CardNotFoundMessage = "Card with ID {CardId} not found";
        private const string ListNotFoundMessage = "List with ID {ListId} not found";
        
        private readonly ICardRepository _cardRepository;
        private readonly IListRepository _listRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<CardService> _logger;

        public CardService(
            ICardRepository cardRepository,
            IListRepository listRepository,
            IMapper mapper,
            ILogger<CardService> logger,
            IUnitOfWork unitOfWork) 
            : base(unitOfWork)
        {
            _cardRepository = cardRepository ?? throw new ArgumentNullException(nameof(cardRepository));
            _listRepository = listRepository ?? throw new ArgumentNullException(nameof(listRepository));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Retrieves all cards for a specific list
        /// </summary>
        /// <param name="listId">The ID of the list</param>
        /// <returns>A collection of card DTOs</returns>
        public async Task<IEnumerable<CardDto>> GetCardsByListIdAsync(Guid listId)
        {
            _logger.LogDebug("Retrieving cards for list {ListId}", listId);
            
            try
            {
                var cards = await _cardRepository.GetCardsByListIdAsync(listId);
                var result = _mapper.Map<IEnumerable<CardDto>>(cards);
                
                _logger.LogDebug("Retrieved {Count} cards for list {ListId}", result.Count(), listId);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving cards for list {ListId}", listId);
                throw;
            }
        }

        /// <summary>
        /// Retrieves a card by its ID
        /// </summary>
        /// <param name="id">The ID of the card to retrieve</param>
        /// <returns>The card DTO if found, null otherwise</returns>
        public async Task<CardDto?> GetCardByIdAsync(Guid id)
        {
            _logger.LogDebug("Retrieving card with ID {CardId}", id);
            
            try
            {
                var card = await _cardRepository.GetByIdAsync(id);
                if (card == null)
                {
                    _logger.LogWarning(CardNotFoundMessage, id);
                    return null;
                }
                
                return _mapper.Map<CardDto>(card);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving card {CardId}", id);
                throw;
            }
        }

        /// <summary>
        /// Creates a new card in the specified list
        /// </summary>
        /// <param name="listId">The ID of the list to add the card to</param>
        /// <param name="createCardDto">The card creation DTO</param>
        /// <returns>The created card DTO</returns>
        /// <exception cref="ArgumentNullException">Thrown when createCardDto is null</exception>
        /// <exception cref="KeyNotFoundException">Thrown when the specified list is not found</exception>
        public async Task<CardDto> CreateCardAsync(Guid listId, CreateCardDto createCardDto)
        {
            if (createCardDto == null)
                throw new ArgumentNullException(nameof(createCardDto));
                
            _logger.LogDebug("Creating new card in list {ListId}", listId);

            // Get the list to satisfy the required navigation property
            var list = await _listRepository.GetByIdOrThrowAsync(listId);
            if (list == null)
            {
                var message = $"List with ID {listId} not found";
                _logger.LogWarning(message);
                throw new KeyNotFoundException(message);
            }

            // Get the max position for the new card
            var maxPosition = await _cardRepository.GetMaxPositionAsync(listId);
            var position = createCardDto.Position >= 0 ? createCardDto.Position : maxPosition + 1;

            // Create the card
            var card = new CardEntity
            {
                Title = createCardDto.Title,
                Description = createCardDto.Description ?? string.Empty,
                ListId = listId,
                List = list,
                Position = position,
                AssignedTo = createCardDto.AssignedTo,
                DueDate = createCardDto.DueDate,
                IsArchived = false,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _cardRepository.AddAsync(card);
            await SaveChangesAsync();
            _logger.LogInformation("Created card {CardId} in list {ListId}", card.Id, listId);

            // Reorder other cards if necessary
            if (position <= maxPosition)
            {
                _logger.LogDebug("Reordering cards in list {ListId} after insertion at position {Position}", listId, position);
                await _cardRepository.ReorderCardsAfterInsertion(listId, position);
                await SaveChangesAsync();
            }

            return _mapper.Map<CardDto>(card);
        }

        /// <summary>
        /// Updates an existing card
        /// </summary>
        /// <param name="id">The ID of the card to update</param>
        /// <param name="updateCardDto">The card update DTO</param>
        /// <exception cref="ArgumentNullException">Thrown when updateCardDto is null</exception>
        /// <exception cref="KeyNotFoundException">Thrown when the card or target list is not found</exception>
        public async Task UpdateCardAsync(Guid id, UpdateCardDto updateCardDto)
        {
            if (updateCardDto == null)
                throw new ArgumentNullException(nameof(updateCardDto));

            _logger.LogDebug("Updating card {CardId}", id);

            try
            {
                var card = await GetCardOrThrowAsync(id);
                var originalPosition = card.Position;
                var originalListId = card.ListId;
                var positionChanged = false;
                var listChanged = false;

                UpdateCardProperties(card, updateCardDto);
                
                // Handle list change if needed
                if (updateCardDto.ListId.HasValue && updateCardDto.ListId.Value != card.ListId)
                {
                    await ValidateListExistsAsync(updateCardDto.ListId.Value);
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
                await SaveChangesAsync();
                _logger.LogInformation("Updated card {CardId}", id);

                // Handle reordering if needed
                await HandleCardReorderingAsync(card, originalListId, originalPosition, positionChanged, listChanged);
            }
            catch (Exception ex) when (ex is not KeyNotFoundException)
            {
                _logger.LogError(ex, "Error updating card {CardId}", id);
                throw;
            }
        }

        /// <summary>
        /// Updates the properties of a card based on the provided DTO
        /// </summary>
        /// <param name="card">The card to update</param>
        /// <param name="updateCardDto">The DTO containing the updated values</param>
        private void UpdateCardProperties(CardEntity card, UpdateCardDto updateCardDto)
        {
            if (updateCardDto.Title != null)
                card.Title = updateCardDto.Title;

            if (updateCardDto.Description != null)
                card.Description = updateCardDto.Description;

            if (updateCardDto.DueDate.HasValue)
                card.DueDate = updateCardDto.DueDate;

            if (updateCardDto.Metadata != null)
                card.MetadataJson = updateCardDto.Metadata;
            
            if (updateCardDto.AssignedTo.HasValue)
                card.AssignedTo = updateCardDto.AssignedTo.Value;
            
            if (updateCardDto.IsArchived.HasValue)
                card.IsArchived = updateCardDto.IsArchived.Value;
        }

        /// <summary>
        /// Moves a card to a new position within the same list or to a different list
        /// </summary>
        /// <param name="cardId">The ID of the card to move</param>
        /// <param name="targetListId">The ID of the target list</param>
        /// <param name="newPosition">The new position in the target list</param>
        /// <exception cref="KeyNotFoundException">Thrown when the card or target list is not found</exception>
        public async Task MoveCardAsync(Guid cardId, Guid targetListId, int newPosition)
        {
            _logger.LogDebug("Moving card {CardId} to list {TargetListId} at position {Position}", 
                cardId, targetListId, newPosition);

            try
            {
                var card = await GetCardOrThrowAsync(cardId);
                var originalListId = card.ListId;
                var originalPosition = card.Position;

                // If moving to a different list, verify the target list exists
                if (card.ListId != targetListId)
                {
                    await ValidateListExistsAsync(targetListId);
                    card.ListId = targetListId;
                }

                // Update position and save
                card.Position = newPosition;
                card.UpdatedAt = DateTime.UtcNow;

                _cardRepository.Update(card);
                await SaveChangesAsync();
                _logger.LogInformation("Moved card {CardId} to list {TargetListId} at position {Position}", 
                    cardId, targetListId, newPosition);

                // Handle reordering
                if (originalListId != targetListId)
                {
                    // Reorder both original and target lists
                    await _cardRepository.ReorderCardsAfterDeletion(originalListId, originalPosition);
                    await _cardRepository.ReorderCardsAfterInsertion(targetListId, newPosition);
                }
                else
                {
                    // Reorder within the same list
                    await _cardRepository.ReorderCardsAsync(targetListId, originalPosition, newPosition);
                }
            }
            catch (Exception ex) when (ex is not KeyNotFoundException)
            {
                _logger.LogError(ex, "Error moving card {CardId} to list {TargetListId} at position {Position}", 
                    cardId, targetListId, newPosition);
                throw;
            }
        }

        /// <summary>
        /// Deletes a card by its ID
        /// </summary>
        /// <param name="id">The ID of the card to delete</param>
        /// <exception cref="KeyNotFoundException">Thrown when the card is not found</exception>
        public async Task DeleteCardAsync(Guid id)
        {
            _logger.LogDebug("Deleting card {CardId}", id);

            try
            {
                var card = await GetCardOrThrowAsync(id);
                var listId = card.ListId;
                var position = card.Position;

                _cardRepository.Delete(card);
                await SaveChangesAsync();
                _logger.LogInformation("Deleted card {CardId} from list {ListId}", id, listId);

                // Reorder remaining cards
                await _cardRepository.ReorderCardsAfterDeletion(listId, position);
            }
            catch (Exception ex) when (ex is not KeyNotFoundException)
            {
                _logger.LogError(ex, "Error deleting card {CardId}", id);
                throw;
            }
        }

        #region Private Helper Methods

        /// <summary>
        /// Gets a card by ID or throws a KeyNotFoundException if not found
        /// </summary>
        private async Task<CardEntity> GetCardOrThrowAsync(Guid cardId)
        {
            var card = await _cardRepository.GetByIdAsync(cardId);
            if (card == null)
            {
                _logger.LogWarning(CardNotFoundMessage, cardId);
                throw new KeyNotFoundException($"Card with ID {cardId} not found");
            }
            return card;
        }

        /// <summary>
        /// Validates that a list with the specified ID exists
        /// </summary>
        private async Task ValidateListExistsAsync(Guid listId)
        {
            var exists = await _listRepository.ExistsAsync(l => l.Id == listId);
            if (!exists)
            {
                _logger.LogWarning(ListNotFoundMessage, listId);
                throw new KeyNotFoundException($"List with ID {listId} not found");
            }
        }

        /// <summary>
        /// Handles card reordering after updates or moves
        /// </summary>
        private async Task HandleCardReorderingAsync(
            CardEntity card, 
            Guid originalListId, 
            int originalPosition, 
            bool positionChanged, 
            bool listChanged)
        {
            if (listChanged)
            {
                _logger.LogDebug("Reordering cards after moving card {CardId} from list {OriginalListId} to {NewListId}", 
                    card.Id, originalListId, card.ListId);
                    
                // Reorder both the old and new lists
                await _cardRepository.ReorderCardsAfterDeletion(originalListId, originalPosition);
                await _cardRepository.ReorderCardsAfterInsertion(card.ListId, card.Position);
            }
            else if (positionChanged)
            {
                _logger.LogDebug("Reordering cards in list {ListId} after position change from {OldPosition} to {NewPosition}", 
                    card.ListId, originalPosition, card.Position);
                    
                // Reorder within the same list
                await _cardRepository.ReorderCardsAsync(card.ListId, originalPosition, card.Position);
            }
        }

        #endregion
    }
}
