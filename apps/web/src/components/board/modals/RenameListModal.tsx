'use client';

import { zodResolver } from '@hookform/resolvers/zod';
import { useMutation, useQueryClient } from '@tanstack/react-query';
import { useState } from 'react';
import { useForm } from 'react-hook-form';
import { toast } from 'sonner';
import { z } from 'zod';

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
import { updateList } from '@/lib/api/board';

const RenameListSchema = z.object({
  title: z
    .string()
    .min(1, 'List title is required')
    .max(100, 'List title must be less than 100 characters')
    .trim(),
});

type RenameListFormData = z.infer<typeof RenameListSchema>;

interface RenameListModalProps {
  boardId: string;
  listId: string;
  currentTitle: string;
  children?: React.ReactNode;
  isOpen?: boolean;
  onOpenChange?: (open: boolean) => void;
  onSuccess?: () => void;
}

export function RenameListModal({
  boardId,
  listId,
  currentTitle,
  children,
  isOpen: externalIsOpen,
  onOpenChange: externalOnOpenChange,
  onSuccess,
}: RenameListModalProps) {
  const [internalIsOpen, setInternalIsOpen] = useState(false);

  const isOpen = externalIsOpen !== undefined ? externalIsOpen : internalIsOpen;
  const setIsOpen = externalOnOpenChange || setInternalIsOpen;
  const queryClient = useQueryClient();

  const form = useForm<RenameListFormData>({
    resolver: zodResolver(RenameListSchema),
    defaultValues: {
      title: currentTitle,
    },
  });

  const renameMutation = useMutation({
    mutationFn: async (data: RenameListFormData) => {
      await updateList(boardId, listId, data.title);
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['board', boardId] });
      form.reset();
      setIsOpen(false);
      toast.success('List renamed successfully!');
      onSuccess?.();
    },
    onError: (error) => {
      toast.error('Failed to rename list');
      console.error('Failed to rename list:', error);
    },
  });

  const handleSubmit = (data: RenameListFormData) => {
    if (data.title === currentTitle) {
      setIsOpen(false);
      return;
    }
    renameMutation.mutate(data);
  };

  const handleOpenChange = (open: boolean) => {
    setIsOpen(open);
    if (!open) {
      form.reset();
    }
  };

  return (
    <Dialog
      open={isOpen}
      onOpenChange={handleOpenChange}
    >
      {children && <DialogTrigger asChild>{children}</DialogTrigger>}

      <DialogContent className='sm:max-w-md'>
        <DialogHeader>
          <DialogTitle>Rename List</DialogTitle>
          <DialogDescription>
            Enter a new name for this list. The name should be clear and descriptive.
          </DialogDescription>
        </DialogHeader>

        <Form {...form}>
          <form
            onSubmit={form.handleSubmit(handleSubmit)}
            className='space-y-4'
          >
            <FormField
              control={form.control}
              name='title'
              render={({ field }: any) => (
                <FormItem>
                  <FormLabel>List Title</FormLabel>
                  <FormControl>
                    <Input
                      placeholder='Enter list title...'
                      {...field}
                      disabled={renameMutation.isPending}
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
                disabled={renameMutation.isPending}
              >
                Cancel
              </Button>
              <Button
                type='submit'
                disabled={renameMutation.isPending}
              >
                {renameMutation.isPending ? 'Saving...' : 'Save Changes'}
              </Button>
            </DialogFooter>
          </form>
        </Form>
      </DialogContent>
    </Dialog>
  );
}
