'use client';

// External dependencies
import {
  DndContext,
  closestCenter,
  type DragEndEvent,
  type DragStartEvent,
  type DragOverEvent,
  PointerSensor,
  useSensor,
  useSensors,
  KeyboardSensor,
} from '@dnd-kit/core';
import {
  arrayMove,
  SortableContext,
  sortableKeyboardCoordinates,
  horizontalListSortingStrategy,
} from '@dnd-kit/sortable';
import React, { forwardRef, useState } from 'react';

// Internal dependencies
import { SkeletonList } from '@/components/common/LoadingSkeleton';
import { cn } from '@/lib/utils';

// Types
interface CardMeta {
  listId: string;
  index: number;
  type: string;
}

interface ListMeta {
  type: string;
  cards: any[];
}

interface BoardViewProps {
  children: React.ReactNode;
  isLoading?: boolean;
  className?: string;
  onMoveCard?: (cardId: string, newListId: string, index: number) => void;
  onMoveList?: (oldIndex: number, newIndex: number) => void;
}

/**
 * BoardView Component
 *
 * Provides a horizontal scrolling container for kanban lists with drag-and-drop functionality.
 * Includes loading skeleton states and smooth scroll behavior.
 */
export const BoardView = forwardRef<HTMLDivElement, BoardViewProps>(
  ({ children, isLoading = false, className, onMoveCard, onMoveList }, ref) => {
    // State
    const [activeId, setActiveId] = useState<string | null>(null);
    const [activeData, setActiveData] = useState<any | null>(null);

    // Sensors
    const sensors = useSensors(
      useSensor(PointerSensor, {
        activationConstraint: { distance: 4 },
      }),
      useSensor(KeyboardSensor, {
        coordinateGetter: sortableKeyboardCoordinates,
      })
    );

    // Event Handlers
    const handleDragStart = (event: DragStartEvent) => {
      const { active } = event;
      setActiveId(active.id as string);
      setActiveData(active.data.current);
    };

    const handleDragOver = (event: DragOverEvent) => {
      const { active, over } = event;
      if (!over) return;

      const activeData = active.data.current as CardMeta | ListMeta;
      const overData = over.data.current as CardMeta | ListMeta;

      // If no data or same item, skip
      if (!activeData || !overData || active.id === over.id) return;

      // Handle card moving between lists
      if (activeData.type === 'card' && overData.type === 'list') {
        const activeCardData = activeData as CardMeta;
        onMoveCard?.(active.id as string, over.id as string, 0);
      }
    };

    const handleDragEnd = (event: DragEndEvent) => {
      const { active, over } = event;

      if (!over) {
        setActiveId(null);
        setActiveData(null);
        return;
      }

      const activeData = active.data.current as CardMeta | ListMeta;
      const overData = over.data.current as CardMeta | ListMeta;

      // If no data or same item, skip
      if (!activeData || !overData || active.id === over.id) {
        setActiveId(null);
        setActiveData(null);
        return;
      }

      // Handle list reordering
      if (activeData.type === 'list' && overData.type === 'list') {
        const lists = React.Children.toArray(children);
        const oldIndex = lists.findIndex((child: any) => child.props.id === active.id);
        const newIndex = lists.findIndex((child: any) => child.props.id === over.id);

        if (oldIndex !== -1 && newIndex !== -1) {
          onMoveList?.(oldIndex, newIndex);
        }
      }

      // Handle card moving between lists
      if (activeData.type === 'card') {
        const activeCardData = activeData as CardMeta;
        const overCardData = overData as CardMeta;

        if (overData.type === 'list') {
          // Drop on list
          onMoveCard?.(active.id as string, over.id as string, 0);
        } else if (overData.type === 'card') {
          // Drop on another card
          onMoveCard?.(active.id as string, overCardData.listId, overCardData.index);
        }
      }

      setActiveId(null);
      setActiveData(null);
    };

    // Render Helpers
    const renderLoadingState = () => (
      <>
        {Array.from({ length: 3 }, (_, index) => (
          <SkeletonList key={`skeleton-list-${index}`} />
        ))}
      </>
    );

    const renderContent = () => {
      if (isLoading) {
        return renderLoadingState();
      }

      // Get list IDs for SortableContext
      const listIds = React.Children.toArray(children).map((child: any) => child.props.id);

      return (
        <SortableContext
          items={listIds}
          strategy={horizontalListSortingStrategy}
        >
          {children}
        </SortableContext>
      );
    };

    // Main Component
    return (
      <DndContext
        sensors={sensors}
        collisionDetection={closestCenter}
        onDragStart={handleDragStart}
        onDragOver={handleDragOver}
        onDragEnd={handleDragEnd}
      >
        <div
          ref={ref}
          className={cn(
            'flex gap-x-4 overflow-x-auto pb-4',
            'scroll-smooth scrollbar-thin scrollbar-thumb-primary/20 scrollbar-track-transparent',
            'snap-x snap-mandatory',
            className
          )}
          style={{
            scrollSnapType: 'x mandatory',
          }}
        >
          {renderContent()}
        </div>
      </DndContext>
    );
  }
);
