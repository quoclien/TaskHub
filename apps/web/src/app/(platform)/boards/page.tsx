"use client";

import { useQuery } from '@tanstack/react-query';
import { useAuthStore } from '@/hooks/useAuthStore';
import Link from 'next/link';
import { Board } from '@task-hub/shared';
import { fetchBoards } from '@/lib/api/board';

const BoardsPage = () => {
  const { token } = useAuthStore();
  const { data: boards, isLoading, isError, error } = useQuery<Board[], Error>({
    queryKey: ['boards'],
    queryFn: () => fetchBoards(token),
    enabled: !!token, // Only run the query if the token exists
  });

  if (isLoading) {
    return <div>Loading boards...</div>;
  }

  if (isError) {
    return <div>Error: {error.message}</div>;
  }

  return (
    <div className="p-4">
      <h1 className="text-2xl font-bold mb-4">Your Boards</h1>
      <div className="grid grid-cols-1 sm:grid-cols-2 md:grid-cols-4 gap-4">
        {boards?.map((board) => (
          <Link href={`/board/${board.id}`} key={board.id} className="block p-4 border rounded-md hover:bg-gray-100">
            {board.title}
          </Link>
        ))}
      </div>
    </div>
  );
};

export default BoardsPage;
