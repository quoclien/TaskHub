using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using TaskHub.Api.Core.Interfaces;
using TaskHub.Api.Models.Dtos.Cards;

namespace TaskHub.Api.Controllers
{
    [Authorize]
    [Route("api/cards")]
    [ApiController]
    public class CardDetailsController : ApiControllerBase
    {
        private readonly ICardService _cardService;
        private readonly ILogger<CardDetailsController> _logger;

        public CardDetailsController(ICardService cardService, ILogger<CardDetailsController> logger)
        {
            _cardService = cardService ?? throw new ArgumentNullException(nameof(cardService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Gets a card by its ID for detailed view (card modal)
        /// </summary>
        /// <param name="id">The card ID</param>
        /// <returns>The card details</returns>
        [HttpGet("{id}")]
        [SwaggerOperation(
            Summary = "Get card details by ID",
            Description = "Retrieves detailed information about a specific card including title, description, comments, and metadata. Used primarily for card detail modals.",
            OperationId = "GetCardById",
            Tags = new[] { "Cards" }
        )]
        [SwaggerResponse(200, "Card found successfully", typeof(CardDto))]
        [SwaggerResponse(404, "Card not found")]
        [SwaggerResponse(401, "Unauthorized - valid JWT token required")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(CardDto))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetCardById(Guid id)
        {
            try
            {
                _logger.LogDebug("Retrieving card details for card {CardId}", id);

                var card = await _cardService.GetCardByIdAsync(id);
                if (card == null)
                {
                    _logger.LogWarning("Card {CardId} not found", id);
                    return NotFound();
                }

                return Ok(card);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving card {CardId}", id);
                return StatusCode(StatusCodes.Status500InternalServerError, "Error retrieving card");
            }
        }
    }
}