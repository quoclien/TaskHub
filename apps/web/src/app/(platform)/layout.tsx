'use client';

import { useParams } from 'next/navigation';

import { Navigation } from '@/components/Navigation';

// Types
interface PlatformLayoutProps {
  children: React.ReactNode;
}

// Component
export default function PlatformLayout({ children }: PlatformLayoutProps) {
  const params = useParams();
  const currentBoardId = params.boardId as string;

  // Main JSX Return
  return (
    <div className='min-h-screen bg-background'>
      <Navigation currentBoardId={currentBoardId} />
      <main className='container mx-auto px-4 py-6'>{children}</main>
    </div>
  );
}
