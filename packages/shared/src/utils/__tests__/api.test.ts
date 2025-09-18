import { describe, it, expect } from 'vitest';
import {
  DEFAULT_API_ERROR,
  HttpError,
  createApiResponse,
  createPaginatedResponse,
  handleApiError,
  isApiError,
  isNetworkError,
  getErrorMessage,
} from '../api';

describe('api utilities', () => {
  it('should create a valid ApiResponse', () => {
    const result = createApiResponse({ foo: 'bar' }, 'ok');
    expect(result).toEqual({ data: { foo: 'bar' }, message: 'ok', success: true });
  });

  it('should create a valid PaginatedResponse', () => {
    const result = createPaginatedResponse([1, 2, 3], 30, 2, 10);
    expect(result).toEqual({
      data: [1, 2, 3],
      total: 30,
      page: 2,
      limit: 10,
      totalPages: 3,
    });
  });

  it('should throw and identify HttpError', () => {
    const err = new HttpError(404, 'Not found', 'NOT_FOUND');
    expect(isApiError(err)).toBe(true);
    expect(err.status).toBe(404);
    expect(err.message).toBe('Not found');
    expect(err.code).toBe('NOT_FOUND');
  });

  it('should rethrow HttpError in handleApiError', () => {
    const err = new HttpError(400, 'Bad');
    expect(() => handleApiError(err)).toThrow(err);
  });

  it('should wrap Error in HttpError in handleApiError', () => {
    const err = new Error('fail');
    try {
      handleApiError(err);
    } catch (e) {
      expect(isApiError(e)).toBe(true);
      expect((e as HttpError).message).toBe('fail');
      expect((e as HttpError).status).toBe(500);
    }
  });

  it('should use DEFAULT_API_ERROR for unknown error in handleApiError', () => {
    try {
      handleApiError(undefined);
    } catch (e) {
      expect(isApiError(e)).toBe(true);
      expect((e as HttpError).message).toBe(DEFAULT_API_ERROR);
    }
  });

  it('should detect network errors', () => {
    // Simulate a network error
    const err = { name: 'TypeError', message: 'Failed to fetch' };
    expect(isNetworkError(err)).toBe(true);
    expect(isNetworkError(new TypeError('Network error'))).toBe(true);
    expect(isNetworkError(new Error('Other error'))).toBe(false);
  });

  it('should get error message', () => {
    expect(getErrorMessage(new Error('msg'))).toBe('msg');
    expect(getErrorMessage('string')).toBe('string');
    expect(getErrorMessage(undefined)).toBe(DEFAULT_API_ERROR);
  });
});
