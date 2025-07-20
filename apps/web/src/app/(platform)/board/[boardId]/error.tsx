'use client';

import { AlertTriangle, RefreshCw } from 'lucide-react';
import { useRouter } from 'next/navigation';

import { Button } from '@/components/ui/button';

interface ErrorProps {
  error: Error & { digest?: string };
  reset: () => void;
}

export default function BoardError({ error, reset }: ErrorProps) {
  const router = useRouter();

  // Event Handlers
  const handleRetry = () => {
    reset();
  };

  const handleGoBack = () => {
    router.back();
  };

  const handleGoHome = () => {
    router.push('/');
  };

  // Main JSX Return
  return (
    <div className='min-h-[400px] flex items-center justify-center'>
      <div className='text-center space-y-4 max-w-md'>
        <div className='flex justify-center'>
          <AlertTriangle className='h-12 w-12 text-red-500' />
        </div>

        <div className='space-y-2'>
          <h2 className='text-xl font-semibold text-gray-900 dark:text-gray-100'>
            Something went wrong
          </h2>
          <p className='text-gray-600 dark:text-gray-400'>
            {error.message || 'Failed to load the board. Please try again.'}
          </p>
        </div>

        <div className='flex flex-col sm:flex-row gap-3 justify-center'>
          <Button
            onClick={handleRetry}
            className='flex items-center gap-2'
          >
            <RefreshCw className='h-4 w-4' />
            Try Again
          </Button>
          <Button
            variant='outline'
            onClick={handleGoBack}
          >
            Go Back
          </Button>
          <Button
            variant='ghost'
            onClick={handleGoHome}
          >
            Go Home
          </Button>
        </div>
      </div>
    </div>
  );
}
