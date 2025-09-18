using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace TaskHub.Api.Common;

public class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;

    public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unhandled exception occurred");
            await HandleExceptionAsync(context, ex);
        }
    }

    private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/problem+json";

        object problemDetails = exception switch
        {
            TaskHub.Api.Core.Exceptions.BoardOperationException boardEx => new
            {
                type = "https://tools.ietf.org/html/rfc7231#section-6.5.1",
                title = GetTitleForStatusCode(boardEx.StatusCode),
                status = boardEx.StatusCode,
                detail = boardEx.Message,
                code = boardEx.ErrorCode,
                instance = context.Request.Path.ToString(),
                metadata = boardEx.Metadata
            },
            TaskHub.Api.Core.Exceptions.NotFoundException => new
            {
                type = "https://tools.ietf.org/html/rfc7231#section-6.5.4",
                title = "Not Found",
                status = StatusCodes.Status404NotFound,
                detail = exception.Message,
                code = "not_found",
                instance = context.Request.Path.ToString(),
                metadata = (object?)null
            },
            TaskHub.Api.Core.Exceptions.UnauthorizedException => new
            {
                type = "https://tools.ietf.org/html/rfc7235#section-3.1",
                title = "Unauthorized",
                status = StatusCodes.Status401Unauthorized,
                detail = exception.Message,
                code = "unauthorized",
                instance = context.Request.Path.ToString(),
                metadata = (object?)null
            },
            ArgumentException argumentEx => new
            {
                type = "https://tools.ietf.org/html/rfc7231#section-6.5.1",
                title = "Bad Request",
                status = StatusCodes.Status400BadRequest,
                detail = argumentEx.Message,
                code = "bad_request",
                instance = context.Request.Path.ToString(),
                metadata = (object?)null
            },
            _ => new
            {
                type = "https://tools.ietf.org/html/rfc7231#section-6.6.1",
                title = "Internal Server Error",
                status = StatusCodes.Status500InternalServerError,
                detail = "An error occurred while processing your request.",
                code = "internal_error",
                instance = context.Request.Path.ToString(),
                metadata = (object?)null
            }
        };

        context.Response.StatusCode = problemDetails.GetType().GetProperty("status")?.GetValue(problemDetails) as int? ?? 500;

        var json = JsonSerializer.Serialize(problemDetails, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        await context.Response.WriteAsync(json);
    }

    private static string GetTitleForStatusCode(int statusCode)
    {
        return statusCode switch
        {
            400 => "Bad Request",
            401 => "Unauthorized",
            403 => "Forbidden",
            404 => "Not Found",
            409 => "Conflict",
            422 => "Unprocessable Entity",
            500 => "Internal Server Error",
            _ => "Error"
        };
    }
}