"use client";

import { useQuery } from '@tanstack/react-query';
import { useAuthStore } from '@/hooks/useAuthStore';
import { useParams } from 'next/navigation';
import { Board } from '@task-hub/shared';
import { fetchBoardById } from '@/lib/api/board';
import { CreateListForm } from './_components/CreateListForm';
import { CreateCardForm } from './_components/CreateCardForm';

const BoardIdPage = () => {
  const { token } = useAuthStore();
  const params = useParams();
  const boardId = params.boardId as string;

  const { data: board, isLoading, isError, error } = useQuery<Board, Error>({
    queryKey: ['board', boardId],
    queryFn: () => fetchBoardById(boardId, token),
    enabled: !!token && !!boardId,
  });

  if (isLoading) {
    return <div className="p-4">Loading board...</div>;
  }

  if (isError) {
    return <div className="p-4 text-red-500">Error: {error.message}</div>;
  }

  return (
    <div className="p-4">
      <h1 className="text-2xl font-bold mb-4">{board?.title}</h1>
      {/* Lists container */}
      <div className="flex gap-x-4 overflow-x-auto pb-4">
        {board.lists.map((list) => (
          <div key={list.id} className="p-4 bg-gray-200 rounded-md shrink-0 w-72">
            <h3 className="font-semibold mb-2">{list.title}</h3>
            <div className="space-y-2">
              {list.cards.map((card) => (
                <div key={card.id} className="p-2 bg-white rounded-md shadow">
                  {card.title}
                </div>
              ))}
            </div>
            <CreateCardForm listId={list.id} cardCount={list.cards.length} boardId={boardId} />
          </div>
        ))}
        <CreateListForm boardId={boardId} listCount={board.lists.length} />
      </div>
    </div>
  );
};

export default BoardIdPage;
