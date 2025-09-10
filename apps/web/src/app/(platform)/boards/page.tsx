'use client';

import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import type { Board, BoardMember } from '@taskhub/shared';
import { formatDate } from '@taskhub/shared';
import { Plus, Trash2, Calendar, Users, Lock, Globe, UserPlus, UserMinus } from 'lucide-react';
import Link from 'next/link';
import { useState } from 'react';

import { Button } from '@/components/ui/button';
import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
  DialogDescription,
} from '@/components/ui/dialog';
import { Input } from '@/components/ui/input';
import { useAuthStore } from '@/hooks/useAuthStore';
import { fetchBoards, createBoard, deleteBoard, joinBoard, leaveBoard } from '@/lib/api/board';

const BoardCard = ({
  board,
  onDelete,
  onJoin,
  onLeave,
  currentUserId,
}: {
  board: Board;
  onDelete: (id: string, boardTitle: string) => void;
  onJoin: (boardId: string) => void;
  onLeave: (boardId: string) => void;
  currentUserId?: string;
}) => {
  const isUserMember = currentUserId ? board.members?.some(member => member.userId === currentUserId) : false;
  const isOwner = currentUserId ? board.ownerId === currentUserId : false;
  const canJoin = board.isPublic && !isUserMember && !isOwner;
  const canLeave = (isUserMember || isOwner) && currentUserId;

  // Debug logging
  if (process.env.NODE_ENV === 'development') {
    console.log(`Board ${board.title}:`, {
      boardId: board.id,
      currentUserId,
      boardOwnerId: board.ownerId,
      isUserMember,
      isOwner,
      canJoin,
      canLeave,
      membersCount: board.members?.length || 0,
      members: board.members?.map(m => ({ userId: m.userId, role: m.role, userName: m.userName })) || [],
      isPublic: board.isPublic,
    });
  }
  return (
    <div className='group relative bg-white dark:bg-gray-800 rounded-lg border border-gray-200 dark:border-gray-700 shadow-sm hover:shadow-md transition-all duration-200 overflow-hidden'>
      <Link
        href={`/board/${board.id}`}
        className='block p-6'
      >
        <div className='flex flex-col h-full'>
          <h3 className='text-lg font-semibold text-gray-900 dark:text-gray-100 mb-2 group-hover:text-blue-600 dark:group-hover:text-blue-400 transition-colors'>
            {board.title}
          </h3>
          {board.description && (
            <p className='text-gray-600 dark:text-gray-400 text-sm mb-4 line-clamp-2 flex-1'>
              {board.description}
            </p>
          )}
          <div className='flex items-center justify-between text-xs text-gray-500 dark:text-gray-500'>
            <div className='flex items-center space-x-3'>
              <div className='flex items-center space-x-1'>
                <Calendar className='w-3 h-3' />
                <span>{formatDate(board.createdAt, { month: 'short', day: 'numeric' })}</span>
              </div>
              <div className='flex items-center space-x-2'>
                <Users className='w-3 h-3' />
                <span>{board.members?.length || 0} members</span>
                {board.members && board.members.length > 0 && (
                  <div className='flex -space-x-1'>
                    {board.members.slice(0, 3).map((member) => (
                      <div
                        key={member.userId}
                        className='w-5 h-5 rounded-full bg-gradient-to-br from-blue-400 to-purple-500 flex items-center justify-center text-xs font-medium text-white border border-white dark:border-gray-800'
                        title={`${member.userName} (${member.role})`}
                      >
                        {member.userName.charAt(0).toUpperCase()}
                      </div>
                    ))}
                    {board.members.length > 3 && (
                      <div className='w-5 h-5 rounded-full bg-gray-300 dark:bg-gray-600 flex items-center justify-center text-xs font-medium text-gray-700 dark:text-gray-300 border border-white dark:border-gray-800'>
                        +{board.members.length - 3}
                      </div>
                    )}
                  </div>
                )}
              </div>
            </div>
            <div className='flex items-center space-x-1'>
              {board.isPublic ? (
                <div className='flex items-center space-x-1 text-green-600 dark:text-green-400'>
                  <Globe className='w-3 h-3' />
                  <span>Public</span>
                </div>
              ) : (
                <div className='flex items-center space-x-1 text-amber-600 dark:text-amber-400'>
                  <Lock className='w-3 h-3' />
                  <span>Private</span>
                </div>
              )}
            </div>
          </div>
        </div>
      </Link>

      <div className='absolute top-3 right-3 opacity-0 group-hover:opacity-100 transition-opacity flex space-x-1'>
        {canJoin && (
          <button
            onClick={(e) => {
              e.preventDefault();
              e.stopPropagation();
              onJoin(board.id);
            }}
            onKeyDown={(e) => {
              if (e.key === 'Enter' || e.key === ' ') {
                e.preventDefault();
                e.stopPropagation();
                onJoin(board.id);
              }
            }}
            className='p-1 rounded-md hover:bg-green-50 dark:hover:bg-green-900/20 text-green-600 dark:text-green-400 focus:opacity-100 focus:outline-none focus:ring-2 focus:ring-green-500 focus:ring-offset-2'
            title='Join board'
            tabIndex={0}
          >
            <UserPlus className='w-4 h-4' />
          </button>
        )}
        {canLeave && (
          <button
            onClick={(e) => {
              e.preventDefault();
              e.stopPropagation();
              onLeave(board.id);
            }}
            onKeyDown={(e) => {
              if (e.key === 'Enter' || e.key === ' ') {
                e.preventDefault();
                e.stopPropagation();
                onLeave(board.id);
              }
            }}
            className='p-1 rounded-md hover:bg-orange-50 dark:hover:bg-orange-900/20 text-orange-600 dark:text-orange-400 focus:opacity-100 focus:outline-none focus:ring-2 focus:ring-orange-500 focus:ring-offset-2'
            title={isOwner ? 'Leave board (as owner)' : 'Leave board'}
            tabIndex={0}
          >
            <UserMinus className='w-4 h-4' />
          </button>
        )}
        <button
          onClick={(e) => {
            e.preventDefault();
            e.stopPropagation();
            onDelete(board.id, board.title);
          }}
          onKeyDown={(e) => {
            if (e.key === 'Enter' || e.key === ' ') {
              e.preventDefault();
              e.stopPropagation();
              onDelete(board.id, board.title);
            }
          }}
          className='p-1 rounded-md hover:bg-red-50 dark:hover:bg-red-900/20 text-red-600 dark:text-red-400 focus:opacity-100 focus:outline-none focus:ring-2 focus:ring-red-500 focus:ring-offset-2'
          title='Delete board'
          tabIndex={0}
        >
          <Trash2 className='w-4 h-4' />
        </button>
      </div>
    </div>
  );
};

