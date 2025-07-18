// External dependencies
import { forwardRef } from 'react';

// Internal dependencies
import { cn } from '@/lib/utils';
import { SkeletonList } from '@/components/common/LoadingSkeleton';

// Types
interface BoardViewProps {
  children?: React.ReactNode;
  isLoading?: boolean;
  className?: string;
}

/**
 * BoardView Component
 * 
 * Provides a horizontal scrolling container for kanban lists.
 * Includes loading skeleton states and smooth scroll behavior.
 */
export const BoardView = forwardRef<HTMLDivElement, BoardViewProps>(
  ({ children, isLoading = false, className }, ref) => {
    
    // Render Helpers
    const renderLoadingState = () => (
      <>
        {Array.from({ length: 3 }, (_, index) => (
          <SkeletonList key={index} />
        ))}
      </>
    );

    const renderContent = () => {
      if (isLoading) {
        return renderLoadingState();
      }
      
      return children;
    };

    // Main Component
    return (
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
    );
  }
);

BoardView.displayName = 'BoardView'; 