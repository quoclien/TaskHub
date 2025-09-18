using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Hosting;
using System.Linq;

namespace TaskHub.Api.Common;

/// <summary>
/// An action filter that ensures an endpoint is only accessible in the Development environment.
/// If the environment is not Development, it returns a 404 Not Found result.
/// </summary>
public class DevelopmentOnlyFilter : IActionFilter
{
    private readonly IHostEnvironment _environment;

    public DevelopmentOnlyFilter(IHostEnvironment environment)
    {
        _environment = environment;
    }

    public void OnActionExecuting(ActionExecutingContext context)
    {
        var isDevelopmentOnly = context.ActionDescriptor.EndpointMetadata
                                       .Any(em => em.GetType() == typeof(DevelopmentOnlyAttribute));

        if (isDevelopmentOnly && !_environment.IsDevelopment())
        {
            context.Result = new NotFoundResult();
        }
    }

    public void OnActionExecuted(ActionExecutedContext context) { }
}
