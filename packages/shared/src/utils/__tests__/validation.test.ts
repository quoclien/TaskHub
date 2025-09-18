import { describe, it, expect } from 'vitest';
import {
  emailSchema,
  passwordSchema,
  uuidSchema,
  paginationSchema,
  searchQuerySchema,
  validate,
  validateWithErrors,
} from '../validation';

describe('validation utilities', () => {
  it('validates emailSchema', () => {
    expect(emailSchema.safeParse('test@email.com').success).toBe(true);
    expect(emailSchema.safeParse('bad').success).toBe(false);
  });
  it('validates passwordSchema', () => {
    expect(passwordSchema.safeParse('Password1!').success).toBe(true);
    expect(passwordSchema.safeParse('short').success).toBe(false);
    expect(passwordSchema.safeParse('nouppercase1!').success).toBe(false);
    expect(passwordSchema.safeParse('NOLOWERCASE1!').success).toBe(false);
    expect(passwordSchema.safeParse('NoNumber!').success).toBe(false);
    expect(passwordSchema.safeParse('NoSpecial1').success).toBe(false);
  });
  it('validates uuidSchema', () => {
    expect(uuidSchema.safeParse('550e8400-e29b-41d4-a716-446655440000').success).toBe(true);
    expect(uuidSchema.safeParse('not-a-uuid').success).toBe(false);
  });
  it('validates paginationSchema', () => {
    const valid = { page: 2, limit: 10, sortBy: 'name', sortOrder: 'asc' };
    expect(paginationSchema.safeParse(valid).success).toBe(true);
    expect(paginationSchema.safeParse({ page: -1 }).success).toBe(false);
  });
  it('validates searchQuerySchema', () => {
    expect(searchQuerySchema.safeParse({ query: 'foo' }).success).toBe(true);
    expect(searchQuerySchema.safeParse({ query: '' }).success).toBe(false);
  });
  it('validate throws on invalid data', () => {
    expect(() => validate(emailSchema, 'bad')).toThrow();
    expect(validate(emailSchema, 'test@email.com')).toBe('test@email.com');
  });
  it('validateWithErrors returns errors for invalid', () => {
    const { data, errors } = validateWithErrors(emailSchema, 'bad');
    expect(data).toBeUndefined();
    expect(errors).toBeDefined();
    const { data: ok, errors: err } = validateWithErrors(emailSchema, 'test@email.com');
    expect(ok).toBe('test@email.com');
    expect(err).toBeUndefined();
  });
});
