'use client';

import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import type { Board } from '@taskhub/shared';
import { Plus, Trash2, Calendar, Users } from 'lucide-react';
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
import { fetchBoards, createBoard, deleteBoard } from '@/lib/api/board';

const BoardCard = ({
  board,
  onDelete,
}: {
  board: Board;
  onDelete: (id: string, boardTitle: string) => void;
}) => {
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
          <div className='flex items-center text-xs text-gray-500 dark:text-gray-500 space-x-4'>
            <div className='flex items-center space-x-1'>
              <Calendar className='w-3 h-3' />
              <span>{new Date(board.created_at).toLocaleDateString()}</span>
            </div>
            <div className='flex items-center space-x-1'>
              <Users className='w-3 h-3' />
              <span>3 members</span>
            </div>
          </div>
        </div>
      </Link>

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
        className='absolute top-3 right-3 opacity-0 group-hover:opacity-100 transition-opacity p-1 rounded-md hover:bg-red-50 dark:hover:bg-red-900/20 text-red-600 dark:text-red-400 focus:opacity-100 focus:outline-none focus:ring-2 focus:ring-red-500 focus:ring-offset-2'
        title='Delete board'
        tabIndex={0}
      >
        <Trash2 className='w-4 h-4' />
      </button>
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
  const { token } = useAuthStore();
  const queryClient = useQueryClient();
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
    queryFn: () => fetchBoards(token),
    enabled: true, // Always enable the query since we have mock data
  });

  const createBoardMutation = useMutation({
    mutationFn: (board: { title: string; description?: string }) => createBoard(board, token),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['boards'] });
    },
    onError: (error) => {
      console.error('Failed to create board:', error);
    },
  });

  const deleteBoardMutation = useMutation({
    mutationFn: (boardId: string) => deleteBoard(boardId, token),
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

  const handleCreateBoard = (board: { title: string; description?: string }) => {
    createBoardMutation.mutate(board);
  };

  const handleDeleteBoard = (boardId: string, boardTitle: string) => {
    setBoardToDelete({ id: boardId, title: boardTitle });
    setIsDeleteModalOpen(true);
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
