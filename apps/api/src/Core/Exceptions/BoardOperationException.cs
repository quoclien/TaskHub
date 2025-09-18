using System;

namespace TaskHub.Api.Core.Exceptions;

/// <summary>
/// Exception for board operation errors with standardized error codes
/// </summary>
public class BoardOperationException : Exception
{
    public string ErrorCode { get; }
    public int StatusCode { get; }
    public object? Metadata { get; }

    public BoardOperationException(string errorCode, string message, int statusCode = 400, object? metadata = null) 
        : base(message)
    {
        ErrorCode = errorCode;
        StatusCode = statusCode;
        Metadata = metadata;
    }

    public BoardOperationException(string errorCode, string message, Exception innerException, int statusCode = 400, object? metadata = null) 
        : base(message, innerException)
    {
        ErrorCode = errorCode;
        StatusCode = statusCode;
        Metadata = metadata;
    }
}
