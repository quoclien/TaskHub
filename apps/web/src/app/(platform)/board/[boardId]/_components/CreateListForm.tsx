"use client";

import { useState } from 'react';
import { useMutation, useQueryClient } from '@tanstack/react-query';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { z } from 'zod';

import { createList } from '@/lib/api/board';
import { useAuthStore } from '@/hooks/useAuthStore';
import { CreateListSchema } from '@/lib/validators/list';
import { Button } from '@/components/ui/button';
import { toast } from 'sonner';
// @ts-ignore - sonner type issue
import { Input } from '@/components/ui/input';

type CreateListFormData = z.infer<typeof CreateListSchema>;

interface CreateListFormProps {
  boardId: string;
  listCount: number;
}

export const CreateListForm = ({ boardId, listCount }: CreateListFormProps) => {
  const [isEditing, setIsEditing] = useState(false);
  const { token } = useAuthStore();
  const queryClient = useQueryClient();

  const { mutate, isPending } = useMutation({
    mutationFn: (title: string) => {
      const position = listCount;
      return createList(boardId, title, position, token);
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['board', boardId] });
      toast.success('List created successfully!');
      setIsEditing(false);
      reset();
    },
    onError: (error) => {
      toast.error('Failed to create list.');
      console.error('Failed to create list:', error);
    },
  });

  const {

    register,
    handleSubmit,
    formState: { errors, isSubmitting },
    reset,
  } = useForm<CreateListFormData>({
    resolver: zodResolver(CreateListSchema),
    defaultValues: {
      title: '',
    },
  });

  const onSubmit = (data: CreateListFormData) => {
    mutate(data.title);
  };

  if (isEditing) {
    return (
      <div className="p-2 bg-gray-200 rounded-md shrink-0 w-72">
        <form onSubmit={handleSubmit(onSubmit)} className="space-y-2">
          <Input
            {...register('title')}
            placeholder="Enter list title..."
            className="w-full"
            autoFocus
          />
          {errors.title && <p className="text-red-500 text-sm">{errors.title.message}</p>}
          <div className="flex items-center gap-x-2">
            <Button type="submit" variant="default" disabled={isPending}>
              Add List
            </Button>
            <Button type="button" variant="ghost" onClick={() => setIsEditing(false)}>
              Cancel
            </Button>
          </div>
        </form>
      </div>
    );
  }

  return (
    <div className="p-2 shrink-0 w-72">
      <Button onClick={() => setIsEditing(true)} className="w-full">
        Add a list
      </Button>
    </div>
  );
};
