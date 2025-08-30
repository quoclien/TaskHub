'use client';

import { AlertTriangle, ArrowLeft } from 'lucide-react';
import Link from 'next/link';
import { useSearchParams } from 'next/navigation';
import { Suspense } from 'react';

import { Button } from '@/components/ui/button';

function AuthErrorContent() {
  const searchParams = useSearchParams();
  const error = searchParams.get('error');

  const getErrorMessage = (error: string | null) => {
    switch (error) {
      case 'Configuration':
        return 'There is a problem with the server configuration.';
      case 'AccessDenied':
        return 'You do not have permission to sign in with this account.';
      case 'Verification':
        return 'The sign in link is no longer valid. It may have been used already or it may have expired.';
      case 'OAuthSignin':
      case 'OAuthCallback':
      case 'OAuthCreateAccount':
      case 'EmailCreateAccount':
      case 'Callback':
        return 'There was an error with the authentication provider. Please try again.';
      case 'OAuthAccountNotLinked':
        return 'To confirm your identity, sign in with the same account you used originally.';
      case 'EmailSignin':
        return 'The email could not be sent.';
      case 'CredentialsSignin':
        return 'Sign in failed. Check the details you provided are correct.';
      case 'SessionRequired':
        return 'Please sign in to access this page.';
      default:
        return 'An unexpected error occurred. Please try again.';
    }
  };

  return (
    <div className='min-h-screen bg-gradient-to-br from-red-50 to-orange-100 dark:from-gray-900 dark:to-gray-800 flex items-center justify-center px-4 sm:px-6 lg:px-8'>
      <div className='max-w-md w-full space-y-8'>
        <div className='text-center'>
          <div className='mx-auto h-16 w-16 bg-red-600 rounded-full flex items-center justify-center mb-4'>
            <AlertTriangle className='h-8 w-8 text-white' />
          </div>
          <h1 className='text-3xl font-bold text-gray-900 dark:text-gray-100'>
            Authentication Error
          </h1>
          <p className='mt-2 text-sm text-gray-600 dark:text-gray-400'>
            There was a problem signing you in
          </p>
        </div>

        <div className='bg-white dark:bg-gray-800 rounded-lg shadow-lg p-6 space-y-4'>
          <div className='bg-red-50 dark:bg-red-900/20 border border-red-200 dark:border-red-800 rounded-md p-4'>
            <h2 className='text-sm font-medium text-red-800 dark:text-red-400 mb-2'>
              Error Details
            </h2>
            <p className='text-sm text-red-700 dark:text-red-300'>{getErrorMessage(error)}</p>
            {error && (
              <p className='text-xs text-red-600 dark:text-red-400 mt-2'>Error code: {error}</p>
            )}
          </div>

          <div className='space-y-3'>
            <Button
              asChild
              className='w-full'
            >
              <Link href='/sign-in'>Try Again</Link>
            </Button>

            <Button
              variant='outline'
              asChild
              className='w-full'
            >
              <Link
                href='/'
                className='flex items-center justify-center gap-2'
              >
                <ArrowLeft className='h-4 w-4' />
                Back to Home
              </Link>
            </Button>
          </div>
        </div>

        <div className='text-center'>
          <p className='text-sm text-gray-600 dark:text-gray-400'>
            If the problem persists, please{' '}
            <a
              href='mailto:support@taskhub.dev'
              className='text-blue-600 hover:text-blue-500 dark:text-blue-400 dark:hover:text-blue-300 underline'
            >
              contact support
            </a>
          </p>
        </div>
      </div>
    </div>
  );
}

export default function AuthError() {
  return (
    <Suspense
      fallback={
        <div className='min-h-screen flex items-center justify-center'>
          <div className='animate-spin rounded-full h-8 w-8 border-b-2 border-blue-600'></div>
        </div>
      }
    >
      <AuthErrorContent />
    </Suspense>
  );
}
