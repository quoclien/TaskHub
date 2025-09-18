import '@testing-library/jest-dom';

declare global {
  namespace jest {
    interface Matchers<R> {
      toBeInTheDocument(): R;
      toHaveAttribute(attr: string, value?: string): R;
      toHaveFocus(): R;
      toHaveClass(className: string): R;
      toHaveStyle(css: string | Record<string, any>): R;
      toHaveValue(value: string | string[] | number): R;
      toHaveDisplayValue(value: string | string[]): R;
      toBeChecked(): R;
      toBeDisabled(): R;
      toBeEmpty(): R;
      toBeEmptyDOMElement(): R;
      toBePartiallyChecked(): R;
      toBeRequired(): R;
      toBeValid(): R;
      toBeVisible(): R;
      toContainElement(element: HTMLElement | null): R;
      toContainHTML(htmlText: string): R;
      toHaveAccessibleDescription(expectedAccessibleDescription?: string | RegExp): R;
      toHaveAccessibleName(expectedAccessibleName?: string | RegExp): R;
      toHaveErrorMessage(expectedErrorMessage?: string | RegExp): R;
      toHaveFormValues(expectedValues: Record<string, any>): R;
      toHaveTextContent(text: string | RegExp, options?: { normalizeWhitespace?: boolean }): R;
      toHaveValue(value: string | string[] | number): R;
    }
  }
}

export {};
