'use client';

// External dependencies
import { useSortable } from '@dnd-kit/sortable';
import { CSS } from '@dnd-kit/utilities';
import { formatRelativeTime, isPast } from '@taskhub/shared';
import { useRouter, useSearchParams } from 'next/navigation';

// Internal dependencies
import { cn } from '@/lib/utils';

// Types
interface LabelDTO {
  id: string;
  name: string;
  color: string;
}

interface MemberDTO {
  id: string;
  name: string;
  avatar?: string;
  initials?: string;
}

interface CardComponentProps {
  id: string;
  title: string;
  labels?: LabelDTO[];
  dueDate?: string;
  members?: MemberDTO[];
  boardId: string;
  listId: string;
  index: number;
  className?: string;
}

/**
 * CardComponent
 *
 * A compact card for displaying task information within kanban lists.
 * Features labels, due dates, member avatars, responsive design, and drag-and-drop functionality.
 */
export function CardComponent({
  id,
  title,
  labels = [],
  dueDate,
  members = [],
  boardId,
  listId,
  index,
  className,
}: CardComponentProps) {
  // Hooks
  const router = useRouter();
  const searchParams = useSearchParams();

  // DnD Hooks
  const { attributes, listeners, setNodeRef, transform, transition, isDragging } = useSortable({
    id,
    data: {
      type: 'card',
      listId,
      index,
    },
  });

  const style = {
    transform: CSS.Transform.toString(transform),
    transition,
  };

  // Event Handlers
  const handleCardClick = (e: React.MouseEvent) => {
    e.preventDefault();
    e.stopPropagation();

    // Add cardId to URL params to open modal
    const params = new URLSearchParams(searchParams.toString());
    params.set('cardId', id);
    router.replace(`?${params.toString()}`);
  };

  // Render Helpers
  const renderLabels = () => {
    if (labels.length === 0) return null;

    return (
      <div className='flex flex-wrap gap-1 mb-2'>
        {labels.map((label) => (
          <span
            key={label.id}
            className='inline-flex items-center rounded px-2 py-1 text-xs font-medium'
            style={{
              backgroundColor: label.color + '20', // Add transparency
              color: label.color,
            }}
          >
            {label.name}
          </span>
        ))}
      </div>
    );
  };

  const renderDueDate = () => {
    if (!dueDate) return null;

    const dueDateObj = new Date(dueDate);
    const isOverdue = isPast(dueDateObj);

    return (
      <div
        className={cn(
          'inline-flex items-center rounded px-2 py-1 text-xs font-medium',
          isOverdue ? 'bg-destructive/10 text-destructive' : 'bg-muted text-muted-foreground'
        )}
      >
        {formatRelativeTime(dueDate)}
      </div>
    );
  };

  const renderFooter = () => {
    if (!dueDate && members.length === 0) return null;

    return (
      <div className='flex items-center justify-between mt-2'>
        {renderDueDate()}
        {members.length > 0 && (
          <div className='flex -space-x-2'>
            {members.map((member) => (
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
    );
  };

  // Main Component
  return (
    <div
      key={id}
      ref={setNodeRef}
      style={style}
      {...attributes}
      {...listeners}
      className={cn(
        'block p-3 bg-white rounded-md shadow-sm border transition-all duration-200',
        'hover:shadow-md hover:-translate-y-0.5',
        'focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring',
        'cursor-grab active:cursor-grabbing',
        isDragging && 'opacity-50',
        className
      )}
      onClick={handleCardClick}
    >
      <div>
        {/* Labels */}
        {renderLabels()}

        {/* Title */}
        <h4 className='font-medium text-sm text-card-foreground line-clamp-3 mb-1'>{title}</h4>

        {/* Footer with due date and members */}
        {renderFooter()}
      </div>
    </div>
  );
}
