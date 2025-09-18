import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { render, screen, waitFor } from '@testing-library/react';
import { vi } from 'vitest';

import * as boardApi from '@/lib/api/board';

import { CardDetailModal } from './CardDetailModal';

// Mock the API
vi.mock('@/lib/api/board', () => ({
  getCardById: vi.fn(),
}));

// Mock Next.js useSearchParams
vi.mock('next/navigation', () => ({
  useSearchParams: vi.fn(() => ({
    get: vi.fn((key: string) => (key === 'cardId' ? 'test-card-id' : null)),
  })),
  useRouter: vi.fn(() => ({
    push: vi.fn(),
    replace: vi.fn(),
  })),
}));

const mockCard = {
  id: 'test-card-id',
  title: 'Test Card Title',
  description: 'Test card description',
  listId: 'test-list-id',
  position: 0,
  createdAt: '2024-01-01T00:00:00Z',
  updatedAt: '2024-01-01T00:00:00Z',
  metadata: {},
  dueDate: null,
  isArchived: false,
  assignedTo: null,
  comments: [],
};

function TestWrapper({ children }: { children: React.ReactNode }) {
  const queryClient = new QueryClient({
    defaultOptions: {
      queries: {
        retry: false,
      },
    },
  });

  return <QueryClientProvider client={queryClient}>{children}</QueryClientProvider>;
}

describe('CardDetailModal', () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  it('should render loading state initially', async () => {
    (boardApi.getCardById as any).mockImplementation(
      () => new Promise(() => {}) // Never resolves
    );

    render(
      <TestWrapper>
        <CardDetailModal />
      </TestWrapper>
    );

    expect(screen.getByTestId('card-detail-loading')).toBeInTheDocument();
  });

  it('should display card details when data is loaded', async () => {
    (boardApi.getCardById as any).mockResolvedValue(mockCard);

    render(
      <TestWrapper>
        <CardDetailModal />
      </TestWrapper>
    );

    await waitFor(() => {
      expect(screen.getByText('Test Card Title')).toBeInTheDocument();
    });

    // Check if the description appears in the description section (not dialog description)
    expect(screen.getByText('Description')).toBeInTheDocument();
    const descriptionElements = screen.getAllByText('Test card description');
    expect(descriptionElements.length).toBeGreaterThan(0);
  });

  it('should display error message when API call fails', async () => {
    (boardApi.getCardById as any).mockRejectedValue(new Error('Failed to fetch card'));

    render(
      <TestWrapper>
        <CardDetailModal />
      </TestWrapper>
    );

    await waitFor(() => {
      expect(screen.getByText('Error')).toBeInTheDocument();
    });

    expect(screen.getByText('Please try again later')).toBeInTheDocument();
  });

  it('should not render when no cardId in URL', async () => {
    // Mock useSearchParams to return null cardId
    const { useSearchParams } = await import('next/navigation');
    (useSearchParams as any).mockReturnValue({
      get: vi.fn(() => null),
    });

    const { container } = render(
      <TestWrapper>
        <CardDetailModal />
      </TestWrapper>
    );

    expect(container.firstChild).toBeNull();
  });
});
