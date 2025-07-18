// External dependencies
import Link from 'next/link';
import { formatRelativeTime, isPast } from '@taskhub/shared';

// Internal dependencies
import { cn } from '@/lib/utils';

// Types
interface LabelDTO {
  id: string;
  name: string;
  color: string;
}

interface UserDTO {
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
  members?: UserDTO[];
  boardId: string;
  className?: string;
}

/**
 * CardComponent
 * 
 * A compact card for displaying task information within kanban lists.
 * Features labels, due dates, member avatars, and responsive design.
 */
export function CardComponent({
  id,
  title,
  labels = [],
  dueDate,
  members = [],
  boardId,
  className,
}: CardComponentProps) {
  
  // Event Handlers
  const handleCardClick = (e: React.MouseEvent) => {
    // Allow event bubbling for link navigation
    e.stopPropagation();
  };

  // Render Helpers
  const renderLabels = () => {
    if (labels.length === 0) return null;

    return (
      <div className="flex flex-wrap gap-1 mb-2">
        {labels.map((label) => (
          <span
            key={label.id}
            className="inline-flex items-center rounded px-2 py-1 text-xs font-medium"
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
      <div className={cn(
        'inline-flex items-center rounded px-2 py-1 text-xs font-medium',
        isOverdue 
          ? 'bg-destructive/10 text-destructive' 
          : 'bg-muted text-muted-foreground'
      )}>
        {formatRelativeTime(dueDate)}
      </div>
    );
  };

  const renderMemberAvatars = () => {
    if (members.length === 0) return null;

    // Show max 3 avatars, then +N indicator
    const visibleMembers = members.slice(0, 3);
    const remainingCount = members.length - 3;

    return (
      <div className="flex items-center -space-x-1">
        {visibleMembers.map((member) => (
          <div
            key={member.id}
            className="inline-flex h-6 w-6 items-center justify-center rounded-full bg-primary text-xs font-medium text-primary-foreground ring-2 ring-background"
            title={member.name}
          >
            {member.avatar ? (
              <img
                src={member.avatar}
                alt={member.name}
                className="h-full w-full rounded-full object-cover"
              />
            ) : (
              <span>{member.initials || member.name.charAt(0).toUpperCase()}</span>
            )}
          </div>
        ))}
        {remainingCount > 0 && (
          <div className="inline-flex h-6 w-6 items-center justify-center rounded-full bg-muted text-xs font-medium text-muted-foreground ring-2 ring-background">
            +{remainingCount}
          </div>
        )}
      </div>
    );
  };

  const renderFooter = () => {
    const hasDueDate = !!dueDate;
    const hasMembers = members.length > 0;

    if (!hasDueDate && !hasMembers) return null;

    return (
      <div className="flex items-center justify-between mt-3">
        {hasDueDate ? renderDueDate() : <div />}
        {hasMembers && renderMemberAvatars()}
      </div>
    );
  };

  // Main Component
  return (
    <Link
      href={`/board/${boardId}/card/${id}`}
      onClick={handleCardClick}
      className={cn(
        "block p-3 bg-white rounded-md shadow-sm border transition-all duration-200",
        "hover:shadow-md hover:-translate-y-0.5",
        "focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring",
        className
      )}
    >
      <div>
        {/* Labels */}
        {renderLabels()}
        
        {/* Title */}
        <h4 className="font-medium text-sm text-card-foreground line-clamp-3 mb-1">
          {title}
        </h4>
        
        {/* Footer with due date and members */}
        {renderFooter()}
      </div>
    </Link>
  );
} 