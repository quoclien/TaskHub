'use client';

import { useEffect } from 'react';
import { useRouter } from 'next/navigation';
import Link from 'next/link';
import { CheckCircle, Users, Zap, ArrowRight, ExternalLink } from 'lucide-react';

import { Button } from '@/components/ui/button';
import { useAuth } from '@/hooks/useAuth';

interface HomePageProps {
  // Add any props here if needed
}

/**
 * HomePage Component
 *
 * Marketing landing page with hero section, features grid, and CTAs.
 * Redirects authenticated users to /boards.
 */
export default function HomePage({}: HomePageProps) {
  const router = useRouter();
  const { isAuthenticated, isLoading } = useAuth();

  // Redirect authenticated users to boards
  useEffect(() => {
    if (isAuthenticated && !isLoading) {
      router.push('/boards');
    }
  }, [isAuthenticated, isLoading, router]);

  // Don't render content for authenticated users while redirecting
  if (isAuthenticated && !isLoading) {
    return null;
  }

  // Hero Section
  const renderHero = () => (
    <div className='text-center space-y-8 mb-16'>
      <div className='space-y-4'>
        <div className='mx-auto h-20 w-20 bg-blue-600 rounded-full flex items-center justify-center mb-6'>
          <svg
            className='h-10 w-10 text-white'
            fill='none'
            stroke='currentColor'
            viewBox='0 0 24 24'
          >
            <path
              strokeLinecap='round'
              strokeLinejoin='round'
              strokeWidth={2}
              d='M9 17V7m0 10a2 2 0 01-2 2H5a2 2 0 01-2-2V7a2 2 0 012-2h2a2 2 0 012 2m0 10a2 2 0 002 2h2a2 2 0 002-2M9 7a2 2 0 012-2h2a2 2 0 002 2m0 0v10a2 2 0 002 2h2a2 2 0 002-2V7a2 2 0 00-2-2H5a2 2 0 00-2 2v10a2 2 0 002 2h2z'
            />
          </svg>
        </div>
        <h1 className='text-5xl font-bold tracking-tight bg-gradient-to-r from-blue-600 to-indigo-600 bg-clip-text text-transparent'>
          TaskHub
        </h1>
        <p className='text-xl text-gray-600 dark:text-gray-300 max-w-2xl mx-auto'>
          The modern task management platform for teams that get things done. 
          Organize, collaborate, and track progress in real-time.
        </p>
      </div>
      
      <div className='flex flex-col sm:flex-row gap-4 justify-center'>
        <Button
          asChild
          size='lg'
          className='bg-blue-600 hover:bg-blue-700 text-white px-8 py-3 text-lg'
        >
          <Link href='/sign-in'>
            Get Started
            <ArrowRight className='ml-2 h-5 w-5' />
          </Link>
        </Button>
        <Button
          variant='outline'
          size='lg'
          asChild
          className='px-8 py-3 text-lg border-gray-300 dark:border-gray-600'
        >
          <Link href='https://github.com/taskhub-team/taskhub' target='_blank' rel='noopener noreferrer'>
            View on GitHub
            <ExternalLink className='ml-2 h-5 w-5' />
          </Link>
        </Button>
      </div>
    </div>
  );

  // Features Grid
  const renderFeatures = () => (
    <div className='mb-16'>
      <div className='text-center mb-12'>
        <h2 className='text-3xl font-bold text-gray-900 dark:text-white mb-4'>
          Everything you need to stay organized
        </h2>
        <p className='text-lg text-gray-600 dark:text-gray-300'>
          Powerful features designed to help teams collaborate effectively
        </p>
      </div>
      
      <div className='grid md:grid-cols-3 gap-8 max-w-5xl mx-auto'>
        {/* Board Management */}
        <div className='bg-white dark:bg-gray-800 rounded-xl shadow-lg p-8 text-center'>
          <div className='mx-auto h-16 w-16 bg-blue-100 dark:bg-blue-900 rounded-full flex items-center justify-center mb-6'>
            <CheckCircle className='h-8 w-8 text-blue-600 dark:text-blue-400' />
          </div>
          <h3 className='text-xl font-semibold text-gray-900 dark:text-white mb-4'>
            Visual Board Management
          </h3>
          <p className='text-gray-600 dark:text-gray-300'>
            Organize tasks with intuitive Kanban boards. Drag and drop cards between lists to track progress effortlessly.
          </p>
        </div>

        {/* Real-time Updates */}
        <div className='bg-white dark:bg-gray-800 rounded-xl shadow-lg p-8 text-center'>
          <div className='mx-auto h-16 w-16 bg-green-100 dark:bg-green-900 rounded-full flex items-center justify-center mb-6'>
            <Zap className='h-8 w-8 text-green-600 dark:text-green-400' />
          </div>
          <h3 className='text-xl font-semibold text-gray-900 dark:text-white mb-4'>
            Real-time Collaboration
          </h3>
          <p className='text-gray-600 dark:text-gray-300'>
            See changes instantly as your team updates tasks. Stay synchronized with live updates and notifications.
          </p>
        </div>

        {/* Authentication */}
        <div className='bg-white dark:bg-gray-800 rounded-xl shadow-lg p-8 text-center'>
          <div className='mx-auto h-16 w-16 bg-purple-100 dark:bg-purple-900 rounded-full flex items-center justify-center mb-6'>
            <Users className='h-8 w-8 text-purple-600 dark:text-purple-400' />
          </div>
          <h3 className='text-xl font-semibold text-gray-900 dark:text-white mb-4'>
            Secure Authentication
          </h3>
          <p className='text-gray-600 dark:text-gray-300'>
            Sign in securely with Google OAuth or traditional credentials. Your data is protected and accessible only to you.
          </p>
        </div>
      </div>
    </div>
  );

  // Final CTA Section
  const renderFinalCTA = () => (
    <div className='text-center bg-white dark:bg-gray-800 rounded-xl shadow-lg p-12 max-w-4xl mx-auto'>
      <h2 className='text-3xl font-bold text-gray-900 dark:text-white mb-4'>
        Ready to boost your productivity?
      </h2>
      <p className='text-lg text-gray-600 dark:text-gray-300 mb-8'>
        Join thousands of teams already using TaskHub to organize their work and achieve their goals.
      </p>
      <Button
        asChild
        size='lg'
        className='bg-blue-600 hover:bg-blue-700 text-white px-8 py-3 text-lg'
      >
        <Link href='/sign-in'>
          Start organizing today
          <ArrowRight className='ml-2 h-5 w-5' />
        </Link>
      </Button>
    </div>
  );

  // Main Component
  return (
    <div className='min-h-screen bg-gradient-to-br from-blue-50 via-indigo-50 to-purple-50 dark:from-gray-900 dark:via-gray-800 dark:to-gray-900'>
      <main className='px-4 sm:px-6 lg:px-8 py-16'>
        <div className='max-w-7xl mx-auto'>
          {renderHero()}
          {renderFeatures()}
          {renderFinalCTA()}
        </div>
      </main>
    </div>
  );
}
