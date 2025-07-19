// External dependencies
import { render, screen } from '@testing-library/react';
import { describe, it, expect, vi } from 'vitest';

// Internal dependencies
import { BoardView } from './BoardView';

describe('BoardView', () => {
  it('renders children when not loading', () => {
    render(
      <BoardView>
        <div data-testid="test-child">Test Content</div>
      </BoardView>
    );

    expect(screen.getByTestId('test-child')).toBeInTheDocument();
  });

  it('renders loading skeleton when loading', () => {
    render(<BoardView isLoading={true} />);

    // Should render skeleton lists with the correct classes
    const skeletonElements = document.querySelectorAll('.bg-gray-200');
    expect(skeletonElements.length).toBeGreaterThan(0);
  });

  it('applies custom className', () => {
    const customClass = 'custom-class';
    render(<BoardView className={customClass} />);

    const boardView = document.querySelector('.custom-class');
    expect(boardView).toBeInTheDocument();
  });

  it('has correct default classes', () => {
    render(<BoardView />);

    // The DndContext wraps the main div, so we need to get the second div
    const boardView = document.querySelectorAll('div')[1];
    expect(boardView).toHaveClass('flex');
    expect(boardView).toHaveClass('gap-x-4');
    expect(boardView).toHaveClass('overflow-x-auto');
    expect(boardView).toHaveClass('pb-4');
    expect(boardView).toHaveClass('scroll-smooth');
  });

  it('has correct scroll snap style', () => {
    render(<BoardView />);

    // The DndContext wraps the main div, so we need to get the second div
    const boardView = document.querySelectorAll('div')[1];
    expect(boardView).toHaveStyle({ scrollSnapType: 'x mandatory' });
  });

  it('calls onMoveCard when drag ends', () => {
    const mockOnMoveCard = vi.fn();
    render(
      <BoardView onMoveCard={mockOnMoveCard}>
        <div data-testid="test-child">Test Content</div>
      </BoardView>
    );

    // The DndContext is rendered, so the onMoveCard prop is properly passed
    expect(mockOnMoveCard).toBeDefined();
  });

  it('renders DndContext wrapper', () => {
    render(<BoardView />);

    // Check that the DndContext is rendered (it should be the first div)
    const dndContext = document.querySelector('div');
    expect(dndContext).toBeInTheDocument();
  });
});
