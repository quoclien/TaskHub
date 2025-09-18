// External dependencies
import { formatRelativeTime } from '@taskhub/shared';
import Link from 'next/link';

// Internal dependencies
import { cn } from '@/lib/utils';

// Types
interface BoardCardProps {
  id: string;
  title: string;
  memberCount: number;
  lastActivity: string | Date;
  className?: string;
}

/**
 * BoardCard Component
 *
 * A clickable card component for displaying board information in a grid layout.
 * Features hover effects, member count badge, and relative time display.
 */
export function BoardCard({ id, title, memberCount, lastActivity, className }: BoardCardProps) {
  // Render Helpers
  const renderMemberBadge = () => (
    <div className='inline-flex items-center rounded-full bg-primary/10 px-2 py-1 text-xs font-medium text-primary'>
      {memberCount} {memberCount === 1 ? 'member' : 'members'}
    </div>
  );

  const renderLastActivity = () => (
    <span className='text-xs text-muted-foreground'>
      Updated {formatRelativeTime(lastActivity)}
    </span>
  );

  // Main Component
  return (
    <Link
      href={`/board/${id}`}
      className={cn(
        'group block rounded-lg border bg-card p-4 transition-all duration-200',
        'hover:shadow-md hover:shadow-primary/5 hover:-translate-y-0.5',
        'focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring',
        className
      )}
    >
      <div className='space-y-3'>
        {/* Title */}
        <h3 className='font-semibold text-card-foreground line-clamp-2 group-hover:text-primary transition-colors'>
          {title}
        </h3>

        {/* Metadata */}
        <div className='flex items-center justify-between'>
          {renderMemberBadge()}
          {renderLastActivity()}
        </div>
      </div>
    </Link>
  );
}
