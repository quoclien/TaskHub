using System;

namespace TaskHub.Api.Common;

/// <summary>
/// Represents the result of a service operation with standardized error codes
/// </summary>
/// <typeparam name="T">The type of data returned on success</typeparam>
public class ServiceResult<T>
{
    public bool Success { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public T? Data { get; set; }
    public object? Metadata { get; set; }

    /// <summary>
    /// Creates a successful result with data
    /// </summary>
    public static ServiceResult<T> Ok(T data, string code = "success", string message = "Operation completed successfully")
    {
        return new ServiceResult<T>
        {
            Success = true,
            Code = code,
            Message = message,
            Data = data
        };
    }

    /// <summary>
    /// Creates a successful result without data
    /// </summary>
    public static ServiceResult<T> Ok(string code = "success", string message = "Operation completed successfully")
    {
        return new ServiceResult<T>
        {
            Success = true,
            Code = code,
            Message = message
        };
    }

    /// <summary>
    /// Creates a failed result with error information
    /// </summary>
    public static ServiceResult<T> Fail(string code, string message, object? metadata = null)
    {
        return new ServiceResult<T>
        {
            Success = false,
            Code = code,
            Message = message,
            Metadata = metadata
        };
    }
}

/// <summary>
/// Standard error codes for board operations
/// </summary>
public static class BoardErrorCodes
{
    public const string BoardNotFound = "board_not_found";
    public const string AlreadyMember = "already_member";
    public const string BoardPrivate = "board_private";
    public const string NotMember = "not_member";
    public const string LastOwner = "last_owner";
    public const string Unauthorized = "unauthorized";
    public const string Joined = "joined";
    public const string Left = "left";
    public const string UserNotFound = "user_not_found";
}