const CreateBoardModal = ({
  isOpen,
  onClose,
  onCreateBoard,
}: {
  isOpen: boolean;
  onClose: () => void;
  onCreateBoard: (board: { title: string; description?: string }) => void;
}) => {
  const [title, setTitle] = useState('');
  const [description, setDescription] = useState('');

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    if (title.trim()) {
      onCreateBoard({ title: title.trim(), description: description.trim() || undefined });
      setTitle('');
      setDescription('');
      onClose();
    }
  };

  return (
    <Dialog
      open={isOpen}
      onOpenChange={onClose}
    >
      <DialogContent className='sm:max-w-md'>
        <DialogHeader>
          <DialogTitle>Create New Board</DialogTitle>
        </DialogHeader>
        <form
          onSubmit={handleSubmit}
          className='space-y-4'
        >
          <div>
            <label
              htmlFor='title'
              className='block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1'
            >
              Board Title
            </label>
            <Input
              id='title'
              type='text'
              value={title}
              onChange={(e) => {
                setTitle(e.target.value);
              }}
              placeholder='Enter board title...'
              className='w-full'
              required
            />
          </div>
          <div>
            <label
              htmlFor='description'
              className='block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1'
            >
              Description (optional)
            </label>
            <Input
              id='description'
              type='text'
              value={description}
              onChange={(e) => {
                setDescription(e.target.value);
              }}
              placeholder='Enter board description...'
              className='w-full'
            />
          </div>
          <div className='flex space-x-3 pt-4'>
            <Button
              type='submit'
              className='flex-1'
            >
              Create Board
            </Button>
            <Button
              type='button'
              variant='outline'
              onClick={onClose}
              className='flex-1'
            >
              Cancel
            </Button>
          </div>
        </form>
      </DialogContent>
    </Dialog>
  );
};

