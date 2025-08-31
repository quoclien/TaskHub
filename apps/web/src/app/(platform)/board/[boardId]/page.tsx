'use client';

import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import type { Board } from '@taskhub/shared';
import { useParams } from 'next/navigation';
import { toast } from 'sonner';

import { BoardView } from '@/components/board/BoardView';
import { ListComponent } from '@/components/board/ListComponent';
import { Breadcrumbs } from '@/components/Breadcrumbs';
import { useAuthStore } from '@/hooks/useAuthStore';
import { deleteList, fetchBoardById, moveCard, moveList, updateList } from '@/lib/api/board';

import { CreateCardForm } from './_components/CreateCardForm';
import { CreateListForm } from './_components/CreateListForm';

const BoardIdPage = () => {
  const { token } = useAuthStore();
  const params = useParams();
  const boardId = params.boardId as string;
  const queryClient = useQueryClient();

  const {
    data: board,
    isLoading,
    isError,
    error,
  } = useQuery<Board>({
    queryKey: ['board', boardId],
    queryFn: async () => {
      console.log('Fetching board data for boardId:', boardId);
      const data = await fetchBoardById(boardId);
      console.log('Fetched board data:', data);
      return data;
    },
    enabled: !!boardId, // Remove token check to allow mock data in development
  });

  // Mutations
  const moveCardMutation = useMutation({
    mutationFn: async ({
      cardId,
      currentListId,
      newListId,
      newPosition,
    }: {
      cardId: string;
      currentListId: string;
      newListId: string;
      newPosition: number;
    }) => {
      await moveCard(boardId, cardId, currentListId, newListId, newPosition, token || undefined);
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['board', boardId] });
    },
    onError: (error) => {
      toast.error('Failed to move card');
      console.error('Failed to move card:', error);
    },
  });

  const moveListMutation = useMutation({
    mutationFn: async ({ oldIndex, newIndex }: { oldIndex: number; newIndex: number }) => {
      await moveList(boardId, oldIndex, newIndex, token || undefined);
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['board', boardId] });
      toast.success('List moved successfully');
    },
    onError: (error) => {
      toast.error('List reordering is not yet implemented');
      console.warn('List reordering error:', error.message);
    },
  });

  const updateListMutation = useMutation({
    mutationFn: async ({ listId, title }: { listId: string; title: string }) => {
      await updateList(boardId, listId, title);
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['board', boardId] });
      toast.success('List renamed successfully');
    },
    onError: (error) => {
      toast.error('Failed to rename list');
      console.error('Failed to rename list:', error);
    },
  });

  const deleteListMutation = useMutation({
    mutationFn: async (listId: string) => {
      await deleteList(boardId, listId);
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['board', boardId] });
      toast.success('List deleted successfully');
    },
    onError: (error) => {
      toast.error('Failed to delete list');
      console.error('Failed to delete list:', error);
    },
  });

  // Event Handlers
  const handleMoveCard = (cardId: string, currentListId: string, newListId: string, newPosition: number) => {
    moveCardMutation.mutate({ cardId, currentListId, newListId, newPosition });
  };

  const handleMoveList = (oldIndex: number, newIndex: number) => {
    moveListMutation.mutate({ oldIndex, newIndex });
  };

  if (isLoading) {
    return <div className='p-4'>Loading board...</div>;
  }

  if (isError) {
    return <div className='p-4 text-red-500'>Error: {error.message}</div>;
  }

  return (
    <div className='space-y-4'>
      <Breadcrumbs
        items={[
          { label: 'Home', href: '/' },
          { label: 'Board', href: '/boards' },
          { label: board?.title ?? boardId, isCurrent: true },
        ]}
      />
      <div className='flex items-center justify-between'>
        <h1 className='text-2xl font-bold'>{board?.title}</h1>
      </div>
      <BoardView
        onMoveCard={handleMoveCard}
        onMoveList={handleMoveList}
      >
        {board?.lists?.map((list) => (
          <ListComponent
            key={list.id}
            id={list.id}
            title={list.title}
            position={list.position}
            cards={list.cards}
            boardId={boardId}
            onAddCard={(listId) => {
              // Show CreateCardForm for this list
              const list = board?.lists?.find((l) => l.id === listId);
              if (list) {
                const cardCount = list.cards?.length ?? 0;
                return (
                  <CreateCardForm
                    listId={listId}
                    cardCount={cardCount}
                    boardId={boardId}
                  />
                );
              }
            }}
            onRename={(listId) => {
              // Prompt for new title
              const list = board?.lists?.find((l) => l.id === listId);
              if (list) {
                const newTitle = window.prompt('Enter new list title:', list.title);
                if (newTitle && newTitle !== list.title) {
                  updateListMutation.mutate({ listId, title: newTitle });
                }
              }
            }}
            onDelete={(listId) => {
              // Confirm deletion
              if (window.confirm('Are you sure you want to delete this list and all its cards?')) {
                deleteListMutation.mutate(listId);
              }
            }}
          />
        ))}
        <CreateListForm
          boardId={boardId}
          listCount={board?.lists?.length ?? 0}
        />
      </BoardView>
    </div>
  );
};

export default BoardIdPage;
