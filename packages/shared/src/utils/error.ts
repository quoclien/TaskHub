import { ApiError } from '../types';

/**
 * Custom error class for application-specific errors
 */
export class AppError extends Error implements ApiError {
  constructor(
    public message: string,
    public status: number = 500,
    public code?: string,
    public details?: unknown
  ) {
    super(message);
    this.name = this.constructor.name;
    
    // Maintain proper stack trace for where our error was thrown (only available on V8)
    if (Error.captureStackTrace) {
      Error.captureStackTrace(this, this.constructor);
    }
  }

  /**
   * Create a new AppError from an unknown error
   */
  static fromError(error: unknown, defaultMessage = 'An unexpected error occurred'): AppError {
    if (error instanceof AppError) {
      return error;
    }
    
    if (error instanceof Error) {
      return new AppError(
        error.message || defaultMessage,
        500,
        'UNKNOWN_ERROR',
        error
      );
    }
    
    return new AppError(
      typeof error === 'string' ? error : defaultMessage,
      500,
      'UNKNOWN_ERROR',
      error
    );
  }

  /**
   * Create a new validation error
   */
  static validationError(message: string, details?: unknown): AppError {
    return new AppError(message, 400, 'VALIDATION_ERROR', details);
  }

  /**
   * Create a new not found error
   */
  static notFound(resource: string, id?: string): AppError {
    const message = id 
      ? `${resource} with ID ${id} not found`
      : `${resource} not found`;
      
    return new AppError(message, 404, 'NOT_FOUND');
  }

  /**
   * Create a new unauthorized error
   */
  static unauthorized(message = 'Unauthorized'): AppError {
    return new AppError(message, 401, 'UNAUTHORIZED');
  }

  /**
   * Create a new forbidden error
   */
  static forbidden(message = 'Forbidden'): AppError {
    return new AppError(message, 403, 'FORBIDDEN');
  }

  /**
   * Create a new conflict error
   */
  static conflict(message: string): AppError {
    return new AppError(message, 409, 'CONFLICT');
  }

  /**
   * Create a new rate limit error
   */
  static tooManyRequests(message = 'Too many requests'): AppError {
    return new AppError(message, 429, 'TOO_MANY_REQUESTS');
  }

  /**
   * Convert the error to a plain object for serialization
   */
  toJSON(): {
    name: string;
    message: string;
    status: number;
    code?: string;
    details?: unknown;
    stack?: string;
  } {
    return {
      name: this.name,
      message: this.message,
      status: this.status,
      code: this.code,
      details: this.details,
      stack: process.env.NODE_ENV === 'development' ? this.stack : undefined,
    };
  }
}

/**
 * Check if an error is a network error
 */
export function isNetworkError(error: unknown): boolean {
  if (typeof error === 'object' && error !== null) {
    const name = 'name' in error && typeof (error as any).name === 'string' ? (error as any).name.toLowerCase() : '';
    const msg = 'message' in error && typeof (error as any).message === 'string' ? (error as any).message : '';
    const networkSubstrings = [
      'failed to fetch',
      'network request failed',
      'networkerror',
      'network error',
      'econnrefused'
    ];
    if (name === 'typeerror' && networkSubstrings.some(s => msg.toLowerCase().includes(s))) {
      return true;
    }
    if (networkSubstrings.some(s => msg.toLowerCase().includes(s))) {
      return true;
    }
  }
  return false;
}

/**
 * Get the appropriate HTTP status code for an error
 */
export function getErrorStatusCode(error: unknown): number {
  if (error instanceof AppError) {
    return error.status;
  }
  
  if (error instanceof Error) {
    switch (error.name) {
      case 'ValidationError':
        return 400;
      case 'UnauthorizedError':
      case 'JsonWebTokenError':
        return 401;
      case 'ForbiddenError':
        return 403;
      case 'NotFoundError':
        return 404;
      case 'ConflictError':
        return 409;
      case 'RateLimitExceededError':
        return 429;
      default:
        return 500;
    }
  }
  
  return 500;
}

/**
 * Get a user-friendly error message for display
 */
export function getUserFriendlyErrorMessage(error: unknown): string {
  if (error instanceof AppError) {
    return error.message;
  }
  if (error instanceof Error) {
    return error.message;
  }
  if (typeof error === 'string') {
    return error;
  }
  return 'An unexpected error occurred';
}

/**
 * Log an error with additional context
 */
export function logError(
  error: unknown,
  context: Record<string, unknown> = {},
  logger: typeof console.error = console.error
): void {
  const timestamp = new Date().toISOString();
  const errorInfo = {
    timestamp,
    error: {
      name: error instanceof Error ? error.name : 'UnknownError',
      message: error instanceof Error ? error.message : String(error),
      stack: error instanceof Error ? error.stack : undefined,
    },
    context,
  };
  
  logger(JSON.stringify(errorInfo, null, 2));
}
