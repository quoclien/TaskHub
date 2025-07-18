"use client";

import { useState } from 'react';
import { useMutation, useQueryClient } from '@tanstack/react-query';
import { useParams } from 'next/navigation';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { z } from 'zod';

import { CreateCardSchema } from '@/lib/validators/card';
import { Button } from '@/components/ui/button';
import { Textarea } from '@/components/ui/textarea';
import { toast } from 'sonner';
// @ts-ignore - sonner type issue
import { useAuthStore } from '@/store/auth';
import { createCard } from '@/lib/api/board';

type CreateCardFormData = z.infer<typeof CreateCardSchema>;

interface CreateCardFormProps {
  listId: string;
  cardCount: number;
  boardId: string;
}

export const CreateCardForm = ({ listId, cardCount, boardId }: CreateCardFormProps) => {
    const [isEditing, setIsEditing] = useState(false);
  const { token } = useAuthStore();
  const queryClient = useQueryClient();

  const { mutate, isPending } = useMutation({
    mutationFn: (data: { title: string }) => 
      createCard(boardId, listId, data.title, cardCount, token),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['board', boardId] });
      toast.success('Card created successfully!');
      setIsEditing(false);
      reset();
    },
    onError: (error) => {
      toast.error('Failed to create card.');
      console.error('Failed to create card:', error);
    },
  });

    const {
    register,
    handleSubmit,
    formState: { errors },
    reset,
  } = useForm<CreateCardFormData>({
    resolver: zodResolver(CreateCardSchema),
    defaultValues: {
      title: '',
    },
  });

    const onSubmit = (data: CreateCardFormData) => {
    mutate(data);
  };

  if (isEditing) {
    return (
      <div className="p-1">
        <form onSubmit={handleSubmit(onSubmit)} className="space-y-2">
          <Textarea
            {...register('title')}
            placeholder="Enter a title for this card..."
            className="w-full resize-none"
            autoFocus
          />
          {errors.title && <p className="text-red-500 text-sm">{errors.title.message}</p>}
          <div className="flex items-center gap-x-2">
            <Button type="submit" variant="default" disabled={isPending}>
              Add Card
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
    <div className="p-1">
      <Button
        onClick={() => setIsEditing(true)}
        className="w-full justify-start"
        variant="ghost"
      >
        + Add a card
      </Button>
    </div>
  );
};
