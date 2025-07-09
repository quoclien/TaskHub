import { z } from 'zod';
import { AppError } from './error';

/**
 * Assert that a condition is true, otherwise throw an error
 * @param condition Condition to check
 * @param message Error message to throw if condition is false
 * @param status HTTP status code (default: 400)
 * @param code Error code (default: 'ASSERTION_ERROR')
 */
export function assert(
  condition: unknown,
  message: string,
  status = 400,
  code = 'ASSERTION_ERROR',
): asserts condition {
  if (!condition) {
    throw new AppError(message, status, code);
  }
}

/**
 * Assert that a value is not null or undefined
 * @param value Value to check
 * @param message Error message to throw if value is null or undefined
 * @returns The non-null value
 */
export function assertDefined<T>(
  value: T | null | undefined,
  message = 'Expected value to be defined',
): T {
  assert(value != null, message);
  return value;
}

/**
 * Assert that a value is a string
 * @param value Value to check
 * @param message Error message to throw if value is not a string
 * @returns The string value
 */
export function assertString(value: unknown, message = 'Expected a string'): string {
  assert(typeof value === 'string', message);
  return value;
}

/**
 * Assert that a value is a number
 * @param value Value to check
 * @param message Error message to throw if value is not a number
 * @returns The number value
 */
export function assertNumber(value: unknown, message = 'Expected a number'): number {
  assert(typeof value === 'number', message);
  return value;
}

/**
 * Assert that a value is an object
 * @param value Value to check
 * @param message Error message to throw if value is not an object
 * @returns The object value
 */
export function assertObject<T = Record<string, unknown>>(
  value: unknown,
  message = 'Expected an object',
): T {
  assert(value !== null && typeof value === 'object', message);
  return value as T;
}

/**
 * Assert that a value is an array
 * @param value Value to check
 * @param message Error message to throw if value is not an array
 * @returns The array value
 */
export function assertArray<T = unknown>(value: unknown, message = 'Expected an array'): T[] {
  assert(Array.isArray(value), message);
  return value as T[];
}

/**
 * Assert that a value matches a Zod schema
 * @param schema Zod schema to validate against
 * @param value Value to validate
 * @param message Error message to throw if validation fails
 * @returns The validated value
 */
export function assertSchema<T>(
  schema: z.ZodSchema<T>,
  value: unknown,
  message = 'Validation failed',
): T {
  try {
    return schema.parse(value);
  } catch (error) {
    if (error instanceof z.ZodError) {
      throw new AppError(message, 400, 'VALIDATION_ERROR', error.errors);
    }
    throw error;
  }
}

/**
 * Assert that a value is an instance of a specific class
 * @param value Value to check
 * @param constructor Constructor function to check against
 * @param message Error message to throw if check fails
 * @returns The value as an instance of the constructor
 */
export function assertInstanceOf<T>(
  value: unknown,
  constructor: new (...args: any[]) => T,
  message = `Expected instance of ${constructor.name}`,
): T {
  assert(value instanceof constructor, message);
  return value;
}

/**
 * Assert that a value is a valid enum value
 * @param enumObj Enum object to check against
 * @param value Value to check
 * @param message Error message to throw if check fails
 * @returns The value as an enum value
 */
export function assertEnumValue<T extends Record<string, string | number>>(
  enumObj: T,
  value: unknown,
  message = 'Invalid enum value',
): T[keyof T] {
  const validValues = Object.values(enumObj);
  assert(
    validValues.includes(value as T[keyof T]),
    `${message}. Expected one of: ${validValues.join(', ')}`,
  );
  return value as T[keyof T];
}

/**
 * Assert that a value is a valid UUID
 * @param value Value to check
 * @param message Error message to throw if check fails
 * @returns The UUID string
 */
export function assertUuid(value: unknown, message = 'Invalid UUID format'): string {
  const uuidRegex = /^[0-9a-f]{8}-[0-9a-f]{4}-[1-5][0-9a-f]{3}-[89ab][0-9a-f]{3}-[0-9a-f]{12}$/i;
  const strValue = String(value);
  assert(uuidRegex.test(strValue), message);
  return strValue;
}

/**
 * Assert that a value is a valid email address
 * @param value Value to check
 * @param message Error message to throw if check fails
 * @returns The email string
 */
export function assertEmail(value: unknown, message = 'Invalid email address'): string {
  const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
  const strValue = String(value);
  assert(emailRegex.test(strValue), message);
  return strValue;
}

/**
 * Assert that a value is within a specified range
 * @param value Value to check
 * @param min Minimum value (inclusive)
 * @param max Maximum value (inclusive)
 * @param message Error message to throw if check fails
 * @returns The value if it's within the range
 */
export function assertInRange(
  value: number,
  min: number,
  max: number,
  message = `Value must be between ${min} and ${max}`,
): number {
  assert(value >= min && value <= max, message);
  return value;
}
