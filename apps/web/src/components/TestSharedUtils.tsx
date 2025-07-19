'use client';

import { formatDate, isToday, formatDuration } from '@taskhub/shared';

export function TestSharedUtils() {
  const now = new Date();
  const yesterday = new Date(now);
  yesterday.setDate(yesterday.getDate() - 1);

  const durationInMs = 3661000; // 1 hour, 1 minute, 1 second

  return (
    <div className='p-4 border rounded-lg bg-card text-card-foreground'>
      <h2 className='text-lg font-semibold mb-4'>Shared Utilities Test</h2>

      <div className='space-y-4'>
        <div>
          <h3 className='font-medium'>Date Formatting</h3>
          <p>Today: {formatDate(now)}</p>
          <p>
            Yesterday: {formatDate(yesterday, { year: 'numeric', month: 'long', day: 'numeric' })}
          </p>
          <p>Is today: {isToday(now) ? 'Yes' : 'No'}</p>
        </div>

        <div>
          <h3 className='font-medium'>Duration Formatting</h3>
          <p>
            {durationInMs}ms = {formatDuration(durationInMs)}
          </p>
        </div>
      </div>
    </div>
  );
}
