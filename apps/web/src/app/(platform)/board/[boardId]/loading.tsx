import { Skeleton } from '@/components/ui/skeleton';

export default function BoardLoading() {
  return (
    <div className='space-y-4'>
      {/* Breadcrumbs skeleton */}
      <div className='flex items-center space-x-2'>
        <Skeleton className='h-4 w-4' />
        <Skeleton className='h-4 w-16' />
        <Skeleton className='h-4 w-4' />
        <Skeleton className='h-4 w-20' />
      </div>

      {/* Board title skeleton */}
      <div className='flex items-center justify-between'>
        <Skeleton className='h-8 w-48' />
      </div>

      {/* Board content skeleton */}
      <div className='flex space-x-4 overflow-x-auto pb-4'>
        {[...Array(4)].map((_, i) => (
          <div
            key={i}
            className='flex-shrink-0 w-80 space-y-4'
          >
            <div className='bg-gray-50 dark:bg-gray-800 rounded-lg p-4'>
              <Skeleton className='h-6 w-32 mb-4' />
              <div className='space-y-3'>
                {[...Array(3)].map((_, j) => (
                  <div
                    key={j}
                    className='bg-white dark:bg-gray-700 rounded p-3'
                  >
                    <Skeleton className='h-4 w-full mb-2' />
                    <Skeleton className='h-3 w-3/4' />
                  </div>
                ))}
              </div>
            </div>
          </div>
        ))}
      </div>
    </div>
  );
}
