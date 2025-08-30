'use client';

// External dependencies
import { useQuery } from '@tanstack/react-query';
import { useRouter, useSearchParams } from 'next/navigation';
import { useCallback, useEffect } from 'react';

// Internal dependencies
import { Skeleton } from '@/components/common/LoadingSkeleton';
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogHeader,
  DialogTitle,
} from '@/components/ui/dialog';
import { getCardById } from '@/lib/api/board';

// Types
interface CardData {
  id: string;
  title: string;
  description?: string;
  listId: string;
  position: number;
  createdAt: string;
  updatedAt: string;
  metadata?: Record<string, any>;
  dueDate?: string | null;
  isArchived: boolean;
  assignedTo?: string | null;
  comments: any[];
}

/**
 * CardDetailModal Component
 *
 * A modal dialog that displays detailed information about a card.
 * The modal state is controlled by URL query parameters (?cardId=...).
 * This ensures the modal view is shareable and integrated with browser history.
 */
export function CardDetailModal() {
  // Hooks
  const router = useRouter();
  const searchParams = useSearchParams();
  const cardId = searchParams.get('cardId');

  // Data fetching
  const {
    data: card,
    isLoading,
    error,
  } = useQuery<CardData>({
    queryKey: ['card', cardId],
    queryFn: () => getCardById(cardId!),
    enabled: !!cardId,
  });

  // Modal state based on URL
  const isOpen = !!cardId;

  // Event handlers
  const handleClose = useCallback(() => {
    const params = new URLSearchParams(searchParams.toString());
    params.delete('cardId');
    const newUrl = params.toString() ? `?${params.toString()}` : '';
    router.replace(newUrl);
  }, [router, searchParams]);

  // Don't render if no cardId
  if (!cardId) {
    return null;
  }

  return (
    <Dialog
      open={isOpen}
      onOpenChange={(open) => !open && handleClose()}
    >
      <DialogContent className='max-w-2xl max-h-[90vh] overflow-y-auto'>
        <DialogHeader>
          <DialogTitle>
            {isLoading ? (
              <Skeleton className='h-6 w-64' />
            ) : error ? (
              'Error'
            ) : (
              card?.title || 'Card Details'
            )}
          </DialogTitle>
          <DialogDescription>
            {isLoading
              ? 'Loading card details...'
              : error
                ? 'Error loading card details'
                : 'Card details and comments'}
          </DialogDescription>
        </DialogHeader>

        <div className='space-y-6'>
          {isLoading && (
            <div
              data-testid='card-detail-loading'
              className='space-y-4'
            >
              <Skeleton className='h-4 w-full' />
              <Skeleton className='h-4 w-3/4' />
              <Skeleton className='h-4 w-1/2' />
            </div>
          )}

          {error && (
            <div className='text-red-600 text-center py-8'>
              <p>Error loading card details</p>
              <p className='text-sm text-gray-500 mt-2'>Please try again later</p>
            </div>
          )}

          {card && !isLoading && !error && (
            <div className='space-y-6'>
              {/* Card Description */}
              <div>
                <h3 className='text-lg font-semibold mb-2'>Description</h3>
                {card.description ? (
                  <div className='prose max-w-none'>
                    <p className='text-gray-700 whitespace-pre-wrap'>{card.description}</p>
                  </div>
                ) : (
                  <p className='text-gray-500 italic'>No description provided</p>
                )}
              </div>

              {/* Card Metadata */}
              <div className='grid grid-cols-2 gap-4 text-sm'>
                <div>
                  <span className='font-medium text-gray-600'>Created:</span>
                  <p className='mt-1'>{new Date(card.createdAt).toLocaleDateString()}</p>
                </div>
                <div>
                  <span className='font-medium text-gray-600'>Updated:</span>
                  <p className='mt-1'>{new Date(card.updatedAt).toLocaleDateString()}</p>
                </div>
                {card.dueDate && (
                  <div>
                    <span className='font-medium text-gray-600'>Due Date:</span>
                    <p className='mt-1'>{new Date(card.dueDate).toLocaleDateString()}</p>
                  </div>
                )}
              </div>

              {/* Comments section placeholder */}
              <div>
                <h3 className='text-lg font-semibold mb-2'>Comments ({card.comments.length})</h3>
                {card.comments.length > 0 ? (
                  <div className='space-y-2'>
                    {card.comments.map((comment, index) => (
                      <div
                        key={index}
                        className='border rounded p-3'
                      >
                        <p className='text-sm text-gray-700'>{comment.content || comment}</p>
                      </div>
                    ))}
                  </div>
                ) : (
                  <p className='text-gray-500 italic'>No comments yet</p>
                )}
              </div>
            </div>
          )}
        </div>
      </DialogContent>
    </Dialog>
  );
}
