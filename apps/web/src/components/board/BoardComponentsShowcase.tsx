'use client';

// External dependencies
import { arrayMove } from '@dnd-kit/sortable';
import { useState } from 'react';

// Internal dependencies

import { SkeletonCard } from '@/components/common/LoadingSkeleton';
import { Button } from '@/components/ui/button';

import {
  BoardCard,
  BoardView,
  CardComponent,
  ListComponent,
  CreateBoardModal,
  CreateCardModal,
} from './index';

// Types
interface CardProps {
  id: string;
  title: string;
  position?: number;
  labels?: {
    id: string;
    name: string;
    color: string;
  }[];
  dueDate?: string;
  members?: {
    id: string;
    name: string;
    avatar?: string;
    initials?: string;
  }[];
}

interface ListProps {
  id: string;
  title: string;
  position: number;
  cards: CardProps[];
  boardId: string;
}

/**
 * BoardComponentsShowcase
 *
 * A demonstration component showcasing all the board UI components.
 * Used for testing and development purposes.
 */
export function BoardComponentsShowcase() {
  // State
  const [isLoading, setIsLoading] = useState(false);

  // Mock Data
  const mockCards = [
    {
      id: 'card-1',
      title: 'Design new user dashboard',
      position: 0,
      labels: [
        { id: 'card-label-1', name: 'Design', color: '#3b82f6' },
        { id: 'card-label-2', name: 'High Priority', color: '#ef4444' },
      ],
      dueDate: '2024-02-01T12:00:00Z',
      members: [
        { id: 'card-member-1', name: 'John Doe', initials: 'JD' },
        { id: 'card-member-2', name: 'Jane Smith', initials: 'JS' },
      ],
    },
    {
      id: 'card-2',
      title: 'Fix mobile responsiveness issues',
      position: 1,
      labels: [{ id: 'card-label-3', name: 'Bug', color: '#f59e0b' }],
      members: [{ id: 'card-member-3', name: 'Mike Johnson', initials: 'MJ' }],
    },
  ];

  const initialLists: ListProps[] = [
    {
      id: 'list-1',
      title: 'To Do',
      position: 0,
      cards: mockCards,
      boardId: 'showcase-board',
    },
    {
      id: 'list-2',
      title: 'In Progress',
      position: 1,
      cards: [mockCards[0]],
      boardId: 'showcase-board',
    },
    {
      id: 'list-3',
      title: 'Done',
      position: 2,
      cards: [],
      boardId: 'showcase-board',
    },
  ];

  const [lists, setLists] = useState<ListProps[]>(initialLists);

  const mockBoards = [
    {
      id: 'board-1',
      title: 'Product Development',
      memberCount: 5,
      lastActivity: '2024-01-20T10:30:00Z',
    },
    {
      id: 'board-2',
      title: 'Marketing Campaign',
      memberCount: 3,
      lastActivity: '2024-01-19T14:15:00Z',
    },
    {
      id: 'board-3',
      title: 'Bug Fixes',
      memberCount: 2,
      lastActivity: '2024-01-18T09:45:00Z',
    },
  ];

  // Event Handlers
  const handleToggleLoading = () => {
    setIsLoading(!isLoading);
  };

  const handleMoveCard = (cardId: string, currentListId: string, newListId: string, newPosition: number) => {
    setLists((prevLists) => {
      // Find the source list and card
      const sourceListIndex = prevLists.findIndex((list) => list.id === currentListId);
      if (sourceListIndex === -1) return prevLists;

      const sourceCardIndex = prevLists[sourceListIndex].cards.findIndex((card) => card.id === cardId);
      if (sourceCardIndex === -1) return prevLists;

      const cardToMove = prevLists[sourceListIndex].cards[sourceCardIndex];
      if (!cardToMove) return prevLists;

      // Find the target list
      const targetListIndex = prevLists.findIndex((list) => list.id === newListId);
      if (targetListIndex === -1) return prevLists;

      const newLists = [...prevLists];

      // Remove card from source list
      newLists[sourceListIndex] = {
        ...newLists[sourceListIndex],
        cards: newLists[sourceListIndex].cards.filter((_, index) => index !== sourceCardIndex),
      };

      // Add card to target list
      const targetList = newLists[targetListIndex];
      const newCards = [...targetList.cards];
      newCards.splice(newPosition, 0, cardToMove);

      newLists[targetListIndex] = {
        ...targetList,
        cards: newCards,
      };

      return newLists;
    });
  };

  // Main Component
  return (
    <div className='p-6 space-y-8'>
      <div className='text-center'>
        <h1 className='text-3xl font-bold mb-4'>Board Components Showcase</h1>
        <p className='text-muted-foreground mb-4'>
          Demonstration of all TaskHub board UI components
        </p>
        <Button onClick={handleToggleLoading}>
          {isLoading ? 'Hide Loading States' : 'Show Loading States'}
        </Button>
      </div>

      {/* Board Cards Section */}
      <section>
        <h2 className='text-2xl font-semibold mb-4'>Board Cards</h2>
        <div className='grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4'>
          {isLoading ? (
            <>
              <SkeletonCard key='skeleton-1' />
              <SkeletonCard key='skeleton-2' />
              <SkeletonCard key='skeleton-3' />
            </>
          ) : (
            mockBoards.map((board) => (
              <BoardCard
                key={board.id}
                id={board.id}
                title={board.title}
                memberCount={board.memberCount}
                lastActivity={board.lastActivity}
              />
            ))
          )}
        </div>
      </section>

      {/* Create Board Modal Section */}
      <section>
        <h2 className='text-2xl font-semibold mb-4'>Create Board Modal</h2>
        <CreateBoardModal>
          <Button>Open Create Board Modal</Button>
        </CreateBoardModal>
      </section>

      {/* Board View with Lists Section */}
      <section>
        <h2 className='text-2xl font-semibold mb-4'>Board View (Kanban)</h2>
        <div className='border rounded-lg p-4 bg-gray-50'>
          <BoardView
            isLoading={isLoading}
            onMoveCard={handleMoveCard}
          >
            {lists.map((list) => (
              <ListComponent
                key={list.id}
                id={list.id}
                title={list.title}
                position={list.position}
                cards={list.cards}
                boardId={list.boardId}
                onRename={(listId) => {
                  console.log('Rename list:', listId);
                }}
                onDelete={(listId) => {
                  console.log('Delete list:', listId);
                }}
                onAddCard={(listId) => {
                  console.log('Add card to list:', listId);
                }}
              />
            ))}
          </BoardView>
        </div>
      </section>

      {/* Individual Card Component Section */}
      <section>
        <h2 className='text-2xl font-semibold mb-4'>Individual Card</h2>
        <div className='max-w-sm'>
          <CardComponent
            id={mockCards[0].id}
            title={mockCards[0].title}
            labels={mockCards[0].labels}
            dueDate={mockCards[0].dueDate}
            members={mockCards[0].members}
            boardId='showcase-board'
            listId='1'
            index={0}
          />
        </div>
      </section>

      {/* Create Card Modal Section */}
      <section>
        <h2 className='text-2xl font-semibold mb-4'>Create Card Modal</h2>
        <CreateCardModal
          listId='1'
          boardId='showcase-board'
          cardCount={2}
        >
          <Button>Open Create Card Modal</Button>
        </CreateCardModal>
      </section>
    </div>
  );
}
