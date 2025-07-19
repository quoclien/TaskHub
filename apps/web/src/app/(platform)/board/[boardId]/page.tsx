'use client';

import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import type { Board } from '@taskhub/shared';
import { useParams } from 'next/navigation';

import { CreateCardForm } from './_components/CreateCardForm';
import { CreateListForm } from './_components/CreateListForm';

import { BoardView } from '@/components/board/BoardView';
import { ListComponent } from '@/components/board/ListComponent';
import { useAuthStore } from '@/hooks/useAuthStore';
import { fetchBoardById, moveCard, moveList } from '@/lib/api/board';

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
      const data = await fetchBoardById(boardId, token);
      console.log('Fetched board data:', data);
      return data;
    },
    enabled: !!boardId, // Remove token check to allow mock data in development
  });

  // Mutations
  const moveCardMutation = useMutation({
    mutationFn: async ({ cardId, newListId, index }: { cardId: string; newListId: string; index: number }) => {
      await moveCard(boardId, cardId, newListId, index, token);
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['board', boardId] });
    },
  });

  const moveListMutation = useMutation({
    mutationFn: async ({ oldIndex, newIndex }: { oldIndex: number; newIndex: number }) => {
      await moveList(boardId, oldIndex, newIndex, token);
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['board', boardId] });
    },
  });

  // Event Handlers
  const handleMoveCard = (cardId: string, newListId: string, index: number) => {
    moveCardMutation.mutate({ cardId, newListId, index });
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
    <div className='p-4'>
      <h1 className='text-2xl font-bold mb-4'>{board?.title}</h1>
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
              console.log('Add card to list:', listId);
            }}
            onRename={(listId) => {
              console.log('Rename list:', listId);
            }}
            onDelete={(listId) => {
              console.log('Delete list:', listId);
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
