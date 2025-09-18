import { Skeleton } from '@/components/ui/skeleton';

export default function BoardsLoading() {
  return (
    <div className='min-h-screen bg-gray-50 dark:bg-gray-900 p-6'>
      <div className='max-w-7xl mx-auto'>
        <div className='flex justify-between items-center mb-8'>
          <Skeleton className='h-8 w-48' />
          <Skeleton className='h-10 w-32' />
        </div>
        <div className='grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4 gap-6'>
          {[...Array(8)].map((_, i) => (
            <div
              key={i}
              className='bg-white dark:bg-gray-800 rounded-lg border border-gray-200 dark:border-gray-700 p-6'
            >
              <Skeleton className='h-6 w-3/4 mb-3' />
              <Skeleton className='h-4 w-full mb-2' />
              <Skeleton className='h-4 w-2/3 mb-4' />
              <div className='flex items-center space-x-4'>
                <Skeleton className='h-3 w-16' />
                <Skeleton className='h-3 w-20' />
              </div>
            </div>
          ))}
        </div>
      </div>
    </div>
  );
}
