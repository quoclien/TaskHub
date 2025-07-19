'use client';

// External dependencies
import { Button } from '@/components/ui/button';

// Types
interface HomePageProps {
  // Add any props here if needed
}

/**
 * HomePage Component
 *
 * Displays the main landing page of the application with a welcome message
 * and call-to-action buttons.
 */
export default function HomePage({}: HomePageProps) {
  // Event Handlers
  const handleGetStarted = () => {
    // TODO: Implement navigation to dashboard or signup
    console.log('Get Started clicked');
  };

  const handleLearnMore = () => {
    // TODO: Implement navigation to features or about page
    console.log('Learn More clicked');
  };

  // Render Functions
  const renderHeader = () => (
    <div className='text-center space-y-2'>
      <h1 className='text-4xl font-bold tracking-tight'>TaskHub</h1>
      <p className='text-lg text-muted-foreground'>Your task management solution</p>
    </div>
  );

  const renderCallToAction = () => (
    <div className='flex flex-col sm:flex-row gap-4'>
      <Button
        onClick={handleGetStarted}
        size='lg'
      >
        Get Started
      </Button>
      <Button
        variant='outline'
        onClick={handleLearnMore}
        size='lg'
      >
        Learn More
      </Button>
    </div>
  );

  // Main Component
  return (
    <div className='flex min-h-screen flex-col items-center justify-center p-8'>
      <main className='flex flex-col items-center gap-8 max-w-2xl text-center'>
        {renderHeader()}
        {renderCallToAction()}
      </main>
    </div>
  );
}
