// External dependencies
import { render, screen } from '@testing-library/react';
import { describe, it, expect } from 'vitest';

// Internal dependencies
import { BoardView } from './BoardView';

describe('BoardView', () => {
  it('renders children when not loading', () => {
    render(
      <BoardView>
        <div>Test List 1</div>
        <div>Test List 2</div>
      </BoardView>
    );

    expect(screen.getByText('Test List 1')).toBeInTheDocument();
    expect(screen.getByText('Test List 2')).toBeInTheDocument();
  });

  it('renders skeleton loading state when isLoading is true', () => {
    render(<BoardView isLoading={true} />);

    // Should render 3 skeleton lists
    const skeletons = screen.getAllByRole('generic');
    const skeletonLists = skeletons.filter(el => 
      el.className.includes('w-72') && el.className.includes('bg-gray-200')
    );
    expect(skeletonLists).toHaveLength(3);
  });

  it('applies custom className', () => {
    const customClass = 'custom-board-view';
    const { container } = render(
      <BoardView className={customClass}>
        <div>Content</div>
      </BoardView>
    );

    const boardView = container.firstChild as HTMLElement;
    expect(boardView).toHaveClass(customClass);
  });

  it('has correct scroll styling classes', () => {
    const { container } = render(
      <BoardView>
        <div>Content</div>
      </BoardView>
    );

    const boardView = container.firstChild as HTMLElement;
    expect(boardView).toHaveClass('flex');
    expect(boardView).toHaveClass('gap-x-4');
    expect(boardView).toHaveClass('overflow-x-auto');
    expect(boardView).toHaveClass('pb-4');
    expect(boardView).toHaveClass('scroll-smooth');
  });

  it('sets scroll-snap-type style', () => {
    const { container } = render(
      <BoardView>
        <div>Content</div>
      </BoardView>
    );

    const boardView = container.firstChild as HTMLElement;
    expect(boardView).toHaveStyle({ scrollSnapType: 'x mandatory' });
  });

  it('forwards ref correctly', () => {
    let ref: HTMLDivElement | null = null;
    
    render(
      <BoardView
        ref={(element) => {
          ref = element;
        }}
      >
        <div>Content</div>
      </BoardView>
    );

    expect(ref).toBeInstanceOf(HTMLDivElement);
  });
}); 