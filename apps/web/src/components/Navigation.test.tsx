import { screen, waitFor, act } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { vi } from 'vitest';

import { render, mockUseAuth } from '@/test-utils';
import { Navigation } from './Navigation';
import { useAuthStore } from '@/hooks/useAuthStore';

// Mock next/navigation
const mockPush = vi.fn();
vi.mock('next/navigation', () => ({
  useRouter: () => ({
    push: mockPush,
  }),
}));

// Mock useAuth hook
vi.mock('@/hooks/useAuth', () => ({
  useAuth: vi.fn(),
}));

// Mock useAuthStore hook  
vi.mock('@/hooks/useAuthStore', () => ({
  useAuthStore: vi.fn(),
}));

// Mock the board API
vi.mock('@/lib/api/board', () => ({
  fetchBoards: vi.fn().mockResolvedValue([]),
}));

const { useAuth } = await import('@/hooks/useAuth');
const { fetchBoards } = await import('@/lib/api/board');

describe('Navigation', () => {
  let user: ReturnType<typeof userEvent.setup>;

  beforeEach(() => {
    vi.clearAllMocks();
    user = userEvent.setup();
    vi.mocked(useAuthStore).mockReturnValue({
      token: null,
      setToken: vi.fn(),
    });
    vi.mocked(fetchBoards as any).mockResolvedValue([]);
  });

  describe('when not authenticated', () => {
    it('renders TaskHub logo and sign in/sign up buttons', () => {
      render(<Navigation />, {});

      expect(screen.getByText('TaskHub')).toBeInTheDocument();
      expect(screen.getByText('Sign In')).toBeInTheDocument();
      expect(screen.getByText('Sign Up')).toBeInTheDocument();
    });

    it('does not render board switcher or user menu', () => {
      render(<Navigation />, {});

      expect(screen.queryByText('Select Board')).not.toBeInTheDocument();
      expect(screen.queryByText('Account')).not.toBeInTheDocument();
      expect(screen.queryByText('Loading boards...')).not.toBeInTheDocument();
    });

    it('renders sign in and sign up links correctly', () => {
      render(<Navigation />, {});

      const signInLink = screen.getByRole('link', { name: 'Sign In' });
      const signUpLink = screen.getByRole('link', { name: 'Sign Up' });

      expect(signInLink).toHaveAttribute('href', '/sign-in');
      expect(signUpLink).toHaveAttribute('href', '/sign-up');
    });
  });

  describe('when authenticated', () => {
    const mockBoards = [
      { id: '1', title: 'Project Alpha', description: 'Main project board' },
      { id: '2', title: 'Project Beta', description: 'Secondary project board' },
    ];

    beforeEach(() => {
      vi.mocked(useAuthStore).mockReturnValue({
        token: 'mock-token',
        setToken: vi.fn(),
      });
      vi.mocked(fetchBoards as any).mockResolvedValue(mockBoards);
    });

    it('renders board switcher and user menu when authenticated', async () => {
      render(<Navigation currentBoardId='1' />, {});

      // Wait for boards to load
      await waitFor(() => {
        expect(screen.getByText('Project Alpha')).toBeInTheDocument();
      });

      expect(screen.getByText('Account')).toBeInTheDocument();
    });

    it('shows current board title in board switcher', async () => {
      render(<Navigation currentBoardId='1' />, {});

      await waitFor(() => {
        expect(screen.getByText('Project Alpha')).toBeInTheDocument();
      });
    });

    it('shows "Select Board" when no current board is selected', async () => {
      render(<Navigation />, {});

      await waitFor(() => {
        expect(screen.getByText('Select Board')).toBeInTheDocument();
      });
    });

    it('opens board switcher dropdown when clicked', async () => {
      render(<Navigation />, {});

      await waitFor(() => {
        expect(screen.getByText('Select Board')).toBeInTheDocument();
      });

      const boardSwitcher = screen.getByText('Select Board');

      await act(async () => {
        await user.click(boardSwitcher);
      });

      // Check that board options are visible
      await waitFor(() => {
        expect(screen.getByText('Project Alpha')).toBeInTheDocument();
        expect(screen.getByText('Project Beta')).toBeInTheDocument();
        expect(screen.getByText('View All Boards')).toBeInTheDocument();
      });
    });

    it('navigates to selected board when board option is clicked', async () => {
      render(<Navigation />, {});

      await waitFor(() => {
        expect(screen.getByText('Select Board')).toBeInTheDocument();
      });

      const boardSwitcher = screen.getByText('Select Board');

      await act(async () => {
        await user.click(boardSwitcher);
      });

      await waitFor(() => {
        expect(screen.getByText('Project Alpha')).toBeInTheDocument();
      });

      const projectAlphaOption = screen.getByText('Project Alpha');

      await act(async () => {
        await user.click(projectAlphaOption);
      });

      expect(mockPush).toHaveBeenCalledWith('/board/1');
    });

    it('opens user menu dropdown when account button is clicked', async () => {
      render(<Navigation />, {});

      await waitFor(() => {
        expect(screen.getByText('Account')).toBeInTheDocument();
      });

      const accountButton = screen.getByText('Account');

      await act(async () => {
        await user.click(accountButton);
      });

      // Check that menu items are visible
      await waitFor(() => {
        expect(screen.getByText('Profile')).toBeInTheDocument();
        expect(screen.getByText('Settings')).toBeInTheDocument();
        expect(screen.getByText('Sign Out')).toBeInTheDocument();
      });
    });

    it('handles sign out when sign out button is clicked', async () => {
      const mockSetToken = vi.fn();
      vi.mocked(useAuthStore).mockReturnValue({
        token: 'mock-token',
        setToken: mockSetToken,
      });

      render(<Navigation />, {});

      await waitFor(() => {
        expect(screen.getByText('Account')).toBeInTheDocument();
      });

      const accountButton = screen.getByText('Account');

      await act(async () => {
        await user.click(accountButton);
      });

      await waitFor(() => {
        expect(screen.getByText('Sign Out')).toBeInTheDocument();
      });

      const signOutButton = screen.getByText('Sign Out');

      await act(async () => {
        await user.click(signOutButton);
      });

      expect(mockSetToken).toHaveBeenCalledWith(null);
      expect(mockPush).toHaveBeenCalledWith('/sign-in');
    });

    it('handles profile navigation when profile button is clicked', async () => {
      const consoleSpy = vi.spyOn(console, 'log').mockImplementation(() => {});

      render(<Navigation />, {});

      await waitFor(() => {
        expect(screen.getByText('Account')).toBeInTheDocument();
      });

      const accountButton = screen.getByText('Account');

      await act(async () => {
        await user.click(accountButton);
      });

      await waitFor(() => {
        expect(screen.getByText('Profile')).toBeInTheDocument();
      });

      const profileButton = screen.getByText('Profile');

      await act(async () => {
        await user.click(profileButton);
      });

      expect(consoleSpy).toHaveBeenCalledWith('Navigate to profile');
      consoleSpy.mockRestore();
    });

    it('handles settings navigation when settings button is clicked', async () => {
      const consoleSpy = vi.spyOn(console, 'log').mockImplementation(() => {});

      render(<Navigation />, {});

      await waitFor(() => {
        expect(screen.getByText('Account')).toBeInTheDocument();
      });

      const accountButton = screen.getByText('Account');

      await act(async () => {
        await user.click(accountButton);
      });

      await waitFor(() => {
        expect(screen.getByText('Settings')).toBeInTheDocument();
      });

      const settingsButton = screen.getByText('Settings');

      await act(async () => {
        await user.click(settingsButton);
      });

      expect(consoleSpy).toHaveBeenCalledWith('Navigate to settings');
      consoleSpy.mockRestore();
    });

    it('shows loading state for boards', () => {
      vi.mocked(fetchBoards as any).mockImplementation(() => new Promise(() => {})); // Never resolves

      render(<Navigation />, {});

      expect(screen.getByText('Loading boards...')).toBeInTheDocument();
    });

    it('does not show board switcher when boards are loading and no current board', () => {
      vi.mocked(fetchBoards as any).mockImplementation(() => new Promise(() => {})); // Never resolves

      render(<Navigation />, {});

      expect(screen.getByText('Loading boards...')).toBeInTheDocument();
      expect(screen.queryByText('Select Board')).not.toBeInTheDocument();
    });
  });

  describe('authentication state changes', () => {
    it('shows different content based on authentication state', async () => {
      // Start with not authenticated
      vi.mocked(useAuthStore).mockReturnValue({
        token: null,
        setToken: vi.fn(),
      });

      const { rerender } = render(<Navigation />, {});

      expect(screen.getByText('Sign In')).toBeInTheDocument();
      expect(screen.queryByText('Account')).not.toBeInTheDocument();

      // Change to authenticated
      vi.mocked(useAuthStore).mockReturnValue({
        token: 'mock-token',
        setToken: vi.fn(),
      });
      vi.mocked(fetchBoards as any).mockResolvedValue([]);

      rerender(<Navigation />);

      await waitFor(() => {
        expect(screen.queryByText('Sign In')).not.toBeInTheDocument();
        expect(screen.getByText('Account')).toBeInTheDocument();
      });
    });
  });

  describe('accessibility', () => {
    it('has proper ARIA labels for dropdown menus', async () => {
      vi.mocked(useAuthStore).mockReturnValue({
        token: 'mock-token',
        setToken: vi.fn(),
      });
      vi.mocked(fetchBoards as any).mockResolvedValue([]);

      render(<Navigation />, {});

      await waitFor(() => {
        expect(screen.getByText('Account')).toBeInTheDocument();
      });

      const accountButton = screen.getByRole('button', { name: /account/i });
      expect(accountButton).toBeInTheDocument();
    });

    it('supports keyboard navigation for dropdown menus', async () => {
      vi.mocked(useAuthStore).mockReturnValue({
        token: 'mock-token',
        setToken: vi.fn(),
      });
      vi.mocked(fetchBoards as any).mockResolvedValue([]);

      render(<Navigation />, {});

      await waitFor(() => {
        expect(screen.getByText('Account')).toBeInTheDocument();
      });

      const accountButton = screen.getByRole('button', { name: /account/i });

      // Focus the button
      await act(async () => {
        accountButton.focus();
      });

      expect(accountButton).toHaveFocus();

      // Press Enter to open menu
      await act(async () => {
        await user.keyboard('{Enter}');
      });

      await waitFor(() => {
        expect(screen.getByText('Sign Out')).toBeInTheDocument();
      });
    });
  });
});
