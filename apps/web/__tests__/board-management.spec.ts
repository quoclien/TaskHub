import { test, expect } from './test-utils';
import { faker } from '@faker-js/faker';

test.describe('Board Management', () => {
  test('should create a new board', async ({ authenticatedPage, testUser }) => {
    // Navigate to boards page
    await authenticatedPage.goto('/boards');
    
    // Click create board button
    await authenticatedPage.getByRole('button', { name: /create board/i }).click();
    
    // Fill in board details
    const boardTitle = `Test Board ${faker.lorem.words(2)}`;
    await authenticatedPage.getByLabel(/board title/i).fill(boardTitle);
    await authenticatedPage.getByLabel(/description/i).fill('Test board description');
    
    // Submit the form
    await authenticatedPage.getByRole('button', { name: /create board/i }).click();
    
    // Verify board was created and we're redirected to the board page
    await expect(authenticatedPage).toHaveURL(/\/board\/[^\/]+$/);
    await expect(authenticatedPage.getByRole('heading', { name: boardTitle })).toBeVisible();
  });

  test('should create a new list on a board', async ({ authenticatedPage }) => {
    // Create a test board first
    await authenticatedPage.goto('/boards');
    await authenticatedPage.getByRole('button', { name: /create board/i }).click();
    const boardTitle = `Test Board ${faker.lorem.words(2)}`;
    await authenticatedPage.getByLabel(/board title/i).fill(boardTitle);
    await authenticatedPage.getByRole('button', { name: /create board/i }).click();
    
    // Add a new list
    const listTitle = 'To Do';
    await authenticatedPage.getByPlaceholder('Enter list title...').fill(listTitle);
    await authenticatedPage.getByRole('button', { name: /add list/i }).click();
    
    // Verify list was added
    await expect(authenticatedPage.getByRole('heading', { level: 3, name: listTitle })).toBeVisible();
  });

  test('should create a new card in a list', async ({ authenticatedPage }) => {
    // Create a test board and list first
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
    await authenticatedPage.getByRole('button', { name: /add a card/i }).first().click();
    await authenticatedPage.getByPlaceholder('Enter a title for this card...').fill(cardTitle);
    await authenticatedPage.getByRole('button', { name: /add card/i }).click();
    
    // Verify card was added
    await expect(authenticatedPage.getByText(cardTitle)).toBeVisible();
  });

  test('should move a card between lists', async ({ authenticatedPage }) => {
    // Setup: Create a board with two lists and a card in the first list
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
    
    // Drag and drop the card to the second list
    const card = authenticatedPage.getByRole('button', { name: cardTitle });
    const targetList = authenticatedPage.getByRole('heading', { level: 3, name: 'In Progress' }).locator('..');
    
    await card.dragTo(targetList);
    
    // Verify the card is now in the second list
    await expect(targetList.getByText(cardTitle)).toBeVisible();
  });

  test('should edit a card', async ({ authenticatedPage }) => {
    // Setup: Create a board with a list and a card
    await authenticatedPage.goto('/boards');
    await authenticatedPage.getByRole('button', { name: /create board/i }).click();
    const boardTitle = `Test Board ${faker.lorem.words(2)}`;
    await authenticatedPage.getByLabel(/board title/i).fill(boardTitle);
    await authenticatedPage.getByRole('button', { name: /create board/i }).click();
    
    // Add a list and a card
    await authenticatedPage.getByPlaceholder('Enter list title...').fill('To Do');
    await authenticatedPage.getByRole('button', { name: /add list/i }).click();
    
    const cardTitle = 'Test Card';
    await authenticatedPage.getByRole('button', { name: /add a card/i }).click();
    await authenticatedPage.getByPlaceholder('Enter a title for this card...').fill(cardTitle);
    await authenticatedPage.getByRole('button', { name: /add card/i }).click();
    
    // Open the card for editing
    await authenticatedPage.getByText(cardTitle).click();
    
    // Edit the card title
    const newCardTitle = 'Updated Card Title';
    await authenticatedPage.getByRole('textbox', { name: /title/i }).fill(newCardTitle);
    await authenticatedPage.getByRole('button', { name: /save/i }).click();
    
    // Verify the card was updated
    await expect(authenticatedPage.getByText(newCardTitle)).toBeVisible();
  });
});
