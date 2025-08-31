using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.Extensions.Logging;
using TaskHub.Api.Core.Exceptions;
using TaskHub.Api.Core.Interfaces;
using TaskHub.Api.Core.Interfaces.Repositories;
using TaskHub.Api.Models.Dtos.Cards;
using CardEntity = TaskHub.Api.Models.Entities.Cards.Card;

namespace TaskHub.Api.Core.Services
{
    public class CardService : BaseService, ICardService
    {
        private const string CardNotFound = "Card not found";
        private const string ListNotFound = "List not found";
        private const string InvalidCardId = "Card ID cannot be empty.";
        private const string InvalidListId = "List ID cannot be empty.";

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

        public async Task<IEnumerable<CardDto>> GetCardsByListIdAsync(Guid listId)
        {
            if (listId == Guid.Empty)
                throw new ArgumentException(InvalidListId, nameof(listId));

            _logger.LogDebug("Retrieving cards for list {ListId}", listId);
            var cards = await _cardRepository.GetCardsByListIdAsync(listId);
            return _mapper.Map<IEnumerable<CardDto>>(cards);
        }

        public async Task<CardDto?> GetCardByIdAsync(Guid id)
        {
            if (id == Guid.Empty)
                throw new ArgumentException(InvalidCardId, nameof(id));

            _logger.LogDebug("Retrieving card with ID {CardId}", id);
            var card = await _cardRepository.GetByIdAsync(id);

            if (card == null)
            {
                _logger.LogWarning("Card with ID {CardId} not found.", id);
                return null;
            }

            return _mapper.Map<CardDto>(card);
        }

        public async Task<CardDto> CreateCardAsync(Guid listId, CreateCardDto createCardDto)
        {
            if (listId == Guid.Empty)
                throw new ArgumentException(InvalidListId, nameof(listId));
            if (createCardDto == null)
                throw new ArgumentNullException(nameof(createCardDto));

            _logger.LogDebug("Creating new card '{CardTitle}' in list {ListId}", createCardDto.Title, listId);

            await ValidateListExistsAsync(listId);

            var maxPosition = await _cardRepository.GetMaxPositionAsync(listId);
            var card = _mapper.Map<CardEntity>(createCardDto);

            card.ListId = listId;
            card.Position = createCardDto.Position >= 0 ? createCardDto.Position : maxPosition + 1;
            card.CreatedAt = DateTime.UtcNow;
            card.UpdatedAt = DateTime.UtcNow;
            
            // Ensure metadata is never null - initialize with empty dictionary if not set
            card.Metadata ??= new Dictionary<string, object>();

            try
            {
                await _cardRepository.AddAsync(card);

                if (card.Position <= maxPosition)
                {
                    await _cardRepository.ReorderCardsAfterInsertion(listId, card.Position);
                }

                await SaveChangesAsync();
                _logger.LogInformation("Created card {CardId} in list {ListId}", card.Id, listId);

                return _mapper.Map<CardDto>(card);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating card in list {ListId}", listId);
                throw new ApplicationException("An error occurred while creating the card.", ex);
            }
        }

        public async Task UpdateCardAsync(Guid id, UpdateCardDto updateCardDto)
        {
            if (id == Guid.Empty)
                throw new ArgumentException(InvalidCardId, nameof(id));
            if (updateCardDto == null)
                throw new ArgumentNullException(nameof(updateCardDto));

            _logger.LogDebug("Updating card {CardId}", id);

            var card = await GetCardOrThrowAsync(id);
            var originalListId = card.ListId;
            var originalPosition = card.Position;

            _mapper.Map(updateCardDto, card);
            card.UpdatedAt = DateTime.UtcNow;

            var listChanged = updateCardDto.ListId.HasValue && updateCardDto.ListId.Value != originalListId;
            if (listChanged)
            {
                await ValidateListExistsAsync(card.ListId);
            }

            try
            {
                _cardRepository.Update(card);
                await HandleCardReorderingAsync(card, originalListId, originalPosition, listChanged);
                await SaveChangesAsync();
                _logger.LogInformation("Updated card {CardId}", id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating card {CardId}", id);
                throw new ApplicationException("An error occurred while updating the card.", ex);
            }
        }

        public async Task MoveCardAsync(Guid cardId, Guid targetListId, int newPosition)
        {
            if (cardId == Guid.Empty)
                throw new ArgumentException(InvalidCardId, nameof(cardId));
            if (targetListId == Guid.Empty)
                throw new ArgumentException(InvalidListId, nameof(targetListId));

            _logger.LogDebug("Moving card {CardId} to list {TargetListId} at position {Position}", cardId, targetListId, newPosition);

            var card = await GetCardOrThrowAsync(cardId);
            var originalListId = card.ListId;
            var originalPosition = card.Position;

            if (originalListId != targetListId)
            {
                await ValidateListExistsAsync(targetListId);
            }

            card.ListId = targetListId;
            card.Position = newPosition;
            card.UpdatedAt = DateTime.UtcNow;

            try
            {
                _cardRepository.Update(card);
                await HandleCardReorderingAsync(card, originalListId, originalPosition, originalListId != targetListId);
                await SaveChangesAsync();
                _logger.LogInformation("Moved card {CardId} to list {TargetListId} at position {Position}", cardId, targetListId, newPosition);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error moving card {CardId}", ex);
                throw new ApplicationException("An error occurred while moving the card.", ex);
            }
        }

        public async Task DeleteCardAsync(Guid id)
        {
            if (id == Guid.Empty)
                throw new ArgumentException(InvalidCardId, nameof(id));

            _logger.LogDebug("Deleting card {CardId}", id);

            var card = await GetCardOrThrowAsync(id);
            var listId = card.ListId;
            var position = card.Position;

            try
            {
                _cardRepository.Delete(card);
                await _cardRepository.ReorderCardsAfterDeletion(listId, position);
                await SaveChangesAsync();
                _logger.LogInformation("Deleted card {CardId} from list {ListId}", id, listId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting card {CardId}", id);
                throw new ApplicationException("An error occurred while deleting the card.", ex);
            }
        }

        private async Task<CardEntity> GetCardOrThrowAsync(Guid cardId)
        {
            var card = await _cardRepository.GetByIdAsync(cardId);
            return card ?? throw new NotFoundException(CardNotFound);
        }

        private async Task ValidateListExistsAsync(Guid listId)
        {
            var exists = await _listRepository.ExistsAsync(l => l.Id == listId);
            if (!exists)
            {
                throw new NotFoundException(ListNotFound);
            }
        }

        private async Task HandleCardReorderingAsync(CardEntity card, Guid originalListId, int originalPosition, bool listChanged)
        {
            var positionChanged = card.Position != originalPosition;

            if (listChanged)
            {
                _logger.LogDebug("Reordering after moving card {CardId} from list {OriginalListId} to {NewListId}", card.Id, originalListId, card.ListId);
                await _cardRepository.ReorderCardsAfterDeletion(originalListId, originalPosition);
                await _cardRepository.ReorderCardsAfterInsertion(card.ListId, card.Position);
            }
            else if (positionChanged)
            {
                _logger.LogDebug("Reordering in list {ListId} after position change from {OldPosition} to {NewPosition}", card.ListId, originalPosition, card.Position);
                await _cardRepository.ReorderCardsAsync(card.ListId, originalPosition, card.Position);
            }
        }
    }
}
