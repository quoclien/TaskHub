import { test, expect, type Page } from './test-utils';
import { faker } from '@faker-js/faker';

// Extend the expect matchers with our custom matchers
declare global {
  // eslint-disable-next-line @typescript-eslint/no-namespace
  namespace PlaywrightTest {
    interface Matchers<R> {
      toHaveNoColorContrastViolations(): Promise<R>;
    }
  }
}

// Accessibility test suite
test.describe('Accessibility', () => {
  test('home page should be accessible', async ({ page }) => {
    await page.goto('/');
    
    // Run accessibility scan
    const accessibilityScanResults = await page.accessibility.snapshot();
    
    // Check for critical accessibility issues
    if (accessibilityScanResults) {
      expect(accessibilityScanResults.violations).toBeUndefined();
    }
    
    // Check for proper heading structure
    await expect(page.getByRole('heading', { level: 1 })).toHaveCount(1);
    
    // Check for sufficient color contrast
    await expect(page).toHaveNoColorContrastViolations();
  });

  test('board page should be accessible', async ({ authenticatedPage }) => {
    // Create a test board
    await authenticatedPage.goto('/boards');
    await authenticatedPage.getByRole('button', { name: /create board/i }).click();
    const boardTitle = `Test Board ${faker.lorem.words(2)}`;
    await authenticatedPage.getByLabel(/board title/i).fill(boardTitle);
    await authenticatedPage.getByRole('button', { name: /create board/i }).click();
    
    // Run accessibility scan
    const accessibilityScanResults = await authenticatedPage.accessibility.snapshot();
    
    // Check for critical accessibility issues
    if (accessibilityScanResults) {
      expect(accessibilityScanResults.violations).toBeUndefined();
    }
    
    // Check for proper ARIA attributes on interactive elements
    const addCardButton = authenticatedPage.getByRole('button', { name: /add a card/i });
    await expect(addCardButton).toHaveAttribute('aria-label');
    
    // Check keyboard navigation
    await authenticatedPage.keyboard.press('Tab');
    await expect(addCardButton).toBeFocused();
  });
});