const BoardsPage = () => {
  const { token, userId: currentUserId } = useAuthStore();
  const queryClient = useQueryClient();
  
  // Debug auth state
  if (process.env.NODE_ENV === 'development') {
    console.log('🔍 BoardsPage auth state:', {
      hasToken: !!token,
      currentUserId,
      tokenLength: token?.length,
    });
  }
  const [isCreateModalOpen, setIsCreateModalOpen] = useState(false);
  const [isDeleteModalOpen, setIsDeleteModalOpen] = useState(false);
  const [confirmTitle, setConfirmTitle] = useState('');
  const [boardToDelete, setBoardToDelete] = useState<{ id: string; title: string } | null>(null);

  const {
    data: boards,
    isLoading,
    isError,
    error,
  } = useQuery<Board[]>({
    queryKey: ['boards'],
    queryFn: () => fetchBoards(token || undefined),
    enabled: true, // Always enable the query since we have mock data
  });

  const createBoardMutation = useMutation({
    mutationFn: (board: { title: string; description?: string }) => createBoard(board, token || undefined),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['boards'] });
    },
    onError: (error) => {
      console.error('Failed to create board:', error);
    },
  });

  const deleteBoardMutation = useMutation({
    mutationFn: (boardId: string) => deleteBoard(boardId, token || undefined),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['boards'] });
      setIsDeleteModalOpen(false);
      setBoardToDelete(null);
      setConfirmTitle('');
    },
    onError: (error) => {
      console.error('Failed to delete board:', error);
    },
  });

  const joinBoardMutation = useMutation({
    mutationFn: (boardId: string) => {
      console.log('🔄 Join board mutation starting:', { boardId, hasToken: !!token });
      return joinBoard(boardId, token || undefined);
    },
    onSuccess: (data) => {
      console.log('✅ Join board mutation success:', data);
      queryClient.invalidateQueries({ queryKey: ['boards'] });
      // Show success message based on the response
      if (data?.code === 'already_member') {
        console.info('Already a member of this board');
      } else {
        console.info('Successfully joined board');
      }
    },
    onError: (error: any) => {
      const errorCode = error?.code;
      const errorMessage = error?.message || 'Failed to join board';
      
      console.error('Join board error:', {
        code: errorCode,
        message: errorMessage,
        status: error?.status,
      });

      // Show user-friendly error messages
      if (errorCode === 'board_private') {
        console.error('Board is private and requires an invitation');
      } else if (errorCode === 'board_not_found') {
        console.error('Board not found');
      } else {
        console.error('Join board error: ' + errorMessage);
      }
    },
  });

  const leaveBoardMutation = useMutation({
    mutationFn: (boardId: string) => leaveBoard(boardId, token || undefined),
    onSuccess: (data) => {
      queryClient.invalidateQueries({ queryKey: ['boards'] });
      console.info('Successfully left board');
    },
    onError: (error: any) => {
      const errorCode = error?.code;
      const errorMessage = error?.message || 'Failed to leave board';
      
      console.error('Leave board error:', {
        code: errorCode,
        message: errorMessage,
        status: error?.status,
      });

      // Show user-friendly error messages with actionable advice
      if (errorCode === 'last_owner') {
        console.error('Cannot leave board as the last owner. Please transfer ownership to another member or delete the board.');
      } else if (errorCode === 'not_member') {
        console.error('You are not a member of this board');
      } else if (errorCode === 'board_not_found') {
        console.error('Board not found');
      } else {
        console.error('Leave board error: ' + errorMessage);
      }
    },
  });

  const handleCreateBoard = (board: { title: string; description?: string }) => {
    createBoardMutation.mutate(board);
  };

  const handleDeleteBoard = (boardId: string, boardTitle: string) => {
    setBoardToDelete({ id: boardId, title: boardTitle });
    setIsDeleteModalOpen(true);
  };

  const handleJoinBoard = (boardId: string) => {
    joinBoardMutation.mutate(boardId);
  };

  const handleLeaveBoard = (boardId: string) => {
    leaveBoardMutation.mutate(boardId);
  };

  if (isLoading) {
    return (
      <div className='min-h-screen bg-gray-50 dark:bg-gray-900 p-6'>
        <div className='max-w-7xl mx-auto'>
          <div className='flex justify-between items-center mb-8'>
            <div className='h-8 bg-gray-200 dark:bg-gray-700 rounded w-48 animate-pulse'></div>
            <div className='h-10 bg-gray-200 dark:bg-gray-700 rounded w-32 animate-pulse'></div>
          </div>
          <div className='grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4 gap-6'>
            {[...Array(8)].map((_, i) => (
              <div
                key={i}
                className='bg-white dark:bg-gray-800 rounded-lg border border-gray-200 dark:border-gray-700 p-6 animate-pulse'
              >
                <div className='h-6 bg-gray-200 dark:bg-gray-700 rounded mb-3'></div>
                <div className='h-4 bg-gray-200 dark:bg-gray-700 rounded mb-2'></div>
                <div className='h-4 bg-gray-200 dark:bg-gray-700 rounded w-3/4'></div>
              </div>
            ))}
          </div>
        </div>
      </div>
    );
  }

  if (isError) {
    return (
      <div className='min-h-screen bg-gray-50 dark:bg-gray-900 flex items-center justify-center p-6'>
        <div className='text-center'>
          <div className='text-red-500 text-xl mb-4'>⚠️ Error loading boards</div>
          <p className='text-gray-600 dark:text-gray-400'>
            {error?.message || 'Something went wrong'}
          </p>
          <Button
            onClick={() => {
              window.location.reload();
            }}
            className='mt-4'
          >
            Try Again
          </Button>
        </div>
      </div>
    );
  }

  return (
    <div className='min-h-screen bg-gray-50 dark:bg-gray-900 p-6'>
      <div className='max-w-7xl mx-auto'>
        <div className='flex justify-between items-center mb-8'>
          <div>
            <h1 className='text-3xl font-bold text-gray-900 dark:text-gray-100'>Your Boards</h1>
            <p className='text-gray-600 dark:text-gray-400 mt-1'>
              Organize your projects and tasks efficiently
            </p>
          </div>
          <Button
            onClick={() => {
              setIsCreateModalOpen(true);
            }}
            className='flex items-center space-x-2'
          >
            <Plus className='w-4 h-4' />
            <span>Create Board</span>
          </Button>
        </div>

        {boards && boards.length > 0 ? (
          <div className='grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4 gap-6'>
            {boards.map((board) => (
              <BoardCard
                key={board.id}
                board={board}
                onDelete={handleDeleteBoard}
                onJoin={handleJoinBoard}
                onLeave={handleLeaveBoard}
                currentUserId={currentUserId ?? undefined}
              />
            ))}
          </div>
        ) : (
          <div className='text-center py-12'>
            <div className='text-gray-400 text-6xl mb-4'>📋</div>
            <h3 className='text-xl font-semibold text-gray-900 dark:text-gray-100 mb-2'>
              No boards yet
            </h3>
            <p className='text-gray-600 dark:text-gray-400 mb-6'>
              Create your first board to get started organizing your tasks
            </p>
            <Button
              onClick={() => {
                setIsCreateModalOpen(true);
              }}
              className='flex items-center space-x-2 mx-auto'
            >
              <Plus className='w-4 h-4' />
              <span>Create Your First Board</span>
            </Button>
          </div>
        )}

        <CreateBoardModal
          isOpen={isCreateModalOpen}
          onClose={() => {
            setIsCreateModalOpen(false);
          }}
          onCreateBoard={handleCreateBoard}
        />

        <Dialog
          open={isDeleteModalOpen}
          onOpenChange={(open) => {
            setIsDeleteModalOpen(open);
            if (!open) {
              setBoardToDelete(null);
              setConfirmTitle('');
            }
          }}
        >
          <DialogContent>
            <DialogHeader>
              <DialogTitle>Delete Board</DialogTitle>
              <DialogDescription>
                This action cannot be undone. To confirm deletion, please type the board name:
                <span className='font-semibold'> {boardToDelete?.title}</span>
              </DialogDescription>
            </DialogHeader>
            <div className='space-y-4'>
              <Input
                placeholder='Enter board name to confirm'
                value={confirmTitle}
                onChange={(e) => {
                  setConfirmTitle(e.target.value);
                }}
              />
              <div className='flex space-x-3'>
                <Button
                  variant='destructive'
                  disabled={!boardToDelete || confirmTitle !== boardToDelete.title}
                  onClick={() => boardToDelete && deleteBoardMutation.mutate(boardToDelete.id)}
                  className='flex-1'
                >
                  Delete Board
                </Button>
                <Button
                  variant='outline'
                  onClick={() => {
                    setIsDeleteModalOpen(false);
                    setBoardToDelete(null);
                    setConfirmTitle('');
                  }}
                  className='flex-1'
                >
                  Cancel
                </Button>
              </div>
            </div>
          </DialogContent>
        </Dialog>
      </div>
    </div>
  );
};

export default BoardsPage;
