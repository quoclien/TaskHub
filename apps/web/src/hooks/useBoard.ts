import { useQuery } from '@tanstack/react-query';
import { fetchBoardById } from '@/lib/api/board';

export const useBoard = (boardId: string | undefined) => {
  return useQuery({
    queryKey: ['board', boardId],
    queryFn: () => fetchBoardById(boardId!),
    enabled: !!boardId,
    staleTime: 5 * 60 * 1000, // 5 minutes
  });
};

