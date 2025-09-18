import { Suspense } from 'react';

import { BoardComponentsShowcase } from '@/components/board/BoardComponentsShowcase';

export default function TestComponentsPage() {
  return (
    <Suspense fallback={<div>Loading components...</div>}>
      <BoardComponentsShowcase />
    </Suspense>
  );
}
