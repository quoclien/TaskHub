using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TaskHub.Api.Core.Interfaces;
using Microsoft.Extensions.Logging;
using TaskHub.Api.Models.Dtos.Boards;

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

        /// <summary>
        /// Gets all boards for the current user
        /// </summary>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<BoardDto>))]
        public async Task<IActionResult> GetBoards()
        {
            try
            {
                // [AI_ASSUMPTION] Assuming the current user's ID is available via User.Identity.Name or similar mechanism
                // In a real application, this would come from the authentication context
                var userId = Guid.Parse(User?.Identity?.Name ?? throw new UnauthorizedAccessException("User not authenticated"));
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
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(BoardDto))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetBoard(Guid id)
        {
            try
            {
                // [AI_ASSUMPTION] Assuming the current user's ID is available via User.Identity.Name or similar mechanism
                // In a real application, this would come from the authentication context
                var userId = Guid.Parse(User?.Identity?.Name ?? throw new UnauthorizedAccessException("User not authenticated"));
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
        [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(BoardDto))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CreateBoard([FromBody] CreateBoardDto createBoardDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                // [AI_ASSUMPTION] Assuming the current user's ID is available via User.Identity.Name or similar mechanism
                // In a real application, this would come from the authentication context
                var userId = Guid.Parse(User?.Identity?.Name ?? throw new UnauthorizedAccessException("User not authenticated"));
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
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateBoard(Guid id, [FromBody] UpdateBoardDto updateBoardDto)
        {
            if (id == Guid.Empty || !ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                // [AI_ASSUMPTION] Assuming the current user's ID is available via User.Identity.Name or similar mechanism
                // In a real application, this would come from the authentication context
                var userId = Guid.Parse(User?.Identity?.Name ?? throw new UnauthorizedAccessException("User not authenticated"));
                
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
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteBoard(Guid id)
        {
            try
            {
                // [AI_ASSUMPTION] Assuming the current user's ID is available via User.Identity.Name or similar mechanism
                // In a real application, this would come from the authentication context
                var userId = Guid.Parse(User?.Identity?.Name ?? throw new UnauthorizedAccessException("User not authenticated"));
                
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
    }
}
