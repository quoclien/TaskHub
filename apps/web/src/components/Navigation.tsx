'use client';

import { useQuery } from '@tanstack/react-query';
import type { Board } from '@taskhub/shared';
import { ChevronDown, LogOut, Settings, User } from 'lucide-react';
import Link from 'next/link';
import { useRouter } from 'next/navigation';
import { useEffect, useState } from 'react';

import { Button } from '@/components/ui/button';
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuSeparator,
  DropdownMenuTrigger,
} from '@/components/ui/dropdown-menu';
import { useAuthStore } from '@/hooks/useAuthStore';
import { fetchBoards } from '@/lib/api/board';

// Types
interface NavigationProps {
  currentBoardId?: string;
}

// Component
export const Navigation = ({ currentBoardId }: NavigationProps) => {
  const { token, setToken } = useAuthStore();
  const router = useRouter();
  const [lastBoardId, setLastBoardId] = useState<string | null>(null);

  // Hooks
  const { data: boards, isLoading: boardsLoading } = useQuery<Board[]>({
    queryKey: ['boards'],
    queryFn: () => fetchBoards(token),
    enabled: !!token,
  });

  // Effects
  useEffect(() => {
    if (currentBoardId) {
      setLastBoardId(currentBoardId);
    }
  }, [currentBoardId]);

  // Event Handlers
  const handleBoardSelect = (boardId: string) => {
    router.push(`/board/${boardId}`);
  };

  const handleSignOut = () => {
    setToken(null);
    router.push('/sign-in');
  };

  const handleProfile = () => {
    // TODO: Navigate to profile page when implemented
    console.log('Navigate to profile');
  };

  const handleSettings = () => {
    // TODO: Navigate to settings page when implemented
    console.log('Navigate to settings');
  };

  // Render Helpers
  const renderBoardSwitcher = () => (
    <DropdownMenu>
      <DropdownMenuTrigger asChild>
        <Button
          variant='outline'
          className='flex items-center gap-2 min-w-[200px] justify-between'
        >
          <span className='truncate'>
            {boardsLoading
              ? 'Loading boards...'
              : boards?.find((b: Board) => b.id === currentBoardId)?.title || 'Select Board'}
          </span>
          <ChevronDown className='h-4 w-4 shrink-0' />
        </Button>
      </DropdownMenuTrigger>
      <DropdownMenuContent
        align='start'
        className='w-[200px]'
      >
        {boards?.map((board: Board) => (
          <DropdownMenuItem
            key={board.id}
            onClick={() => {
              handleBoardSelect(board.id);
            }}
            className='cursor-pointer'
          >
            <div className='flex flex-col items-start'>
              <span className='font-medium'>{board.title}</span>
              {board.description && (
                <span className='text-xs text-muted-foreground truncate w-full'>
                  {board.description}
                </span>
              )}
            </div>
          </DropdownMenuItem>
        ))}
        <DropdownMenuSeparator />
        <DropdownMenuItem asChild>
          <Link
            href='/boards'
            className='cursor-pointer'
          >
            View All Boards
          </Link>
        </DropdownMenuItem>
      </DropdownMenuContent>
    </DropdownMenu>
  );

  const renderUserMenu = () => (
    <DropdownMenu>
      <DropdownMenuTrigger asChild>
        <Button
          variant='ghost'
          size='sm'
          className='flex items-center gap-2'
        >
          <User className='h-4 w-4' />
          <span>Account</span>
          <ChevronDown className='h-4 w-4' />
        </Button>
      </DropdownMenuTrigger>
      <DropdownMenuContent
        align='end'
        className='w-48'
      >
        <DropdownMenuItem
          onClick={handleProfile}
          className='cursor-pointer'
        >
          <User className='mr-2 h-4 w-4' />
          Profile
        </DropdownMenuItem>
        <DropdownMenuItem
          onClick={handleSettings}
          className='cursor-pointer'
        >
          <Settings className='mr-2 h-4 w-4' />
          Settings
        </DropdownMenuItem>
        <DropdownMenuSeparator />
        <DropdownMenuItem
          onClick={handleSignOut}
          className='cursor-pointer'
        >
          <LogOut className='mr-2 h-4 w-4' />
          Sign Out
        </DropdownMenuItem>
      </DropdownMenuContent>
    </DropdownMenu>
  );

  // Main JSX Return
  return (
    <nav className='border-b bg-background/95 backdrop-blur supports-[backdrop-filter]:bg-background/60'>
      <div className='flex h-14 items-center px-4'>
        <div className='flex items-center gap-4 flex-1'>
          <Link
            href='/'
            className='font-semibold text-lg'
          >
            TaskHub
          </Link>
          {token && renderBoardSwitcher()}
        </div>
        <div className='flex items-center gap-2'>
          {token ? (
            renderUserMenu()
          ) : (
            <div className='flex items-center gap-2'>
              <Button
                variant='ghost'
                asChild
              >
                <Link href='/sign-in'>Sign In</Link>
              </Button>
              <Button asChild>
                <Link href='/sign-up'>Sign Up</Link>
              </Button>
            </div>
          )}
        </div>
      </div>
    </nav>
  );
};
