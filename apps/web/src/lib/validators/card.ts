import { z } from 'zod';

export const CreateCardSchema = z.object({
  title: z.string().min(1, {
    message: 'Title is required.',
  }),
});
