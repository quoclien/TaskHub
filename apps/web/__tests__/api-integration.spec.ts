import { test, expect } from './test-utils';
import { faker } from '@faker-js/faker';

test.describe('API Integration & Error Handling', () => {
  test('should handle network errors when fetching boards', async ({ page }) => {
    // Mock network error for boards API
    await page.route('**/api/boards', (route) => {
      route.abort('failed');
    });

    await page.goto('/boards');
    
    // Verify error state is shown
    await expect(page.getByText(/failed to fetch boards/i)).toBeVisible();
  });

  test('should handle 401 unauthorized errors', async ({ page }) => {
    // Mock 401 response for boards API
    await page.route('**/api/boards', (route) => {
      route.fulfill({
        status: 401,
        contentType: 'application/json',
        body: JSON.stringify({ message: 'Unauthorized' }),
      });
    });

    await page.goto('/boards');
    
    // Verify user is redirected to sign-in page
    await expect(page).toHaveURL(/\/sign-in/);
    await expect(page.getByText(/please sign in/i)).toBeVisible();
  });

  test('should handle 404 errors when board is not found', async ({ authenticatedPage }) => {
    // Mock 404 response for a specific board
    await authenticatedPage.route('**/api/boards/non-existent-board', (route) => {
      route.fulfill({
        status: 404,
        contentType: 'application/json',
        body: JSON.stringify({ message: 'Board not found' }),
      });
    });

    await authenticatedPage.goto('/board/non-existent-board');
    
    // Verify 404 error state is shown
    await expect(authenticatedPage.getByText(/board not found/i)).toBeVisible();
  });

  test('should handle validation errors when creating a board', async ({ authenticatedPage }) => {
    await authenticatedPage.goto('/boards');
    
    // Mock validation error response
    await authenticatedPage.route('**/api/boards', (route) => {
      if (route.request().method() === 'POST') {
        route.fulfill({
          status: 400,
          contentType: 'application/json',
          body: JSON.stringify({
            message: 'Validation failed',
            errors: {
              title: ['Title is required'],
            },
          }),
        });
      } else {
        route.continue();
      }
    });

    // Open create board dialog
    await authenticatedPage.getByRole('button', { name: /create board/i }).click();
    
    // Submit form without required fields
    await authenticatedPage.getByRole('button', { name: /create board/i }).click();
    
    // Verify validation error is shown
    await expect(authenticatedPage.getByText(/title is required/i)).toBeVisible();
  });

  test('should handle optimistic updates when moving a card', async ({ authenticatedPage }) => {
    // Create a test board with two lists and a card
    await authenticatedPage.goto('/boards');
    await authenticatedPage.getByRole('button', { name: /create board/i }).click();
    const boardTitle = `Test Board ${faker.lorem.words(2)}`;
    await authenticatedPage.getByLabel(/board title/i).fill(boardTitle);
    await authenticatedPage.getByRole('button', { name: /create board/i }).click();
    
    // Add two lists
    await authenticatedPage.getByPlaceholder('Enter list title...').fill('To Do');
    await authenticatedPage.getByRole('button', { name: /add list/i }).click();
    await authenticatedPage.getByPlaceholder('Enter list title...').fill('In Progress');
    await authenticatedPage.getByRole('button', { name: /add list/i }).click();
    
    // Add a card to the first list
    const cardTitle = 'Test Card';
    await authenticatedPage.getByRole('button', { name: /add a card/i }).first().click();
    await authenticatedPage.getByPlaceholder('Enter a title for this card...').fill(cardTitle);
    await authenticatedPage.getByRole('button', { name: /add card/i }).click();
    
    // Mock the API response for moving the card
    await authenticatedPage.route('**/api/cards/*/move', (route) => {
      // Simulate network delay
      setTimeout(() => {
        route.fulfill({
          status: 200,
          contentType: 'application/json',
          body: JSON.stringify({ success: true }),
        });
      }, 1000);
    });
    
    // Get the card and target list
    const card = authenticatedPage.getByRole('button', { name: cardTitle });
    const targetList = authenticatedPage.getByRole('heading', { level: 3, name: 'In Progress' }).locator('..');
    
    // Drag and drop the card
    await card.dragTo(targetList);
    
    // Verify the card appears in the target list immediately (optimistic update)
    await expect(targetList.getByText(cardTitle)).toBeVisible();
    
    // Wait for the API call to complete
    await authenticatedPage.waitForResponse('**/api/cards/*/move');
    
    // Verify the card is still in the target list after API call
    await expect(targetList.getByText(cardTitle)).toBeVisible();
  });

  test('should handle API errors during optimistic updates', async ({ authenticatedPage }) => {
    // Create a test board with a list and a card
    await authenticatedPage.goto('/boards');
    await authenticatedPage.getByRole('button', { name: /create board/i }).click();
    const boardTitle = `Test Board ${faker.lorem.words(2)}`;
    await authenticatedPage.getByLabel(/board title/i).fill(boardTitle);
    await authenticatedPage.getByRole('button', { name: /create board/i }).click();
    
    // Add a list
    await authenticatedPage.getByPlaceholder('Enter list title...').fill('To Do');
    await authenticatedPage.getByRole('button', { name: /add list/i }).click();
    
    // Add a card to the list
    const cardTitle = 'Test Card';
    await authenticatedPage.getByRole('button', { name: /add a card/i }).click();
    await authenticatedPage.getByPlaceholder('Enter a title for this card...').fill(cardTitle);
    await authenticatedPage.getByRole('button', { name: /add card/i }).click();
    
    // Mock a failed API response for deleting the card
    await authenticatedPage.route('**/api/cards/*', (route) => {
      if (route.request().method() === 'DELETE') {
        route.fulfill({
          status: 500,
          contentType: 'application/json',
          body: JSON.stringify({ message: 'Internal server error' }),
        });
      } else {
        route.continue();
      }
    });
    
    // Open the card
    await authenticatedPage.getByText(cardTitle).click();
    
    // Click delete button
    await authenticatedPage.getByRole('button', { name: /delete/i }).click();
    await authenticatedPage.getByRole('button', { name: /confirm/i }).click();
    
    // Verify error message is shown
    await expect(authenticatedPage.getByText(/failed to delete card/i)).toBeVisible();
    
    // Verify the card is still visible (since the delete failed)
    await expect(authenticatedPage.getByText(cardTitle)).toBeVisible();
  });
});
