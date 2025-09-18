import { z } from 'zod';

/**
 * Create a schema for validating email addresses
 */
export const emailSchema = z.string().min(1, 'Email is required').email('Invalid email address');

/**
 * Create a schema for validating passwords
 * - At least 8 characters
 * - At least one uppercase letter
 * - At least one lowercase letter
 * - At least one number
 * - At least one special character
 */
export const passwordSchema = z
  .string()
  .min(8, 'Password must be at least 8 characters')
  .regex(/[A-Z]/, 'Password must contain at least one uppercase letter')
  .regex(/[a-z]/, 'Password must contain at least one lowercase letter')
  .regex(/[0-9]/, 'Password must contain at least one number')
  .regex(/[^A-Za-z0-9]/, 'Password must contain at least one special character');

/**
 * Create a schema for validating UUIDs
 */
export const uuidSchema = z.string().uuid('Invalid UUID format');

/**
 * Create a schema for validating pagination parameters
 */
export const paginationSchema = z.object({
  page: z.number().int().positive().default(1).optional(),
  limit: z.number().int().positive().max(100).default(10).optional(),
  sortBy: z.string().optional(),
  sortOrder: z.enum(['asc', 'desc']).default('desc').optional(),
});

/**
 * Validate data against a schema and throw a formatted error if validation fails
 * @param schema Zod schema to validate against
 * @param data Data to validate
 * @param errorMessage Optional custom error message
 * @returns The validated and parsed data
 * @throws {Error} If validation fails
 */
export function validate<T>(
  schema: z.ZodSchema<T>,
  data: unknown,
  errorMessage = 'Validation failed',
): T {
  const result = schema.safeParse(data);

  if (!result.success) {
    const formattedError = result.error.format();
    const error = new Error(errorMessage) as Error & { cause: unknown };
    error.name = 'ValidationError';
    error.cause = formattedError;
    throw error;
  }

  return result.data;
}

/**
 * Create a schema for validating search queries
 */
export const searchQuerySchema = z.object({
  query: z.string().min(1, { message: 'Search query must not be empty' }),
});

/**
 * Create a schema for validating file uploads
 */
export const fileSchema = z.object({
  name: z.string(),
  type: z.string(),
  size: z.number().max(10 * 1024 * 1024, 'File size must be less than 10MB'),
  data: z.instanceof(ArrayBuffer).or(z.instanceof(Buffer)),
});

/**
 * Create a schema for validating date ranges
 */
export const dateRangeSchema = z
  .object({
    from: z.date(),
    to: z.date(),
  })
  .refine((data) => data.from <= data.to, {
    message: 'Start date must be before end date',
    path: ['from'],
  });

/**
 * Create a schema for validating time intervals
 */
export const timeIntervalSchema = z
  .object({
    start: z.date(),
    end: z.date(),
  })
  .refine((data) => data.start < data.end, {
    message: 'Start time must be before end time',
    path: ['start'],
  });

/**
 * Validate an object against a schema and return the validated data or validation errors
 * @param schema Zod schema to validate against
 * @param data Data to validate
 * @returns An object containing the validated data and any validation errors
 */
export function validateWithErrors<T>(
  schema: z.ZodSchema<T>,
  data: unknown,
): { data?: T; errors?: Record<string, string> } {
  const result = schema.safeParse(data);

  if (!result.success) {
    const errors: Record<string, string> = {};

    result.error.errors.forEach((error) => {
      const path = error.path.join('.');
      errors[path] = error.message;
    });

    return { errors };
  }

  return { data: result.data };
}