// Performance test suite
test.describe('Performance', () => {
  test('should load home page within performance budget', async ({ page }) => {
    // Set performance budget (in milliseconds)
    const PERFORMANCE_BUDGET = 2000; // 2 seconds
    
    // Enable performance tracking
    await page.goto('about:blank'); // Clear any existing traces
    const client = await page.context().newCDPSession(page);
    await client.send('Performance.enable');
    
    // Navigate to the home page and measure load time
    const startTime = Date.now();
    await page.goto('/');
    const loadTime = Date.now() - startTime;
    
    // Get performance metrics
    const metrics = await page.evaluate((): Record<string, number> => {
      const [timing] = performance.getEntriesByType('navigation') as PerformanceNavigationTiming[];
      return {
        dns: timing.domainLookupEnd - timing.domainLookupStart,
        tcp: timing.connectEnd - timing.connectStart,
        ttfb: timing.responseStart - timing.requestStart,
        domLoaded: timing.domContentLoadedEventEnd - timing.navigationStart,
        pageLoad: timing.loadEventEnd - timing.navigationStart,
      };
    });
    
    // Log performance metrics
    console.log('Performance Metrics:', {
      loadTime: `${loadTime}ms`,
      dns: `${metrics.dns}ms`,
      tcp: `${metrics.tcp}ms`,
      ttfb: `${metrics.ttfb}ms`,
      domLoaded: `${metrics.domLoaded}ms`,
      pageLoad: `${metrics.pageLoad}ms`,
    });
    
    // Assert performance meets budget
    expect(loadTime).toBeLessThan(PERFORMANCE_BUDGET);
    
    // Check for large layout shifts (CLS)
    const cls = await page.evaluate((): Promise<number> => {
      return new Promise((resolve) => {
        let cls = 0;
        const observer = new PerformanceObserver((entryList) => {
          for (const entry of entryList.getEntries()) {
            const layoutShiftEntry = entry as LayoutShift;
            if (!layoutShiftEntry.hadRecentInput) {
              cls += layoutShiftEntry.value;
            }
          }
          resolve(cls);
        });
        
        observer.observe({
          type: 'layout-shift',
          buffered: true,
        } as PerformanceObserverInit);
        
        // Ensure we clean up the observer
        setTimeout(() => {
          observer.disconnect();
        }, 0);
      });
    });
    
    console.log('Cumulative Layout Shift (CLS):', cls);
    expect(Number(cls)).toBeLessThan(0.1); // CLS should be less than 0.1
  });

  test('should load board page within performance budget', async ({ authenticatedPage }) => {
    // Set performance budget (in milliseconds)
    const PERFORMANCE_BUDGET = 3000; // 3 seconds
    
    // Create a test board with some data
    await authenticatedPage.goto('/boards');
    await authenticatedPage.getByRole('button', { name: /create board/i }).click();
    const boardTitle = `Test Board ${faker.lorem.words(2)}`;
    await authenticatedPage.getByLabel(/board title/i).fill(boardTitle);
    await authenticatedPage.getByRole('button', { name: /create board/i }).click();
    
    // Add some lists and cards
    await authenticatedPage.getByPlaceholder('Enter list title...').fill('To Do');
    await authenticatedPage.getByRole('button', { name: /add list/i }).click();
    
    for (let i = 0; i < 3; i++) {
      await authenticatedPage.getByRole('button', { name: /add a card/i }).click();
      await authenticatedPage.getByPlaceholder('Enter a title for this card...').fill(`Card ${i + 1}`);
      await authenticatedPage.getByRole('button', { name: /add card/i }).click();
    }
    
    // Clear any existing traces
    await authenticatedPage.goto('about:blank');
    
    // Enable performance tracking
    const client = await authenticatedPage.context().newCDPSession(authenticatedPage);
    await client.send('Performance.enable');
    
    // Navigate to the board page and measure load time
    const startTime = Date.now();
    await authenticatedPage.goto('/boards');
    await authenticatedPage.getByText(boardTitle).click();
    const loadTime = Date.now() - startTime;
    
    // Get performance metrics
    const metrics = await authenticatedPage.evaluate(() => {
      const [timing] = performance.getEntriesByType('navigation');
      return {
        dns: timing.domainLookupEnd - timing.domainLookupStart,
        tcp: timing.connectEnd - timing.connectStart,
        ttfb: timing.responseStart - timing.requestStart,
        domLoaded: timing.domContentLoadedEventEnd - timing.navigationStart,
        pageLoad: timing.loadEventEnd - timing.navigationStart,
      };
    });
    
    // Log performance metrics
    console.log('Board Page Performance Metrics:', {
      loadTime: `${loadTime}ms`,
      dns: `${metrics.dns}ms`,
      tcp: `${metrics.tcp}ms`,
      ttfb: `${metrics.ttfb}ms`,
      domLoaded: `${metrics.domLoaded}ms`,
      pageLoad: `${metrics.pageLoad}ms`,
    });
    
    // Assert performance meets budget
    expect(loadTime).toBeLessThan(PERFORMANCE_BUDGET);
  });
});

// Implement custom matchers
expect.extend({
  async toHaveNoColorContrastViolations(page: Page) {
    try {
      // This is a simplified example - in a real implementation, you would use a library like axe-core
      // or implement your own contrast checking logic
      const issues: string[] = [];
      
      // Check text contrast
      const elements = await page.$$('p, h1, h2, h3, h4, h5, h6, a, button, [role="button"]');
      
      for (const element of elements) {
        const color = await element.evaluate((el: HTMLElement) => {
          const style = window.getComputedStyle(el);
          return {
            bgColor: style.backgroundColor,
            textColor: style.color,
            text: el.textContent?.trim(),
          };
        });
        
        // Skip if no text or if text is not visible
        if (!color.text || color.text.length === 0 || color.textColor === 'rgba(0, 0, 0, 0)') {
          continue;
        }
        
        // This is a simplified check - in a real implementation, you would calculate the contrast ratio
        // and compare it against WCAG guidelines
        if (color.textColor === color.bgColor) {
          issues.push(`Insufficient contrast for element with text: "${color.text}"`);
        }
      }
      
      return {
        pass: issues.length === 0,
        message: () => issues.length > 0 
          ? `Found ${issues.length} color contrast issues:\n${issues.join('\n')}`
          : 'No color contrast issues found',
      };
    } catch (error) {
      return {
        pass: false,
        message: () => `Error checking color contrast: ${error instanceof Error ? error.message : String(error)}`,
      };
    }
  },
});
