import { z } from 'zod';

export const CreateListSchema = z.object({
  title: z.string({
    required_error: 'Title is required',
    invalid_type_error: 'Title must be a string',
  }).min(1, { message: 'Title must be at least 1 character long' }),
});
