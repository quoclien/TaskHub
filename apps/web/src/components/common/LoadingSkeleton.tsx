// External dependencies
import { cn } from '@/lib/utils';

// Re-export Skeleton from shadcn/ui
export { Skeleton } from '@/components/ui/skeleton';

// Types
interface SkeletonTextProps {
  lines?: number;
  className?: string;
}

interface SkeletonCardProps {
  className?: string;
}

/**
 * SkeletonText Component
 *
 * Renders multiple skeleton lines to simulate text content.
 * Used for loading states of content with multiple text lines.
 */
export function SkeletonText({ lines = 3, className }: SkeletonTextProps) {
  return (
    <div className={cn('space-y-2', className)}>
      {Array.from({ length: lines }, (_, index) => (
        <div
          key={index}
          className={cn(
            'h-4 bg-muted rounded animate-pulse',
            // Make lines slightly different widths for realism
            index === lines - 1 ? 'w-2/3' : 'w-full'
          )}
        />
      ))}
    </div>
  );
}

/**
 * SkeletonCard Component
 *
 * Renders a skeleton placeholder that matches the BoardCard layout.
 * Used for loading states in board grids.
 */
export function SkeletonCard({ className }: SkeletonCardProps) {
  return (
    <div className={cn('rounded-lg border bg-card p-4 space-y-3', className)}>
      {/* Title skeleton */}
      <div className='space-y-2'>
        <div className='h-4 bg-muted rounded animate-pulse w-3/4' />
        <div className='h-4 bg-muted rounded animate-pulse w-1/2' />
      </div>

      {/* Metadata skeleton */}
      <div className='flex items-center justify-between'>
        <div className='h-6 bg-muted rounded-full animate-pulse w-20' />
        <div className='h-3 bg-muted rounded animate-pulse w-24' />
      </div>
    </div>
  );
}

/**
 * SkeletonList Component
 *
 * Renders a skeleton placeholder for list columns in board view.
 * Used for loading states in kanban board layouts.
 */
export function SkeletonList({ className }: { className?: string }) {
  return (
    <div className={cn('p-4 bg-gray-200 rounded-md shrink-0 w-72 space-y-4', className)}>
      {/* List title skeleton */}
      <div className='h-6 bg-muted rounded animate-pulse w-2/3' />

      {/* Cards skeleton */}
      <div className='space-y-2'>
        {Array.from({ length: 3 }, (_, index) => (
          <div
            key={index}
            className='h-16 bg-white rounded-md shadow animate-pulse'
          />
        ))}
      </div>

      {/* Add card button skeleton */}
      <div className='h-8 bg-muted rounded animate-pulse w-full' />
    </div>
  );
}
