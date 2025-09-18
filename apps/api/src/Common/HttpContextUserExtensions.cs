using System;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;

namespace TaskHub.Api.Common;

/// <summary>
/// Extensions for extracting user information from HttpContext
/// </summary>
public static class HttpContextUserExtensions
{
    /// <summary>
    /// Extracts the user ID from the current HttpContext claims in a consistent manner
    /// </summary>
    /// <param name="context">The HTTP context</param>
    /// <returns>The user ID as a Guid</returns>
    /// <exception cref="UnauthorizedAccessException">When user ID claim is not found or invalid</exception>
    public static Guid GetUserId(this HttpContext context)
    {
        var user = context.User;

        // Try different claim types in order of preference
        var userIdClaim = user.FindFirst(ClaimTypes.Name) ??
                          user.FindFirst(ClaimTypes.NameIdentifier) ??
                          user.FindFirst("sub");

        if (userIdClaim == null || string.IsNullOrWhiteSpace(userIdClaim.Value))
        {
            throw new UnauthorizedAccessException("User ID claim not found or is empty.");
        }

        if (!Guid.TryParse(userIdClaim.Value, out var userId))
        {
            throw new UnauthorizedAccessException($"User ID claim '{userIdClaim.Value}' is not a valid GUID.");
        }

        return userId;
    }
}
