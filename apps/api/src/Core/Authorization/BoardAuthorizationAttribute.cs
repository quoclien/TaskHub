using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Security.Claims;
using TaskHub.Api.Core.Interfaces.Repositories;
using TaskHub.Api.Models.Entities.Enums;

namespace TaskHub.Api.Core.Authorization
{
    /// <summary>
    /// Custom authorization attribute for board-level access control
    /// </summary>
    public class BoardAuthorizationAttribute : Attribute, IAsyncAuthorizationFilter
    {
        private readonly BoardRole _minimumRole;
        private readonly string _boardIdParameterName;

        /// <summary>
        /// Initializes a new instance of the BoardAuthorizationAttribute
        /// </summary>
        /// <param name="minimumRole">The minimum role required to access the resource</param>
        /// <param name="boardIdParameterName">The name of the route parameter containing the board ID (default: "boardId")</param>
        public BoardAuthorizationAttribute(BoardRole minimumRole, string boardIdParameterName = "boardId")
        {
            _minimumRole = minimumRole;
            _boardIdParameterName = boardIdParameterName;
        }

        public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
        {
            // Check if user is authenticated
            if (!context.HttpContext.User.Identity?.IsAuthenticated ?? false)
            {
                context.Result = new UnauthorizedResult();
                return;
            }

            // Get user ID from claims
            var userIdClaim = context.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)
                           ?? context.HttpContext.User.FindFirst("sub");

            if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
            {
                context.Result = new UnauthorizedResult();
                return;
            }

            // Get board ID from route
            if (!context.RouteData.Values.TryGetValue(_boardIdParameterName, out var boardIdValue) ||
                !Guid.TryParse(boardIdValue?.ToString(), out var boardId))
            {
                context.Result = new BadRequestObjectResult($"Invalid or missing {_boardIdParameterName} parameter");
                return;
            }

            // Get board repository from DI
            var boardRepository = context.HttpContext.RequestServices.GetService<IBoardRepository>();
            if (boardRepository == null)
            {
                context.Result = new StatusCodeResult(500);
                return;
            }

            try
            {
                // Check user's role in the board
                var userRole = await boardRepository.GetUserRoleInBoardAsync(userId, boardId);

                if (userRole == null)
                {
                    context.Result = new ForbidResult("You do not have access to this board");
                    return;
                }

                // Check if user has sufficient role
                if (!HasSufficientRole(userRole.Value, _minimumRole))
                {
                    context.Result = new ForbidResult("You do not have sufficient permissions for this action");
                    return;
                }

                // Authorization successful
            }
            catch (Exception)
            {
                // Log the exception (you might want to inject ILogger here)
                context.Result = new StatusCodeResult(500);
                return;
            }
        }

        /// <summary>
        /// Determines if the user's role meets the minimum required role
        /// </summary>
        /// <param name="userRole">The user's current role</param>
        /// <param name="minimumRole">The minimum required role</param>
        /// <returns>True if the user has sufficient permissions</returns>
        private static bool HasSufficientRole(BoardRole userRole, BoardRole minimumRole)
        {
            // Lower enum values indicate higher permissions (Owner = 1, Admin = 2, Member = 3, Viewer = 4)
            return (int)userRole <= (int)minimumRole;
        }
    }
}