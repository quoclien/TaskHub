// External dependencies
import { render, screen } from '@testing-library/react';
import { describe, it, expect, vi } from 'vitest';

// Internal dependencies
import { BoardCard } from './BoardCard';

// Mock the shared utility
vi.mock('@taskhub/shared', () => ({
  formatRelativeTime: vi.fn((date: string | Date | number) => {
    if (typeof date === 'string') {
      return '2 days ago';
    }
    return '1 hour ago';
  }),
}));

describe('BoardCard', () => {
  const defaultProps = {
    id: 'board-123',
    title: 'Project Alpha',
    memberCount: 3,
    lastActivity: '2024-01-15T10:30:00Z',
  };

  it('renders board information correctly', () => {
    render(<BoardCard {...defaultProps} />);

    // Check title
    expect(screen.getByText('Project Alpha')).toBeInTheDocument();

    // Check member count
    expect(screen.getByText('3 members')).toBeInTheDocument();

    // Check last activity
    expect(screen.getByText(/Updated 2 days ago/)).toBeInTheDocument();
  });

  it('handles singular member count correctly', () => {
    render(
      <BoardCard
        {...defaultProps}
        memberCount={1}
      />
    );

    expect(screen.getByText('1 member')).toBeInTheDocument();
  });

  it('renders correct link href', () => {
    render(<BoardCard {...defaultProps} />);

    const link = screen.getByRole('link');
    expect(link).toHaveAttribute('href', '/board/board-123');
  });

  it('applies custom className', () => {
    const customClass = 'custom-board-card';
    render(
      <BoardCard
        {...defaultProps}
        className={customClass}
      />
    );

    const link = screen.getByRole('link');
    expect(link).toHaveClass(customClass);
  });

  it('has proper accessibility attributes', () => {
    render(<BoardCard {...defaultProps} />);

    const link = screen.getByRole('link');
    expect(link).toHaveClass('focus-visible:outline-none');
    expect(link).toHaveClass('focus-visible:ring-2');
  });

  it('handles Date object for lastActivity', () => {
    const dateObject = new Date('2024-01-15T10:30:00Z');
    render(
      <BoardCard
        {...defaultProps}
        lastActivity={dateObject}
      />
    );

    expect(screen.getByText(/Updated 1 hour ago/)).toBeInTheDocument();
  });

  it('truncates long titles with line-clamp', () => {
    const longTitle = 'This is a very long board title that should be truncated with line clamp';
    render(
      <BoardCard
        {...defaultProps}
        title={longTitle}
      />
    );

    const titleElement = screen.getByText(longTitle);
    expect(titleElement).toHaveClass('line-clamp-2');
  });
});
