import { test, expect } from '@playwright/experimental-ct-react';
import type { Page } from '@playwright/test';
import { BoardCard } from '@/components/board/BoardCard';
import { ListComponent } from '@/components/board/ListComponent';
import type { Card, List } from '@taskhub/shared';

// Extend the test context with our custom fixtures
type ComponentFixtures = {
  mount: (component: React.ReactElement) => Promise<Page>;
};

// Helper function to create a test card
const createTestCard = (overrides: Partial<Card> = {}): Card => ({
  id: '1',
  title: 'Test Card',
  description: 'This is a test card',
  listId: 'list-1',
  position: 0,
  createdAt: new Date().toISOString(),
  updatedAt: new Date().toISOString(),
  ...overrides,
});

// Helper function to create a test list
const createTestList = (overrides: Partial<List> = {}): List => ({
  id: 'list-1',
  title: 'Test List',
  boardId: 'board-1',
  position: 0,
  cards: [],
  ...overrides,
});

test.describe('Component Behaviors', () => {
  test('BoardCard should display card title and description', async ({ mount }: { mount: ComponentFixtures['mount'] }) => {
    const testCard = createTestCard({
      title: 'Test Card',
      description: 'This is a test card',
    });

    const component = await mount(
      <BoardCard 
        card={testCard} 
        onEdit={() => {}} 
        isDragging={false}
      />
    );

    await expect(component.getByText(testCard.title)).toBeVisible();
    await expect(component.getByText(testCard.description!)).toBeVisible();
  });

  test('ListComponent should display list title and cards', async ({ mount }: { mount: ComponentFixtures['mount'] }) => {
    const testList = createTestList({
      cards: [
        createTestCard({
          id: 'card-1',
          title: 'Card 1',
          description: 'Description 1',
          position: 0,
        }),
        createTestCard({
          id: 'card-2',
          title: 'Card 2',
          description: 'Description 2',
          position: 1,
        }),
      ],
    });

    const component = await mount(
      <ListComponent
        list={testList}
        onAddCard={() => {}}
        onEditCard={() => {}}
        onCardClick={() => {}}
        onCardDragEnd={() => {}}
        isOver={false}
      />
    );

    await expect(component.getByText(testList.title)).toBeVisible();
    await expect(component.getByText('Card 1')).toBeVisible();
    await expect(component.getByText('Card 2')).toBeVisible();
  });

  test('ListComponent should show add card form when add button is clicked', async ({ mount }: { mount: ComponentFixtures['mount'] }) => {
    const testList = createTestList({ cards: [] });
    let addCardClicked = false;
    
    const component = await mount(
      <ListComponent
        list={testList}
        onAddCard={() => { addCardClicked = true; }}
        onEditCard={() => {}}
        onCardClick={() => {}}
        onCardDragEnd={() => {}}
        isOver={false}
      />
    );

    // Click the add card button
    await component.getByRole('button', { name: /add a card/i }).click();
    
    // Verify the form is visible
    await expect(component.getByPlaceholder('Enter a title for this card...')).toBeVisible();
    
    // Enter a card title and submit
    await component.getByPlaceholder('Enter a title for this card...').fill('New Card');
    await component.getByRole('button', { name: /add card/i }).click();
    
    // Verify the callback was called
    expect(addCardClicked).toBe(true);
  });

  test('BoardCard should call onEdit when clicked', async ({ mount }: { mount: ComponentFixtures['mount'] }) => {
    const testCard = createTestCard();
    let editClicked = false;

    const component = await mount(
      <BoardCard 
        card={testCard} 
        onEdit={() => { editClicked = true; }} 
        isDragging={false}
      />
    );

    try {
      // Click the card
      await component.click({
        position: { x: 10, y: 10 },
        timeout: 5000,
      });
      
      // Verify the callback was called
      expect(editClicked, 'onEdit callback should be called when card is clicked').toBe(true);
    } catch (error) {
      console.error('Error in BoardCard click test:', error);
      throw error;
    }
  });
});
