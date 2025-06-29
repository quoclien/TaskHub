import { ApiError, ApiResponse, PaginatedResponse } from '../types';

export const DEFAULT_API_ERROR = 'An unexpected error occurred';

export class HttpError extends Error implements ApiError {
  constructor(
    public status: number,
    message: string,
    public code?: string,
    public details?: unknown
  ) {
    super(message);
    this.name = 'HttpError';
  }
}

export function createApiResponse<T>(
  data: T,
  message?: string
): ApiResponse<T> {
  return {
    data,
    message,
    success: true,
  };
}

export function createPaginatedResponse<T>(
  data: T[],
  total: number,
  page: number,
  limit: number
): PaginatedResponse<T> {
  return {
    data,
    total,
    page,
    limit,
    totalPages: Math.ceil(total / limit),
  };
}

export function handleApiError(error: unknown): never {
  if (error instanceof HttpError) {
    throw error;
  }

  if (error instanceof Error) {
    throw new HttpError(500, error.message, 'INTERNAL_SERVER_ERROR');
  }

  throw new HttpError(500, DEFAULT_API_ERROR, 'UNKNOWN_ERROR');
}

export function isApiError(error: unknown): error is HttpError {
  return error instanceof HttpError;
}

// Re-export isNetworkError from error.ts for consistency
export { isNetworkError } from './error';

export function getErrorMessage(error: unknown): string {
  if (isApiError(error)) {
    return error.message;
  }

  if (error instanceof Error) {
    return error.message;
  }

  if (typeof error === 'string') {
    return error;
  }

  return DEFAULT_API_ERROR;
}
