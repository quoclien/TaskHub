import { ReactElement, ReactNode } from 'react';
import { render, RenderOptions } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { BrowserRouter } from 'react-router-dom';
import { server } from './mocks/server';
import { http } from 'msw';

type HttpMethod = 'get' | 'post' | 'put' | 'delete' | 'patch';

declare global {
  // eslint-disable-next-line @typescript-eslint/no-namespace
  namespace jest {
    interface Matchers<R> {
      toBeInTheDocument(): R;
    }
  }
}

// Create a custom render function that includes providers
const customRender = (
  ui: ReactElement,
  options?: Omit<RenderOptions, 'wrapper'>
) => {
  // Create a new QueryClient for each test
  const queryClient = new QueryClient({
    defaultOptions: {
      queries: {
        retry: false, // Disable retries for testing
      },
    },
  });

  const Wrapper = ({ children }: { children: ReactNode }) => (
    <QueryClientProvider client={queryClient}>
      <BrowserRouter>
        {children}
      </BrowserRouter>
    </QueryClientProvider>
  );

  return {
    user: userEvent.setup(),
    ...render(ui, { wrapper: Wrapper, ...options }),
  };
};

// Re-export everything
export * from '@testing-library/react';
// Override render method
export { customRender as render };

// Re-export user event for convenience
export * from '@testing-library/user-event';

// Export MSW utilities
export { server };

// Helper to wait for loading to finish
export const waitForLoadingToFinish = () =>
  new Promise((resolve) => setTimeout(resolve, 0));

// Helper to mock API errors
export const mockApiError = (method: HttpMethod, endpoint: string, status = 500, error: unknown = {}) => {
  const handler = http[method](endpoint, () => {
    return new Response(JSON.stringify(error), {
      status,
      headers: {
        'Content-Type': 'application/json',
      },
    });
  });
  
  server.use(handler);
  return handler;
};
