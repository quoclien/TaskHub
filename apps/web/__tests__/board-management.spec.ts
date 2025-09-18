import { faker } from '@faker-js/faker';
import { test, expect, type Page } from '@playwright/test';

import type { TestUser } from './test-utils';

// Define test fixtures type
interface BoardManagementFixtures {
  authenticatedPage: Page;
  testUser: TestUser;
}

// Extend the test context with our fixtures
test.extend<BoardManagementFixtures>();

// Helper function to create a test board
const createTestBoard = async (page: Page, title: string): Promise<string> => {
  await page.getByRole('button', { name: /create board/i }).click();
  await page.getByLabel(/board title/i).fill(title);
  await page.getByRole('button', { name: /create board/i }).click();
  await expect(page).toHaveURL(/\/board\/[^\/]+$/);
  return title;
};

// Helper function to create a test list
const createTestList = async (page: Page, title: string): Promise<void> => {
  await page.getByPlaceholder('Enter list title...').fill(title);
  await page.getByRole('button', { name: /add list/i }).click();
  await expect(page.getByRole('heading', { level: 3, name: title })).toBeVisible();
};

// Helper function to create a test card
const createTestCard = async (page: Page, title: string, listIndex = 0): Promise<void> => {
  const addCardButtons = await page.getByRole('button', { name: /add a card/i }).all();
  await addCardButtons[listIndex]?.click();
  await page.getByPlaceholder('Enter a title for this card...').fill(title);
  await page.getByRole('button', { name: /add card/i }).click();
  await expect(page.getByText(title)).toBeVisible();
};

test.describe('Board Management', () => {
  test('should create a new board', async ({ authenticatedPage, testUser }) => {
    try {
      // Navigate to boards page
      await authenticatedPage.goto('/boards');

      // Generate test data
      const boardTitle = `Test Board ${faker.lorem.words(2)}`;
      const boardDescription = 'Test board description';

      // Fill in board details
      await authenticatedPage.getByRole('button', { name: /create board/i }).click();
      await authenticatedPage.getByLabel(/board title/i).fill(boardTitle);
      await authenticatedPage.getByLabel(/description/i).fill(boardDescription);

      // Submit the form and wait for navigation
      const navigationPromise = authenticatedPage.waitForURL(/\/board\/[^\/]+$/);
      await authenticatedPage.getByRole('button', { name: /create board/i }).click();
      await navigationPromise;

      // Verify board was created and we're redirected to the board page
      await expect(authenticatedPage, 'Should be redirected to the new board page').toHaveURL(
        /\/board\/[^\/]+$/
      );

      await expect(
        authenticatedPage.getByRole('heading', { name: boardTitle }),
        'Board title should be visible on the page'
      ).toBeVisible({
        timeout: 5000,
      });
    } catch (error) {
      console.error('Board creation test failed:', error);
      throw error;
    }
  });

  test('should create a new list on a board', async ({ authenticatedPage }) => {
    try {
      // Navigate to boards page and create a test board
      await authenticatedPage.goto('/boards');
      const boardTitle = `Test Board ${faker.lorem.words(2)}`;
      await createTestBoard(authenticatedPage, boardTitle);

      // Add a new list
      const listTitle = 'To Do';
      await createTestList(authenticatedPage, listTitle);

      // Verify list was added
      await expect(
        authenticatedPage.getByRole('heading', { level: 3, name: listTitle }),
        'List should be visible on the board'
      ).toBeVisible({
        timeout: 5000,
      });
    } catch (error) {
      console.error('List creation test failed:', error);
      throw error;
    }
  });

  test('should create a new card in a list', async ({ authenticatedPage }) => {
    try {
      // Setup: Create a board and a list
      await authenticatedPage.goto('/boards');
      const boardTitle = `Test Board ${faker.lorem.words(2)}`;
      await createTestBoard(authenticatedPage, boardTitle);
      await createTestList(authenticatedPage, 'To Do');

      // Add a card to the list
      const cardTitle = 'Test Card';
      await createTestCard(authenticatedPage, cardTitle);

      // Verify the card was added to the list
      await expect(
        authenticatedPage.getByText(cardTitle, { exact: true }),
        'Card should be visible in the list'
      ).toBeVisible({
        timeout: 5000,
      });
    } catch (error) {
      console.error('Card creation test failed:', error);
      throw error;
    }
  });

  test('should move a card between lists', async ({ authenticatedPage }) => {
    try {
      // Setup: Create a board with two lists and a card in the first list
      await authenticatedPage.goto('/boards');
      const boardTitle = `Test Board ${faker.lorem.words(2)}`;
      await createTestBoard(authenticatedPage, boardTitle);

      // Add two lists
      await createTestList(authenticatedPage, 'To Do');
      await createTestList(authenticatedPage, 'In Progress');

      // Add a card to the first list
      const cardTitle = 'Test Card';
      await createTestCard(authenticatedPage, cardTitle, 0);

      // Get references to the card and target list
      const card = authenticatedPage.getByRole('button', { name: cardTitle });
      const targetList = authenticatedPage
        .getByRole('heading', { level: 3, name: 'In Progress' })
        .locator('..');

      // Verify the card is initially in the first list
      await expect(
        authenticatedPage
          .getByRole('heading', { level: 3, name: 'To Do' })
          .locator('..')
          .getByText(cardTitle),
        'Card should be in the first list initially'
      ).toBeVisible();

      // Drag and drop the card to the second list
      await card.dragTo(targetList);

      // Verify the card is now in the second list
      await expect(
        targetList.getByText(cardTitle, { exact: true }),
        'Card should be in the second list after drag and drop'
      ).toBeVisible({
        timeout: 5000,
      });
    } catch (error) {
      console.error('Card movement test failed:', error);
      throw error;
    }
  });

  test('should edit a card', async ({ authenticatedPage }) => {
    try {
      // Setup: Create a board with a list and a card
      await authenticatedPage.goto('/boards');
      const boardTitle = `Test Board ${faker.lorem.words(2)}`;
      await createTestBoard(authenticatedPage, boardTitle);

      // Add a list and a card
      await createTestList(authenticatedPage, 'To Do');

      const cardTitle = 'Test Card';
      await createTestCard(authenticatedPage, cardTitle);

      // Open the card for editing
      const cardElement = authenticatedPage.getByText(cardTitle, { exact: true });
      await cardElement.click();

      // Wait for the card details modal to be visible
      const cardModal = authenticatedPage.getByRole('dialog');
      await expect(cardModal, 'Card details modal should be visible').toBeVisible();

      // Edit the card title
      const newCardTitle = 'Updated Card Title';
      const titleInput = cardModal.getByRole('textbox', { name: /title/i });
      await expect(titleInput, 'Title input should be visible').toBeVisible();
      await titleInput.fill(newCardTitle);

      // Save the changes
      const saveButton = cardModal.getByRole('button', { name: /save/i });
      await saveButton.click();

      // Verify the card was updated
      await expect(
        authenticatedPage.getByText(newCardTitle, { exact: true }),
        'Updated card title should be visible'
      ).toBeVisible({
        timeout: 5000,
      });
    } catch (error) {
      console.error('Card editing test failed:', error);
      throw error;
    }
  });
});
