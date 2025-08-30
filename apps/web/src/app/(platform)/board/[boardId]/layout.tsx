import type { Metadata } from 'next';

import { fetchBoardById } from '@/lib/api/board';

import { ModalProvider } from './_components/ModalProvider';

export async function generateMetadata({
  params,
}: {
  params: Promise<{ boardId: string }>;
}): Promise<Metadata> {
  try {
    const { boardId } = await params;
    const board = await fetchBoardById(boardId);
    return {
      title: `${board.title} - TaskHub`,
      description: board.description || `View and manage tasks in ${board.title}`,
    };
  } catch {
    return {
      title: 'Board - TaskHub',
      description: 'View and manage your board tasks',
    };
  }
}

export default function BoardLayout({ children }: { children: React.ReactNode }) {
  return (
    <>
      {children}
      {/* Global modals scoped to board routes */}
      <ModalProvider />
    </>
  );
}
