using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskHub.Api.Core.Interfaces;
using TaskHub.Api.Models.Dtos.Lists;

namespace TaskHub.Api.Controllers
{
    [Authorize]
    [Route("api/boards/{boardId}/[controller]")]
    public class ListsController : ApiControllerBase
    {
        private readonly IListService _listService;
        private readonly ILogger<ListsController> _logger;

        public ListsController(IListService listService, ILogger<ListsController> logger)
        {
            _listService = listService ?? throw new ArgumentNullException(nameof(listService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<ListDto>))]
        public async Task<IActionResult> GetLists(Guid boardId)
        {
            try
            {
                var lists = await _listService.GetListsByBoardIdAsync(boardId);
                return Ok(lists);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving lists for board {BoardId}", boardId);
                return StatusCode(StatusCodes.Status500InternalServerError, "Error retrieving lists");
            }
        }

        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ListDto))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetList(Guid boardId, Guid id)
        {
            try
            {
                var list = await _listService.GetListByIdAsync(id);
                if (list?.BoardId != boardId)
                {
                    return NotFound();
                }
                return Ok(list);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving list {ListId} for board {BoardId}", id, boardId);
                return StatusCode(StatusCodes.Status500InternalServerError, "Error retrieving list");
            }
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(ListDto))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CreateList(Guid boardId, [FromBody] CreateListDto createListDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var list = await _listService.CreateListAsync(boardId, createListDto);
                return CreatedAtAction(nameof(GetList), new { boardId, id = list.Id }, list);
            }
            catch (KeyNotFoundException)
            {
                return NotFound("Board not found");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating list in board {BoardId}", boardId);
                return StatusCode(StatusCodes.Status500InternalServerError, "Error creating list");
            }
        }

        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateList(Guid boardId, Guid id, [FromBody] UpdateListDto updateListDto)
        {
            if (id == Guid.Empty || !ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                await _listService.UpdateListAsync(id, updateListDto);
                return NoContent();
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating list {ListId} in board {BoardId}", id, boardId);
                return StatusCode(StatusCodes.Status500InternalServerError, "Error updating list");
            }
        }

        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteList(Guid boardId, Guid id)
        {
            try
            {
                await _listService.DeleteListAsync(id);
                return NoContent();
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting list {ListId} from board {BoardId}", id, boardId);
                return StatusCode(StatusCodes.Status500InternalServerError, "Error deleting list");
            }
        }
    }
}
