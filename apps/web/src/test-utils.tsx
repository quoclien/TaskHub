import { ReactElement } from 'react';
import { render, RenderOptions } from '@testing-library/react';
import { SessionProvider } from 'next-auth/react';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { vi } from 'vitest';

// Mock session data
const mockSession = {
  user: {
    id: '1',
    name: 'Test User',
    email: 'test@example.com',
  },
  expires: '2024-12-31',
};

// Create a new QueryClient for each test to avoid state pollution
const createTestQueryClient = () =>
  new QueryClient({
    defaultOptions: {
      queries: {
        retry: false,
      },
      mutations: {
        retry: false,
      },
    },
  });

interface TestProvidersProps {
  children: React.ReactNode;
  session?: any;
}

const TestProviders = ({ children, session = null }: TestProvidersProps) => {
  const queryClient = createTestQueryClient();

  return (
    <QueryClientProvider client={queryClient}>
      <SessionProvider session={session}>
        {children}
      </SessionProvider>
    </QueryClientProvider>
  );
};

// Custom render function that includes providers
const customRender = (
  ui: ReactElement,
  options?: Omit<RenderOptions, 'wrapper'> & {
    session?: any;
  }
) => {
  const { session, ...renderOptions } = options || {};
  
  return render(ui, {
    wrapper: ({ children }) => (
      <TestProviders session={session}>{children}</TestProviders>
    ),
    ...renderOptions,
  });
};

// Re-export everything
export * from '@testing-library/react';
export { customRender as render };

// Export utilities for test setup
export { mockSession, createTestQueryClient };

// Mock hooks for testing
export const mockUseAuth = (overrides = {}) => {
  return {
    isAuthenticated: false,
    user: null,
    loading: false,
    signIn: vi.fn(),
    signOut: vi.fn(),
    ...overrides,
  };
};

export const mockUseBoardStore = (overrides = {}) => {
  return {
    boards: [],
    currentBoard: null,
    isLoading: false,
    fetchBoards: vi.fn(),
    setCurrentBoard: vi.fn(),
    ...overrides,
  };
};