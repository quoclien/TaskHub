'use client';

import { ChevronRight, Home } from 'lucide-react';
import Link from 'next/link';
import { useParams, usePathname } from 'next/navigation';

// Types
interface BreadcrumbItem {
  label: string;
  href?: string;
  isCurrent?: boolean;
}

interface BreadcrumbsProps {
  items?: BreadcrumbItem[];
}

// Component
export const Breadcrumbs = ({ items }: BreadcrumbsProps) => {
  const params = useParams();
  const pathname = usePathname();

  // Hooks
  const generateBreadcrumbs = (): BreadcrumbItem[] => {
    if (items) return items;

    const segments = pathname.split('/').filter(Boolean);
    const breadcrumbs: BreadcrumbItem[] = [{ label: 'Home', href: '/' }];

    let currentPath = '';

    segments.forEach((segment, index) => {
      // Special case for boards screen
      if (segment === 'board') {
        breadcrumbs.push({
          label: 'Board',
          href: index === segments.length - 1 ? undefined : '/boards',
          isCurrent: index === segments.length - 1,
        });
        return;
      }

      currentPath += `/${segment}`;

      // Handle board detail page
      if (segment === 'board') {
        breadcrumbs.push({
          label: 'Boards',
          href: '/boards',
        });
      }
      // Handle dynamic segments
      else if (segment === '[boardId]' || segment.startsWith('[')) {
        const paramKey = segment.replace(/[\[\]]/g, '');
        const paramValue = params[paramKey];
        if (paramValue) {
          breadcrumbs.push({
            label: typeof paramValue === 'string' ? paramValue : 'Board',
            href: index === segments.length - 1 ? undefined : currentPath,
            isCurrent: index === segments.length - 1,
          });
        }
      } else {
        // Handle static segments
        const label = segment.charAt(0).toUpperCase() + segment.slice(1);
        breadcrumbs.push({
          label,
          href: index === segments.length - 1 ? undefined : currentPath,
          isCurrent: index === segments.length - 1,
        });
      }
    });

    return breadcrumbs;
  };

  const breadcrumbs = generateBreadcrumbs();

  // Event Handlers
  const handleKeyDown = (event: React.KeyboardEvent, href?: string) => {
    if (event.key === 'Enter' || event.key === ' ') {
      event.preventDefault();
      if (href) {
        window.location.href = href;
      }
    }
  };

  // Render Helpers
  const renderBreadcrumbItem = (item: BreadcrumbItem, index: number) => {
    const isLast = index === breadcrumbs.length - 1;

    return (
      <li
        key={index}
        className='flex items-center'
      >
        {index > 0 && <ChevronRight className='h-4 w-4 text-muted-foreground mx-2' />}

        {item.href && !isLast ? (
          <Link
            href={item.href}
            className='text-sm text-muted-foreground hover:text-foreground transition-colors focus:outline-none focus:ring-2 focus:ring-ring focus:ring-offset-2 rounded'
            onKeyDown={(e) => {
              handleKeyDown(e, item.href);
            }}
            tabIndex={0}
          >
            {index === 0 ? (
              <Home
                className='h-4 w-4'
                data-testid='home-icon'
              />
            ) : (
              item.label
            )}
          </Link>
        ) : (
          <span
            className={`text-sm ${
              isLast ? 'text-foreground font-medium' : 'text-muted-foreground'
            }`}
            aria-current={isLast ? 'page' : undefined}
          >
            {index === 0 ? (
              <Home
                className='h-4 w-4'
                data-testid='home-icon'
              />
            ) : (
              item.label
            )}
          </span>
        )}
      </li>
    );
  };

  // Main JSX Return
  return (
    <nav
      aria-label='Breadcrumb'
      className='mb-4'
    >
      <ol className='flex items-center space-x-1'>{breadcrumbs.map(renderBreadcrumbItem)}</ol>
    </nav>
  );
};
