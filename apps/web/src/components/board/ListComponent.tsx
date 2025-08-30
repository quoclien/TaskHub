'use client';

// External dependencies
import { useSortable } from '@dnd-kit/sortable';
import { SortableContext, verticalListSortingStrategy } from '@dnd-kit/sortable';
import { CSS } from '@dnd-kit/utilities';
import { MoreHorizontal, Plus } from 'lucide-react';
import { useState } from 'react';

// Internal dependencies

import { Button } from '@/components/ui/button';
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuTrigger,
} from '@/components/ui/dropdown-menu';
import { cn } from '@/lib/utils';

import { CardComponent } from './CardComponent';

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

interface ListComponentProps {
  id: string;
  title: string;
  position?: number;
  cards: CardProps[];
  boardId: string;
  onRename?: (listId: string) => void;
  onDelete?: (listId: string) => void;
  onAddCard?: (listId: string) => void;
  className?: string;
}

/**
 * ListComponent
 *
 * A kanban list container with cards, header actions, and add card functionality.
 * Supports drag and drop operations and keyboard accessibility.
 */
export function ListComponent({
  id,
  title,
  position,
  cards,
  boardId,
  onRename,
  onDelete,
  onAddCard,
  className,
}: ListComponentProps) {
  // State
  const [isMenuOpen, setIsMenuOpen] = useState(false);

  // DnD Hooks
  const { attributes, listeners, setNodeRef, transform, transition, isDragging } = useSortable({
    id,
    data: {
      type: 'list',
      cards,
    },
  });

  const style = {
    transform: CSS.Transform.toString(transform),
    transition,
  };

  // Event Handlers
  const handleRename = () => {
    onRename?.(id);
    setIsMenuOpen(false);
  };

  const handleDelete = () => {
    onDelete?.(id);
    setIsMenuOpen(false);
  };

  const handleAddCard = () => {
    onAddCard?.(id);
  };

  // Render Helpers
  const renderHeader = () => (
    <div className='flex items-center justify-between mb-3'>
      <h3 className='font-semibold text-sm text-gray-700 truncate flex-1'>{title}</h3>

      <DropdownMenu
        open={isMenuOpen}
        onOpenChange={setIsMenuOpen}
      >
        <DropdownMenuTrigger asChild>
          <Button
            variant='ghost'
            size='sm'
            className='h-8 w-8 p-0 text-gray-500 hover:text-gray-700'
            aria-label={`Options for ${title} list`}
          >
            <MoreHorizontal className='h-4 w-4' />
          </Button>
        </DropdownMenuTrigger>
        <DropdownMenuContent
          align='end'
          className='w-40'
        >
          <DropdownMenuItem onClick={handleRename}>Rename list</DropdownMenuItem>
          <DropdownMenuItem
            onClick={handleDelete}
            className='text-destructive focus:text-destructive'
          >
            Delete list
          </DropdownMenuItem>
        </DropdownMenuContent>
      </DropdownMenu>
    </div>
  );

  const renderCards = () => {
    const cardIds = cards.map((card) => card.id);

    return (
      <div
        className={cn(
          'space-y-2 mb-3 max-h-[calc(100vh-200px)] overflow-y-auto',
          'min-h-[100px]', // Ensure there's always a drop target
          isDragging && 'opacity-50'
        )}
      >
        <SortableContext
          items={cardIds}
          strategy={verticalListSortingStrategy}
        >
          {cards
            .sort((a, b) => (a.position || 0) - (b.position || 0))
            .map((card, index) => (
              <CardComponent
                key={card.id}
                {...card}
                boardId={boardId}
                listId={id}
                index={index}
              />
            ))}
        </SortableContext>
      </div>
    );
  };

  const renderAddCardButton = () => (
    <Button
      variant='ghost'
      onClick={handleAddCard}
      className='w-full justify-start text-gray-600 hover:text-gray-800 hover:bg-gray-100/50'
      aria-label={`Add card to ${title} list`}
    >
      <Plus className='h-4 w-4 mr-2' />
      Add a card
    </Button>
  );

  // Main Component
  return (
    <div
      ref={setNodeRef}
      style={style}
      {...attributes}
      {...listeners}
      className={cn(
        'p-3 bg-gray-100 rounded-md shrink-0 w-72',
        'snap-start', // For scroll snap
        isDragging && 'opacity-50',
        className
      )}
      role='region'
      aria-label={`${title} list`}
    >
      {renderHeader()}
      {renderCards()}
      {renderAddCardButton()}
    </div>
  );
}
