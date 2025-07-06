using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using TaskHub.Api.Core.Interfaces;
using TaskHub.Api.Models.Dtos.Cards;

namespace TaskHub.Api.Controllers
{
    [Authorize]
    [Route("api/boards/{boardId}/lists/{listId}/[controller]")]
    public class CardsController : ApiControllerBase
    {
        private readonly ICardService _cardService;
        private readonly ILogger<CardsController> _logger;

        public CardsController(ICardService cardService, ILogger<CardsController> logger)
        {
            _cardService = cardService ?? throw new ArgumentNullException(nameof(cardService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Gets all cards for a specific list
        /// </summary>
        /// <param name="boardId">The ID of the board</param>
        /// <param name="listId">The ID of the list</param>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<CardDto>))]
        public async Task<IActionResult> GetCards(Guid boardId, Guid listId)
        {
            try
            {
                var cards = await _cardService.GetCardsByListIdAsync(listId);
                return Ok(cards);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving cards for list {ListId} in board {BoardId}", listId, boardId);
                return StatusCode(StatusCodes.Status500InternalServerError, "Error retrieving cards");
            }
        }

        /// <summary>
        /// Gets a specific card by ID
        /// </summary>
        /// <param name="boardId">The ID of the board</param>
        /// <param name="listId">The ID of the list</param>
        /// <param name="id">The ID of the card</param>
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(CardDto))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetCard(Guid boardId, Guid listId, Guid id)
        {
            try
            {
                var card = await _cardService.GetCardByIdAsync(id);
                if (card?.ListId != listId)
                {
                    return NotFound();
                }
                return Ok(card);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving card {CardId} from list {ListId} in board {BoardId}", id, listId, boardId);
                return StatusCode(StatusCodes.Status500InternalServerError, "Error retrieving card");
            }
        }

        /// <summary>
        /// Creates a new card in the specified list
        /// </summary>
        /// <param name="boardId">The ID of the board</param>
        /// <param name="listId">The ID of the list</param>
        /// <param name="createCardDto">The card data</param>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(CardDto))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CreateCard(Guid boardId, Guid listId, [FromBody] CreateCardDto createCardDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var card = await _cardService.CreateCardAsync(listId, createCardDto);
                return CreatedAtAction(nameof(GetCard), new { boardId, listId, id = card.Id }, card);
            }
            catch (KeyNotFoundException)
            {
                return NotFound("List not found");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating card in list {ListId} of board {BoardId}", listId, boardId);
                return StatusCode(StatusCodes.Status500InternalServerError, "Error creating card");
            }
        }

        /// <summary>
        /// Updates an existing card
        /// </summary>
        /// <param name="boardId">The ID of the board</param>
        /// <param name="listId">The ID of the list</param>
        /// <param name="id">The ID of the card to update</param>
        /// <param name="updateCardDto">The updated card data</param>
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateCard(Guid boardId, Guid listId, Guid id, [FromBody] UpdateCardDto updateCardDto)
        {
            if (id == Guid.Empty || !ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                await _cardService.UpdateCardAsync(id, updateCardDto);
                return NoContent();
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating card {CardId} in list {ListId} of board {BoardId}", id, listId, boardId);
                return StatusCode(StatusCodes.Status500InternalServerError, "Error updating card");
            }
        }

        /// <summary>
        /// Moves a card to a different list
        /// </summary>
        /// <param name="boardId">The ID of the board</param>
        /// <param name="listId">The ID of the current list</param>
        /// <param name="id">The ID of the card to move</param>
        /// <param name="moveCardDto">The move operation data</param>
        [HttpPost("{id}/move")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> MoveCard(Guid boardId, Guid listId, Guid id, [FromBody] MoveCardDto moveCardDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                await _cardService.MoveCardAsync(id, moveCardDto.TargetListId, moveCardDto.NewPosition);
                return NoContent();
            }
            catch (KeyNotFoundException)
            {
                return NotFound("Card or target list not found");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error moving card {CardId} to list {TargetListId}", id, moveCardDto.TargetListId);
                return StatusCode(StatusCodes.Status500InternalServerError, "Error moving card");
            }
        }

        /// <summary>
        /// Deletes a card
        /// </summary>
        /// <param name="boardId">The ID of the board</param>
        /// <param name="listId">The ID of the list</param>
        /// <param name="id">The ID of the card to delete</param>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteCard(Guid boardId, Guid listId, Guid id)
        {
            try
            {
                await _cardService.DeleteCardAsync(id);
                return NoContent();
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting card {CardId} from list {ListId} in board {BoardId}", id, listId, boardId);
                return StatusCode(StatusCodes.Status500InternalServerError, "Error deleting card");
            }
        }
    }
}
