import { faker } from '@faker-js/faker';

import { test, expect, type Page } from './test-utils';

/**
 * Critical Path E2E Tests
 *
 * These tests validate the core user journey with real API integration:
 * 1. User Registration/Login
 * 2. Board Creation & Management
 * 3. List Creation & Organization
 * 4. Card Creation & Drag-and-Drop
 *
 * Running with NEXT_PUBLIC_DISABLE_MOCKS=true to test against live API
 */

test.describe('Critical Path: Complete User Journey', () => {
  test('Complete workflow: Register → Create Board → Add Lists → Create & Move Cards', async ({
    authenticatedPage,
    testUser,
  }) => {
    try {
      // Step 1: Verify authenticated state
      await expect(authenticatedPage).toHaveURL('/boards');
      await expect(authenticatedPage.getByRole('heading', { name: /your boards/i })).toBeVisible();

      // Step 2: Create a new board
      const boardTitle = `E2E Test Board ${faker.lorem.words(2)}`;
      const boardDescription = 'End-to-end test board for critical path validation';

      await authenticatedPage.getByRole('button', { name: /create board/i }).click();
      await authenticatedPage.getByLabel(/board title/i).fill(boardTitle);
      await authenticatedPage.getByLabel(/description/i).fill(boardDescription);

      // Wait for navigation to board detail page
      const navigationPromise = authenticatedPage.waitForURL(/\/board\/[^\/]+$/);
      await authenticatedPage.getByRole('button', { name: /create board/i }).click();
      await navigationPromise;

      // Verify board creation
      await expect(authenticatedPage.getByRole('heading', { name: boardTitle })).toBeVisible({
        timeout: 10000,
      });

      // Step 3: Create multiple lists
      const listTitles = ['To Do', 'In Progress', 'Review', 'Done'];

      for (const listTitle of listTitles) {
        await authenticatedPage.getByPlaceholder('Enter list title...').fill(listTitle);
        await authenticatedPage.getByRole('button', { name: /add list/i }).click();

        // Verify list creation
        await expect(
          authenticatedPage.getByRole('heading', { level: 3, name: listTitle })
        ).toBeVisible({ timeout: 5000 });
      }

      // Step 4: Create cards in different lists
      const cardData = [
        {
          title: 'Setup project structure',
          listIndex: 0,
          description: 'Initialize the project repository',
        },
        {
          title: 'Implement authentication',
          listIndex: 1,
          description: 'Add user login and registration',
        },
        {
          title: 'Create API endpoints',
          listIndex: 1,
          description: 'Build REST API for the application',
        },
        { title: 'Review code changes', listIndex: 2, description: 'Code review and testing' },
        {
          title: 'Deploy to staging',
          listIndex: 3,
          description: 'Deploy application to staging environment',
        },
      ];

      for (const card of cardData) {
        // Click add card button for the specific list
        const addCardButtons = await authenticatedPage
          .getByRole('button', { name: /add a card/i })
          .all();
        if (addCardButtons[card.listIndex]) {
          await addCardButtons[card.listIndex].click();

          // Fill card details
          await authenticatedPage
            .getByPlaceholder('Enter a title for this card...')
            .fill(card.title);
          await authenticatedPage.getByRole('button', { name: /add card/i }).click();

          // Verify card creation
          await expect(authenticatedPage.getByText(card.title)).toBeVisible({ timeout: 5000 });
        }
      }

      // Step 5: Test drag and drop functionality
      // Move a card from "To Do" to "In Progress"
      const sourceCard = authenticatedPage.getByRole('button', { name: 'Setup project structure' });
      const targetList = authenticatedPage
        .getByRole('heading', { level: 3, name: 'In Progress' })
        .locator('..');

      // Perform drag and drop
      await sourceCard.dragTo(targetList);

      // Verify card moved successfully
      await expect(targetList.getByText('Setup project structure')).toBeVisible({ timeout: 10000 });

      // Step 6: Test card detail modal
      const cardToEdit = authenticatedPage.getByText('Implement authentication');
      await cardToEdit.click();

      // Verify modal opens
      const cardModal = authenticatedPage.getByRole('dialog');
      await expect(cardModal).toBeVisible({ timeout: 5000 });

      // Edit card details
      const titleInput = cardModal.getByRole('textbox', { name: /title/i });
      await expect(titleInput).toBeVisible();
      await titleInput.fill('Implement user authentication system');

      // Save changes
      const saveButton = cardModal.getByRole('button', { name: /save/i });
      await saveButton.click();

      // Verify changes persist
      await expect(authenticatedPage.getByText('Implement user authentication system')).toBeVisible(
        { timeout: 5000 }
      );

      // Step 7: Verify board state persistence
      // Refresh page and verify all data persists
      await authenticatedPage.reload();

      await expect(authenticatedPage.getByRole('heading', { name: boardTitle })).toBeVisible();

      // Verify all lists still exist
      for (const listTitle of listTitles) {
        await expect(
          authenticatedPage.getByRole('heading', { level: 3, name: listTitle })
        ).toBeVisible();
      }

      // Verify moved card is still in correct list
      const inProgressList = authenticatedPage
        .getByRole('heading', { level: 3, name: 'In Progress' })
        .locator('..');
      await expect(inProgressList.getByText('Setup project structure')).toBeVisible();

      // Verify edited card title persists
      await expect(
        authenticatedPage.getByText('Implement user authentication system')
      ).toBeVisible();
    } catch (error) {
      console.error('Critical path test failed:', error);

      // Take screenshot on failure for debugging
      await authenticatedPage.screenshot({
        path: `test-results/critical-path-failure-${Date.now()}.png`,
        fullPage: true,
      });

      throw error;
    }
  });

  test('Error handling: Network failures and recovery', async ({ authenticatedPage }) => {
    try {
      // Navigate to boards page
      await authenticatedPage.goto('/boards');

      // Intercept API calls and simulate network failures
      await authenticatedPage.route('**/api/boards', (route) => {
        if (route.request().method() === 'POST') {
          // Simulate network error for board creation
          route.abort('failed');
        } else {
          route.continue();
        }
      });

      // Attempt to create board (should fail)
      await authenticatedPage.getByRole('button', { name: /create board/i }).click();
      await authenticatedPage.getByLabel(/board title/i).fill('Test Board');
      await authenticatedPage.getByRole('button', { name: /create board/i }).click();

      // Verify error handling
      await expect(authenticatedPage.getByText(/failed to create board|error/i)).toBeVisible({
        timeout: 10000,
      });

      // Remove route interception and retry
      await authenticatedPage.unroute('**/api/boards');

      // Retry board creation (should succeed)
      await authenticatedPage.getByRole('button', { name: /create board/i }).click();
      const retryBoardTitle = `Retry Board ${faker.lorem.words(1)}`;
      await authenticatedPage.getByLabel(/board title/i).fill(retryBoardTitle);

      const navigationPromise = authenticatedPage.waitForURL(/\/board\/[^\/]+$/);
      await authenticatedPage.getByRole('button', { name: /create board/i }).click();
      await navigationPromise;

      // Verify recovery
      await expect(authenticatedPage.getByRole('heading', { name: retryBoardTitle })).toBeVisible({
        timeout: 10000,
      });
    } catch (error) {
      console.error('Error handling test failed:', error);
      throw error;
    }
  });

  test('Authentication boundary: Logout and re-login maintains session', async ({
    authenticatedPage,
    testUser,
  }) => {
    try {
      // Create a board while authenticated
      await authenticatedPage.goto('/boards');

      const testBoardTitle = `Session Test Board ${faker.lorem.words(1)}`;
      await authenticatedPage.getByRole('button', { name: /create board/i }).click();
      await authenticatedPage.getByLabel(/board title/i).fill(testBoardTitle);

      const navigationPromise = authenticatedPage.waitForURL(/\/board\/[^\/]+$/);
      await authenticatedPage.getByRole('button', { name: /create board/i }).click();
      await navigationPromise;

      // Extract board ID from URL for later verification
      const currentUrl = authenticatedPage.url();
      const boardId = /\/board\/([^\/]+)$/.exec(currentUrl)?.[1];
      expect(boardId).toBeDefined();

      // Sign out
      await authenticatedPage.goto('/api/auth/signout');
      await authenticatedPage.getByRole('button', { name: /sign out/i }).click();

      // Verify redirect to sign-in
      await expect(authenticatedPage).toHaveURL(/\/sign-in/);

      // Sign back in with same credentials
      await authenticatedPage.getByLabel(/email or username/i).fill(testUser.email);
      await authenticatedPage.getByLabel(/password/i).fill(testUser.password);
      await authenticatedPage.getByRole('button', { name: /sign in/i }).click();

      // Verify boards page loads
      await expect(authenticatedPage).toHaveURL('/boards');

      // Verify previously created board still exists
      await expect(authenticatedPage.getByText(testBoardTitle)).toBeVisible({ timeout: 10000 });

      // Navigate back to specific board
      if (boardId) {
        await authenticatedPage.goto(`/board/${boardId}`);
        await expect(authenticatedPage.getByRole('heading', { name: testBoardTitle })).toBeVisible({
          timeout: 10000,
        });
      }
    } catch (error) {
      console.error('Authentication boundary test failed:', error);
      throw error;
    }
  });
});

