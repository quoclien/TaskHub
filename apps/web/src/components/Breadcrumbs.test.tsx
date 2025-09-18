import { render, screen } from '@testing-library/react';
import { vi } from 'vitest';

import { Breadcrumbs } from './Breadcrumbs';

// Mock next/navigation
vi.mock('next/navigation', () => ({
  useParams: () => ({}),
  usePathname: () => '/',
}));

describe('Breadcrumbs', () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  it('renders home breadcrumb by default', () => {
    render(<Breadcrumbs />);

    expect(screen.getByLabelText('Breadcrumb')).toBeInTheDocument();
    expect(screen.getByRole('list')).toBeInTheDocument();
  });

  it('renders custom breadcrumb items when provided', () => {
    const customItems = [
      { label: 'Home', href: '/' },
      { label: 'Projects', href: '/projects' },
      { label: 'Current Project', isCurrent: true },
    ];

    render(<Breadcrumbs items={customItems} />);

    // Check for home icon instead of text
    expect(screen.getByTestId('home-icon')).toBeInTheDocument();
    expect(screen.getByText('Projects')).toBeInTheDocument();
    expect(screen.getByText('Current Project')).toBeInTheDocument();
  });

  it('applies correct aria-current attribute to current page', () => {
    const customItems = [
      { label: 'Home', href: '/' },
      { label: 'Current Page', isCurrent: true },
    ];

    render(<Breadcrumbs items={customItems} />);

    const currentPage = screen.getByText('Current Page');
    expect(currentPage.getAttribute('aria-current')).toBe('page');
  });

  it('renders home icon for first breadcrumb', () => {
    render(<Breadcrumbs />);

    const homeIcon = screen.getByTestId('home-icon');
    expect(homeIcon).toBeInTheDocument();
  });
});
