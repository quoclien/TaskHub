import type { Metadata } from 'next';

export const metadata: Metadata = {
  title: 'Your Boards - TaskHub',
  description: 'View and manage all your project boards in TaskHub.',
};

export default function BoardsLayout({ children }: { children: React.ReactNode }) {
  return children;
}
