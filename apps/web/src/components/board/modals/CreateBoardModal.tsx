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
import { useAuthStore } from '@/hooks/useAuthStore';

// API Configuration
const API_BASE = process.env.NEXT_PUBLIC_API_BASE_URL || 'http://localhost:5205/api';

// Types and Schema
const CreateBoardSchema = z.object({
  title: z
    .string()
    .min(1, 'Board title is required')
    .max(100, 'Board title must be less than 100 characters')
    .trim(),
});

type CreateBoardFormData = z.infer<typeof CreateBoardSchema>;

interface CreateBoardModalProps {
  children?: React.ReactNode;
  onSuccess?: () => void;
}

/**
 * CreateBoardModal Component
 *
 * A modal dialog for creating new boards with form validation.
 * Includes optimistic updates and proper error handling.
 */
export function CreateBoardModal({ children, onSuccess }: CreateBoardModalProps) {
  // State
  const [isOpen, setIsOpen] = useState(false);

  // Hooks
  const { token } = useAuthStore();
  const queryClient = useQueryClient();

  // Form
  const form = useForm<CreateBoardFormData>({
    resolver: zodResolver(CreateBoardSchema),
    defaultValues: {
      title: '',
    },
  });

  // Mutations
  const createBoardMutation = useMutation({
    mutationFn: async (data: CreateBoardFormData) => {
      if (!token) {
        throw new Error('Not authenticated');
      }

      const response = await fetch(`${API_BASE}/Boards`, {
        method: 'POST',
        headers: {
          Authorization: `Bearer ${token}`,
          'Content-Type': 'application/json',
        },
        body: JSON.stringify(data),
      });

      if (!response.ok) {
        throw new Error('Failed to create board');
      }

      return response.json();
    },
    onSuccess: () => {
      // Invalidate boards query to refresh the list
      queryClient.invalidateQueries({ queryKey: ['boards'] });

      // Reset form and close modal
      form.reset();
      setIsOpen(false);

      // Show success toast
      toast.success('Board created successfully!');

      // Call optional success callback
      onSuccess?.();
    },
    onError: (error) => {
      toast.error('Failed to create board');
      console.error('Failed to create board:', error);
    },
  });

  // Event Handlers
  const handleSubmit = (data: CreateBoardFormData) => {
    createBoardMutation.mutate(data);
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

    return <Button>Create Board</Button>;
  };

  // Main Component
  return (
    <Dialog
      open={isOpen}
      onOpenChange={handleOpenChange}
    >
      <DialogTrigger asChild>{renderTrigger()}</DialogTrigger>

      <DialogContent className='sm:max-w-md'>
        <DialogHeader>
          <DialogTitle>Create New Board</DialogTitle>
          <DialogDescription>
            Create a new board to organize your tasks and collaborate with your team.
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
              render={({ field }) => (
                <FormItem>
                  <FormLabel>Board Title</FormLabel>
                  <FormControl>
                    <Input
                      placeholder='Enter board title...'
                      {...field}
                      disabled={createBoardMutation.isPending}
                      autoFocus
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
                disabled={createBoardMutation.isPending}
              >
                Cancel
              </Button>
              <Button
                type='submit'
                disabled={createBoardMutation.isPending}
              >
                {createBoardMutation.isPending ? 'Creating...' : 'Create Board'}
              </Button>
            </DialogFooter>
          </form>
        </Form>
      </DialogContent>
    </Dialog>
  );
}