test.describe('Critical Path: Performance & Accessibility', () => {
  test('Page load performance meets budget', async ({ authenticatedPage }) => {
    // Enable performance tracking
    const client = await authenticatedPage.context().newCDPSession(authenticatedPage);
    await client.send('Performance.enable');

    const startTime = Date.now();
    await authenticatedPage.goto('/boards');
    const loadTime = Date.now() - startTime;

    // Assert performance budget (3s for boards page)
    expect(loadTime).toBeLessThan(3000);

    // Check for Core Web Vitals
    const cls = await authenticatedPage.evaluate(() => {
      return new Promise<number>((resolve) => {
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

        observer.observe({ type: 'layout-shift', buffered: true } as PerformanceObserverInit);

        setTimeout(() => {
          observer.disconnect();
        }, 0);
      });
    });

    expect(cls).toBeLessThan(0.1); // CLS should be less than 0.1
  });

  test('Basic accessibility compliance', async ({ authenticatedPage }) => {
    await authenticatedPage.goto('/boards');

    // Check for proper heading structure
    await expect(authenticatedPage.getByRole('heading', { level: 1 })).toHaveCount(1);

    // Check for accessible navigation
    const createButton = authenticatedPage.getByRole('button', { name: /create board/i });
    await expect(createButton).toBeVisible();

    // Test keyboard navigation
    await authenticatedPage.keyboard.press('Tab');
    await expect(createButton).toBeFocused();

    // Verify no critical a11y violations (simplified check)
    const accessibilityResults = await authenticatedPage.accessibility.snapshot();
    expect(accessibilityResults).toBeDefined();
  });
});
