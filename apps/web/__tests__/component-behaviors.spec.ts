import { test, expect } from '@playwright/experimental-ct-react';
import { BoardCard } from '@/components/board/BoardCard';
import { ListComponent } from '@/components/board/ListComponent';
import { Card } from '@taskhub/shared';

test.describe('Component Behaviors', () => {
  test('BoardCard should display card title and description', async ({ mount }) => {
    const testCard: Card = {
      id: '1',
      title: 'Test Card',
      description: 'This is a test card',
      listId: 'list-1',
      position: 0,
      createdAt: new Date().toISOString(),
      updatedAt: new Date().toISOString(),
    };

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

  test('ListComponent should display list title and cards', async ({ mount }) => {
    const testList = {
      id: 'list-1',
      title: 'Test List',
      boardId: 'board-1',
      position: 0,
      cards: [
        {
          id: 'card-1',
          title: 'Card 1',
          description: 'Description 1',
          listId: 'list-1',
          position: 0,
          createdAt: new Date().toISOString(),
          updatedAt: new Date().toISOString(),
        },
        {
          id: 'card-2',
          title: 'Card 2',
          description: 'Description 2',
          listId: 'list-1',
          position: 1,
          createdAt: new Date().toISOString(),
          updatedAt: new Date().toISOString(),
        },
      ],
    };

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

  test('ListComponent should show add card form when add button is clicked', async ({ mount }) => {
    const testList = {
      id: 'list-1',
      title: 'Test List',
      boardId: 'board-1',
      position: 0,
      cards: [],
    };

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

  test('BoardCard should call onEdit when clicked', async ({ mount }) => {
    const testCard: Card = {
      id: '1',
      title: 'Test Card',
      description: 'This is a test card',
      listId: 'list-1',
      position: 0,
      createdAt: new Date().toISOString(),
      updatedAt: new Date().toISOString(),
    };

    let editClicked = false;

    const component = await mount(
      <BoardCard 
        card={testCard} 
        onEdit={() => { editClicked = true; }} 
        isDragging={false}
      />
    );

    // Click the card
    await component.click();
    
    // Verify the callback was called
    expect(editClicked).toBe(true);
  });
});
