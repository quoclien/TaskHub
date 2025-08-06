# TaskHub Frontend Tests

This directory contains end-to-end (E2E) and component tests for the TaskHub frontend application.

## Test Structure

```
__tests__/
├── auth.spec.ts              # Authentication flow tests
├── board-management.spec.ts  # Board and card management tests
├── component-behaviors.spec.ts # Component-level behavior tests
├── api-integration.spec.ts   # API integration and error handling tests
├── accessibility-performance.spec.ts # Accessibility and performance tests
├── test-utils/               # Test utilities and helpers
│   ├── matchers.ts           # Custom Playwright matchers
│   ├── test-ids.ts           # Centralized test IDs
│   └── index.ts              # Re-exports
└── fixtures/                 # Test fixtures and mocks
```

## Running Tests

### Prerequisites

- Node.js 18+
- pnpm
- Playwright browsers installed

### Installation

```bash
# Install dependencies
pnpm install

# Install Playwright browsers
npx playwright install
```

### Running Tests

```bash
# Run all tests
pnpm test:e2e

# Run tests in UI mode
pnpm test:e2e:ui

# Run a specific test file
pnpm test:e2e tests/auth.spec.ts

# Run tests in debug mode
PWDEBUG=1 pnpm test:e2e

# Run tests on specific browsers
pnpm test:e2e --project=chromium
pnpm test:e2e --project=firefox
pnpm test:e2e --project=webkit
```

### Test Scripts

- `test:e2e`: Run all E2E tests in headless mode
- `test:e2e:ui`: Run tests in UI mode
- `test:e2e:debug`: Run tests in debug mode
- `test:component`: Run component tests
- `test:coverage`: Generate test coverage report
- `test:update-snapshots`: Update test snapshots

## Writing Tests

### Test Organization

- Group related tests in `describe` blocks
- Use `test` or `it` for individual test cases
- Follow the Arrange-Act-Assert pattern
- Use page object models for complex components

### Best Practices

1. **Test IDs**: Use `data-testid` attributes for reliable element selection
2. **Selectors**: Prefer role-based selectors over CSS selectors
3. **Assertions**: Use descriptive assertion messages
4. **Mocks**: Mock external dependencies consistently
5. **Cleanup**: Ensure proper cleanup after each test

### Example Test

```typescript
test('should create a new board', async ({ page }) => {
  // Arrange
  await page.goto('/boards');
  
  // Act
  await page.getByTestId('create-board-button').click();
  await page.getByLabel('Board Title').fill('My New Board');
  await page.getByRole('button', { name: 'Create' }).click();
  
  // Assert
  await expect(page.getByText('My New Board')).toBeVisible();
});
```

## Debugging Tests

### Playwright Inspector

Run tests with the Playwright Inspector for step-by-step debugging:

```bash
PWDEBUG=1 pnpm test:e2e
```

### Screenshots and Videos

Test failures automatically generate:
- Screenshots in `test-results/`
- Videos in `test-results/videos/`
- Trace files in `test-results/trace/`

### Console Logs

Access browser console logs with:

```typescript
page.on('console', msg => console.log('BROWSER LOG:', msg.text()));
```

## Continuous Integration

Tests run on every push and pull request. The CI pipeline includes:

1. Linting
2. Type checking
3. Unit tests
4. E2E tests
5. Accessibility audits
6. Performance budgets

## Test Coverage

Generate a coverage report with:

```bash
pnpm test:coverage
```

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.
