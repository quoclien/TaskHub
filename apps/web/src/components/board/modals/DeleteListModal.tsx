'use client';

import { useMutation, useQueryClient } from '@tanstack/react-query';
import { useState } from 'react';
import { toast } from 'sonner';

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
import { deleteList } from '@/lib/api/board';

interface DeleteListModalProps {
  boardId: string;
  listId: string;
  listTitle: string;
  children?: React.ReactNode;
  isOpen?: boolean;
  onOpenChange?: (open: boolean) => void;
  onSuccess?: () => void;
}

export function DeleteListModal({
  boardId,
  listId,
  listTitle,
  children,
  isOpen: externalIsOpen,
  onOpenChange: externalOnOpenChange,
  onSuccess,
}: DeleteListModalProps) {
  const [internalIsOpen, setInternalIsOpen] = useState(false);

  const isOpen = externalIsOpen !== undefined ? externalIsOpen : internalIsOpen;
  const setIsOpen = externalOnOpenChange || setInternalIsOpen;
  const queryClient = useQueryClient();

  const deleteMutation = useMutation({
    mutationFn: async () => {
      await deleteList(boardId, listId);
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['board', boardId] });
      setIsOpen(false);
      toast.success('List deleted successfully!');
      onSuccess?.();
    },
    onError: (error) => {
      toast.error('Failed to delete list');
      console.error('Failed to delete list:', error);
    },
  });

  const handleDelete = () => {
    deleteMutation.mutate();
  };

  return (
    <Dialog
      open={isOpen}
      onOpenChange={setIsOpen}
    >
      {children && <DialogTrigger asChild>{children}</DialogTrigger>}

      <DialogContent className='sm:max-w-md'>
        <DialogHeader>
          <DialogTitle>Delete List</DialogTitle>
          <DialogDescription>
            Are you sure you want to delete &quot;{listTitle}&quot;? This will permanently remove
            the list and all its cards. This action cannot be undone.
          </DialogDescription>
        </DialogHeader>

        <DialogFooter className='gap-2 sm:gap-0'>
          <Button
            type='button'
            variant='outline'
            onClick={() => {
              setIsOpen(false);
            }}
            disabled={deleteMutation.isPending}
          >
            Cancel
          </Button>
          <Button
            type='button'
            variant='destructive'
            onClick={handleDelete}
            disabled={deleteMutation.isPending}
          >
            {deleteMutation.isPending ? 'Deleting...' : 'Delete List'}
          </Button>
        </DialogFooter>
      </DialogContent>
    </Dialog>
  );
}
