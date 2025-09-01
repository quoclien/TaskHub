'use client';

// External dependencies
import { zodResolver } from '@hookform/resolvers/zod';
import { useMutation, useQueryClient } from '@tanstack/react-query';
import { useState } from 'react';
import { useForm } from 'react-hook-form';
import { toast } from 'sonner';
import { z } from 'zod';
// @ts-ignore - sonner type issue

// Internal dependencies
import { Button } from '@/components/ui/button';
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
  DialogTrigger,
} from '@/components/ui/dialog';
import {
  Form,
  FormControl,
  FormField,
  FormItem,
  FormLabel,
  FormMessage,
} from '@/components/ui/form';
import { Input } from '@/components/ui/input';
import { Textarea } from '@/components/ui/textarea';
import { useAuthStore } from '@/hooks/useAuthStore';
import { createCard } from '@/lib/api/board';

// Types and Schema
const CreateCardSchema = z.object({
  title: z
    .string()
    .min(1, 'Card title is required')
    .max(200, 'Card title must be less than 200 characters')
    .trim(),
  description: z
    .string()
    .max(1000, 'Description must be less than 1000 characters')
    .optional()
    .transform((val) => (val === '' ? undefined : val)),
});

type CreateCardFormData = z.infer<typeof CreateCardSchema>;

interface CreateCardModalProps {
  listId: string;
  boardId: string;
  cardCount: number;
  children?: React.ReactNode;
  onSuccess?: () => void;
}

/**
 * CreateCardModal Component
 *
 * A modal dialog for creating new cards with title and description.
 * Includes form validation and optimistic updates.
 */
export function CreateCardModal({
  listId,
  boardId,
  cardCount,
  children,
  onSuccess,
}: CreateCardModalProps) {
  // State
  const [isOpen, setIsOpen] = useState(false);

  // Hooks
  const queryClient = useQueryClient();
  const { token } = useAuthStore();

  // Form
  const form = useForm<CreateCardFormData>({
    resolver: zodResolver(CreateCardSchema),
    defaultValues: {
      title: '',
      description: undefined as unknown as string | undefined,
    },
  });

  // Mutations
  const createCardMutation = useMutation({
    mutationFn: async (data: CreateCardFormData) => {
      console.log('[CreateCardModal] Attempting to create card:', {
        boardId,
        listId,
        title: data.title,
        position: cardCount,
        hasDescription: !!data.description,
        hasToken: !!token,
      });

      try {
        const result = await createCard(boardId, listId, data.title, cardCount, data.description);
        console.log('[CreateCardModal] Card created successfully:', result);
        return result;
      } catch (error) {
        console.error('[CreateCardModal] Card creation failed:', error);
        throw error;
      }
    },
    onSuccess: (data) => {
      console.log('[CreateCardModal] Mutation succeeded:', data);

      // Invalidate board query to refresh the cards
      queryClient.invalidateQueries({ queryKey: ['board', boardId] });
      console.log('[CreateCardModal] Invalidated board query:', boardId);

      // Reset form and close modal
      form.reset();
      setIsOpen(false);

      // Show success toast
      toast.success('Card created successfully!');

      // Call optional success callback
      onSuccess?.();
    },
    onError: (error: Error) => {
      console.error('[CreateCardModal] Mutation error:', {
        message: error.message,
        stack: error.stack,
        formData: form.getValues(),
      });
      toast.error(`Failed to create card: ${error.message}`);
    },
  });

  // Event Handlers
  const handleSubmit = (data: CreateCardFormData) => {
    createCardMutation.mutate(data);
  };

  const handleOpenChange = (open: boolean) => {
    setIsOpen(open);
    if (!open) {
      form.reset();
    }
  };

  // Render Helpers
  const renderTrigger = () => {
    if (children) {
      return children;
    }

    return (
      <Button
        variant='ghost'
        size='sm'
      >
        Add Card
      </Button>
    );
  };

  // Main Component
  return (
    <Dialog
      open={isOpen}
      onOpenChange={handleOpenChange}
    >
      <DialogTrigger asChild>{renderTrigger()}</DialogTrigger>

      <DialogContent className='sm:max-w-lg'>
        <DialogHeader>
          <DialogTitle>Create New Card</DialogTitle>
          <DialogDescription>
            Add a new card to organize your tasks and track progress.
          </DialogDescription>
        </DialogHeader>

        <Form {...form}>
          <form
            onSubmit={form.handleSubmit(handleSubmit)}
            className='space-y-6'
          >
            <FormField
              control={form.control}
              name='title'
              render={({ field }: any) => (
                <FormItem>
                  <FormLabel>Card Title</FormLabel>
                  <FormControl>
                    <Input
                      placeholder='Enter card title...'
                      {...field}
                      disabled={createCardMutation.isPending}
                      autoFocus
                    />
                  </FormControl>
                  <FormMessage />
                </FormItem>
              )}
            />

            <FormField
              control={form.control}
              name='description'
              render={({ field }: any) => (
                <FormItem>
                  <FormLabel>Description (Optional)</FormLabel>
                  <FormControl>
                    <Textarea
                      placeholder='Enter card description...'
                      rows={3}
                      {...field}
                      disabled={createCardMutation.isPending}
                    />
                  </FormControl>
                  <FormMessage />
                </FormItem>
              )}
            />

            <DialogFooter>
              <Button
                type='button'
                variant='outline'
                onClick={() => {
                  setIsOpen(false);
                }}
                disabled={createCardMutation.isPending}
              >
                Cancel
              </Button>
              <Button
                type='submit'
                disabled={createCardMutation.isPending}
              >
                {createCardMutation.isPending ? 'Creating...' : 'Create Card'}
              </Button>
            </DialogFooter>
          </form>
        </Form>
      </DialogContent>
    </Dialog>
  );
}
