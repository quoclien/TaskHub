import { describe, it, expect, vi } from 'vitest';
import {
  AppError,
  isNetworkError,
  getErrorStatusCode,
  getUserFriendlyErrorMessage,
  logError,
} from '../error';

describe('AppError', () => {
  it('should instantiate with correct properties', () => {
    const err = new AppError('fail', 400, 'ERR', { foo: 1 });
    expect(err.message).toBe('fail');
    expect(err.status).toBe(400);
    expect(err.code).toBe('ERR');
    expect(err.details).toEqual({ foo: 1 });
    expect(err.name).toBe('AppError');
  });

  it('should create from unknown error', () => {
    const e = AppError.fromError('string');
    expect(e).toBeInstanceOf(AppError);
    expect(e.message).toBe('string');
    const orig = new Error('orig');
    const e2 = AppError.fromError(orig);
    expect(e2).toBeInstanceOf(AppError);
    expect(e2.message).toBe('orig');
  });

  it('should create validation/notFound/unauthorized/forbidden/conflict errors', () => {
    expect(AppError.validationError('bad').code).toBe('VALIDATION_ERROR');
    expect(AppError.notFound('Res', 'id').code).toBe('NOT_FOUND');
    expect(AppError.unauthorized().code).toBe('UNAUTHORIZED');
    expect(AppError.forbidden().code).toBe('FORBIDDEN');
    expect(AppError.conflict('msg').code).toBe('CONFLICT');
  });
});

describe('isNetworkError', () => {
  it('detects network errors by name/message', () => {
    expect(isNetworkError({ name: 'TypeError', message: 'Failed to fetch' })).toBe(true);
    expect(isNetworkError(new TypeError('Network error'))).toBe(true);
    expect(isNetworkError(new Error('Other error'))).toBe(false);
  });
});

describe('getErrorStatusCode', () => {
  it('returns status from AppError', () => {
    expect(getErrorStatusCode(new AppError('fail', 401))).toBe(401);
  });
  it('returns 500 for unknown error', () => {
    expect(getErrorStatusCode('oops')).toBe(500);
  });
});

describe('getUserFriendlyErrorMessage', () => {
  it('returns message from AppError', () => {
    expect(getUserFriendlyErrorMessage(new AppError('fail'))).toBe('fail');
  });
  it('returns default for unknown error', () => {
    expect(getUserFriendlyErrorMessage(undefined)).toMatch(/unexpected/i);
  });
});

describe('logError', () => {
  it('calls logger with error and context', () => {
    const logger = vi.fn();
    logError(new Error('fail'), { foo: 1 }, logger);
    expect(logger).toHaveBeenCalled();
  });
});
