using Microsoft.AspNetCore.Mvc;

namespace TaskHub.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public abstract class ApiControllerBase : ControllerBase
    {
        protected IActionResult OkOrNotFound<T>(T? result) where T : class
            => result == null ? NotFound() : Ok(result);

        protected new IActionResult CreatedAtAction<T>(string actionName, object routeValues, T value)
            => base.CreatedAtAction(actionName, routeValues, value);
    }
}
