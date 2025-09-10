using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskHub.Api.Common;
using TaskHub.Api.Core.Authorization;
using TaskHub.Api.Core.Interfaces;
using TaskHub.Api.Models.Dtos.Boards;
using TaskHub.Api.Models.Entities.Enums;

namespace TaskHub.Api.Controllers
{
    [Authorize]
    public class BoardsController : ApiControllerBase
    {
        private readonly IBoardService _boardService;
        private readonly ILogger<BoardsController> _logger;

        public BoardsController(IBoardService boardService, ILogger<BoardsController> logger)
        {
            _boardService = boardService ?? throw new ArgumentNullException(nameof(boardService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<BoardDto>>> GetBoards()
        {
            try
            {
                var userId = HttpContext.GetUserId();
                var boards = await _boardService.GetUserBoardsAsync(userId);
                return Ok(boards);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving boards");
                return StatusCode(StatusCodes.Status500InternalServerError, "Error retrieving boards");
            }
        }

        /// <summary>
        /// Gets a specific board by ID
        /// </summary>
        /// <param name="id">The ID of the board</param>
        [HttpGet("{id}")]
        [BoardAuthorization(BoardRole.Viewer, "id")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<BoardDto>> GetBoard(Guid id)
        {
            try
            {
                var userId = HttpContext.GetUserId();
                var board = await _boardService.GetBoardByIdAsync(id, userId);
                return board == null ? NotFound() : Ok(board);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving board with ID: {BoardId}", id);
                return StatusCode(StatusCodes.Status500InternalServerError, "Error retrieving board");
            }
        }

        /// <summary>
        /// Creates a new board
        /// </summary>
        /// <param name="createBoardDto">The board data</param>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CreateBoard([FromBody] CreateBoardDto createBoardDto)
        {
            try
            {
                var userId = HttpContext.GetUserId();
                var board = await _boardService.CreateBoardAsync(createBoardDto, userId);
                return CreatedAtAction(nameof(GetBoard), new { id = board.Id }, board);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating board");
                return StatusCode(StatusCodes.Status500InternalServerError, "Error creating board");
            }
        }

        /// <summary>
        /// Updates an existing board
        /// </summary>
        /// <param name="id">The ID of the board to update</param>
        /// <param name="updateBoardDto">The updated board data</param>
        [HttpPut("{id}")]
        [BoardAuthorization(BoardRole.Admin, "id")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateBoard(Guid id, [FromBody] UpdateBoardDto updateBoardDto)
        {
            if (id == Guid.Empty)
            {
                return BadRequest("Invalid board ID");
            }

            try
            {
                var userId = HttpContext.GetUserId();

                await _boardService.UpdateBoardAsync(id, updateBoardDto, userId);
                return NoContent();
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating board with ID: {BoardId}", id);
                return StatusCode(StatusCodes.Status500InternalServerError, "Error updating board");
            }
        }

        /// <summary>
        /// Deletes a board
        /// </summary>
        /// <param name="id">The ID of the board to delete</param>
        [HttpDelete("{id}")]
        [BoardAuthorization(BoardRole.Owner, "id")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteBoard(Guid id)
        {
            try
            {
                var userId = HttpContext.GetUserId();

                await _boardService.DeleteBoardAsync(id, userId);
                return NoContent();
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting board with ID: {BoardId}", id);
                return StatusCode(StatusCodes.Status500InternalServerError, "Error deleting board");
            }
        }

        /// <summary>
        /// Join a board as a member
        /// </summary>
        /// <param name="id">The ID of the board to join</param>
        [HttpPost("{id}/join")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> JoinBoard(Guid id)
        {
            try
            {
                var userId = HttpContext.GetUserId();
                var result = await _boardService.JoinPublicBoardAsync(id, userId);

                if (!result.Success)
                {
                    var statusCode = result.Code switch
                    {
                        BoardErrorCodes.BoardNotFound => StatusCodes.Status404NotFound,
                        BoardErrorCodes.BoardPrivate => StatusCodes.Status403Forbidden,
                        BoardErrorCodes.UserNotFound => StatusCodes.Status404NotFound,
                        _ => StatusCodes.Status400BadRequest
                    };

                    return StatusCode(statusCode, new 
                    { 
                        type = "https://tools.ietf.org/html/rfc7231",
                        title = "Join Board Failed",
                        status = statusCode,
                        detail = result.Message,
                        code = result.Code,
                        instance = HttpContext.Request.Path.ToString()
                    });
                }

                return Ok(new { message = result.Message, code = result.Code });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error joining board with ID: {BoardId}", id);
                return StatusCode(StatusCodes.Status500InternalServerError, "Error joining board");
            }
        }

        /// <summary>
        /// Leave a board (remove yourself as a member)
        /// </summary>
        /// <param name="id">The ID of the board to leave</param>
        [HttpPost("{id}/leave")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> LeaveBoard(Guid id)
        {
            try
            {
                var userId = HttpContext.GetUserId();
                var result = await _boardService.RemoveBoardMemberAsync(id, userId, userId);

                if (!result.Success)
                {
                    var statusCode = result.Code switch
                    {
                        BoardErrorCodes.BoardNotFound => StatusCodes.Status404NotFound,
                        BoardErrorCodes.NotMember => StatusCodes.Status400BadRequest,
                        BoardErrorCodes.LastOwner => StatusCodes.Status400BadRequest,
                        BoardErrorCodes.Unauthorized => StatusCodes.Status403Forbidden,
                        _ => StatusCodes.Status400BadRequest
                    };

                    return StatusCode(statusCode, new 
                    { 
                        type = "https://tools.ietf.org/html/rfc7231",
                        title = "Leave Board Failed",
                        status = statusCode,
                        detail = result.Message,
                        code = result.Code,
                        instance = HttpContext.Request.Path.ToString()
                    });
                }

                return Ok(new { message = result.Message, code = result.Code });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error leaving board with ID: {BoardId}", id);
                return StatusCode(StatusCodes.Status500InternalServerError, "Error leaving board");
            }
        }
    }
}
