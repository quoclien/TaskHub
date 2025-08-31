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
  DragOverlay,
  type DragStartEvent as DragStartEventType,
} from '@dnd-kit/core';
import {
  arrayMove,
  SortableContext,
  sortableKeyboardCoordinates,
  horizontalListSortingStrategy,
} from '@dnd-kit/sortable';
import { CSS } from '@dnd-kit/utilities';
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
  onMoveCard?: (cardId: string, currentListId: string, newListId: string, newPosition: number) => void;
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
    const [activeCard, setActiveCard] = useState<any | null>(null);

    // Sensors
    // Disable DnD activation when any dialog is open
    const isDialogOpen = typeof window !== 'undefined' && document.documentElement.dataset.dialogOpen === 'true';

    const sensors = useSensors(
      useSensor(PointerSensor, {
        activationConstraint: { distance: 4 },
        onActivation: (ev: any) => {
          if (isDialogOpen && typeof ev?.preventDefault === 'function') {
            ev.preventDefault();
          }
        },
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

      // Find the active card for drag overlay
      if (active.data.current?.type === 'card') {
        const activeCardData = active.data.current as CardMeta;
        const lists = React.Children.toArray(children);
        const sourceList = lists.find((list: any) => list.props.id === activeCardData.listId);
        if (sourceList) {
          const card = (sourceList as any).props.cards.find((c: any) => c.id === active.id);
          setActiveCard(card);
        }
      }
    };

    const handleDragOver = (event: DragOverEvent) => {
      const { active, over } = event;
      if (!over) return;

      const activeData = active.data.current as CardMeta | ListMeta;
      const overData = over.data.current as CardMeta | ListMeta;

      // If no data or same item, skip
      if (!activeData || !overData || active.id === over.id) return;

      // Handle cross-list moves in dragOver to enable smooth animations
      if (activeData.type === 'card' && overData.type === 'card') {
        const activeCardData = activeData as CardMeta;
        const overCardData = overData as CardMeta;

        // If it's a cross-list move, trigger the move immediately for animation
        if (activeCardData.listId !== overCardData.listId) {
          const activeCardId = active.id as string;
          const currentListId = activeCardData.listId;
          const targetListId = overCardData.listId;
          const newPosition = overCardData.index;

          // Trigger the move for immediate visual feedback
          onMoveCard?.(activeCardId, currentListId, targetListId, newPosition);
        }
      }

      // Handle dropping on empty list
      if (activeData.type === 'card' && overData.type === 'list') {
        const activeCardData = activeData as CardMeta;
        const targetListId = over.id as string;

        // If it's a cross-list move to empty list, trigger the move immediately
        if (activeCardData.listId !== targetListId) {
          const activeCardId = active.id as string;
          const currentListId = activeCardData.listId;
          const newPosition = 0; // Insert at the beginning of empty list

          // Trigger the move for immediate visual feedback
          onMoveCard?.(activeCardId, currentListId, targetListId, newPosition);
        }
      }
    };

    const handleDragEnd = (event: DragEndEvent) => {
      const { active, over } = event;

      const activeData = active.data.current as CardMeta | ListMeta;

      // Handle list reordering (only if no cross-list card move was handled in dragOver)
      if (activeData?.type === 'list' && over) {
        const overData = over.data.current as CardMeta | ListMeta;
        if (overData?.type === 'list' && active.id !== over.id) {
          const lists = React.Children.toArray(children);
          const oldIndex = lists.findIndex((child: any) => child.props.id === active.id);
          const newIndex = lists.findIndex((child: any) => child.props.id === over.id);

          if (oldIndex !== -1 && newIndex !== -1) {
            onMoveList?.(oldIndex, newIndex);
          }
        }
      }

      // Handle same-list card reordering (not handled in dragOver)
      if (activeData?.type === 'card' && over) {
        const overData = over.data.current as CardMeta | ListMeta;
        if (overData?.type === 'card') {
          const activeCardData = activeData as CardMeta;
          const overCardData = overData as CardMeta;

          // Only handle same-list reordering here (cross-list handled in dragOver)
          if (activeCardData.listId === overCardData.listId && activeCardData.index !== overCardData.index) {
            const activeCardId = active.id as string;
            const currentListId = activeCardData.listId;
            const targetListId = overCardData.listId;
            const newPosition = activeCardData.index < overCardData.index
              ? overCardData.index  // Moving down
              : overCardData.index; // Moving up
            onMoveCard?.(activeCardId, currentListId, targetListId, newPosition);
          }
        }
      }

      // Clean up state
      setActiveId(null);
      setActiveData(null);
      setActiveCard(null);
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

    // Render Helpers
    const renderDragOverlay = () => {
      if (!activeCard) return null;

      return (
        <div className="rotate-3 opacity-90">
          <div className="p-3 bg-white rounded-md shadow-lg border border-gray-200">
            <div>
              {/* Labels */}
              {activeCard.labels && activeCard.labels.length > 0 && (
                <div className='flex flex-wrap gap-1 mb-2'>
                  {activeCard.labels.map((label: any) => (
                    <span
                      key={label.id}
                      className='inline-flex items-center rounded px-2 py-1 text-xs font-medium'
                      style={{
                        backgroundColor: label.color + '20',
                        color: label.color,
                      }}
                    >
                      {label.name}
                    </span>
                  ))}
                </div>
              )}

              {/* Title */}
              <h4 className='font-medium text-sm text-gray-900 line-clamp-3 mb-1'>
                {activeCard.title}
              </h4>

              {/* Due date and members */}
              {(activeCard.dueDate || (activeCard.members && activeCard.members.length > 0)) && (
                <div className='flex items-center justify-between mt-2'>
                  {activeCard.dueDate && (
                    <div className='inline-flex items-center rounded px-2 py-1 text-xs font-medium bg-gray-100 text-gray-600'>
                      Due date
                    </div>
                  )}
                  {activeCard.members && activeCard.members.length > 0 && (
                    <div className='flex -space-x-2'>
                      {activeCard.members.slice(0, 2).map((member: any) => (
                        <div
                          key={member.id}
                          className='h-6 w-6 rounded-full bg-primary/10 flex items-center justify-center text-xs font-medium ring-2 ring-white'
                          title={member.name}
                        >
                          {member.initials}
                        </div>
                      ))}
                    </div>
                  )}
                </div>
              )}
            </div>
          </div>
        </div>
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

        <DragOverlay>
          {activeId && activeCard ? renderDragOverlay() : null}
        </DragOverlay>
      </DndContext>
    );
  }
);
