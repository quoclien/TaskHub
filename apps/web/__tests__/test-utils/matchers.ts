import { expect, type Locator } from '@playwright/test';

declare global {
  // eslint-disable-next-line @typescript-eslint/no-namespace
  namespace PlaywrightTest {
    interface Matchers<R> {
      toBeVisibleAndContainText(expected: string | RegExp): Promise<R>;
      toHaveNoColorContrastViolations(): Promise<R>;
      toBeInViewport(): Promise<R>;
    }
  }
}

expect.extend({
  async toBeVisibleAndContainText(
    locator: Locator,
    expected: string | RegExp
  ) {
    const isVisible = await locator.isVisible();
    const text = await locator.textContent();
    const pass = isVisible && text !== null && new RegExp(expected).test(text);
    
    return {
      pass,
      message: () =>
        pass
          ? 'passed'
          : `Expected element to be visible and contain "${expected}" but found "${text}"`,
    };
  },

  async toHaveNoColorContrastViolations(page: any) {
    // This is a simplified example - in a real implementation, you would use a library like axe-core
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
  },

  async toBeInViewport(locator: Locator) {
    const isInViewport = await locator.evaluate((el) => {
      const rect = el.getBoundingClientRect();
      return (
        rect.top >= 0 &&
        rect.left >= 0 &&
        rect.bottom <= (window.innerHeight || document.documentElement.clientHeight) &&
        rect.right <= (window.innerWidth || document.documentElement.clientWidth)
      );
    });

    return {
      pass: isInViewport,
      message: () => `Expected element to be in viewport`,
    };
  },
});

// Re-export custom matchers
export const customMatchers = {
  toBeVisibleAndContainText: expect.toBeVisibleAndContainText,
  toHaveNoColorContrastViolations: expect.toHaveNoColorContrastViolations,
  toBeInViewport: expect.toBeInViewport,
};

// Export test utilities
export * from './test-utils';
export * from './test-ids';
