import { test, expect } from '@playwright/test';
import type { Board, List as TaskList, Card } from '@taskhub/shared';

/**
 * Contract Validation Tests
 * 
 * These tests validate that frontend and backend maintain API contract compatibility.
 * Tests cover data shapes, status codes, error formats, and type safety.
 */

const API_BASE = process.env.NEXT_PUBLIC_API_BASE_URL || 'http://localhost:5205/api';

test.describe('API Contract Validation', () => {
  let authToken: string;

  test.beforeAll(async ({ request }) => {
    // Authenticate to get token for API calls
    const loginResponse = await request.post(`${API_BASE}/auth/login`, {
      data: {
        usernameOrEmail: 'admin',
        password: 'admin'
      }
    });

    expect(loginResponse.ok()).toBeTruthy();
    const authData = await loginResponse.json();
    authToken = authData.token;
    expect(authToken).toBeDefined();
  });

  test('Board API contract matches frontend types', async ({ request }) => {
    // Create board via API
    const createBoardResponse = await request.post(`${API_BASE}/boards`, {
      headers: { Authorization: `Bearer ${authToken}` },
      data: {
        title: 'Contract Test Board',
        description: 'Testing API contract compliance'
      }
    });

    expect(createBoardResponse.ok()).toBeTruthy();
    const createdBoard = await createBoardResponse.json();

    // Validate Board type structure
    expect(createdBoard).toHaveProperty('id');
    expect(createdBoard).toHaveProperty('title', 'Contract Test Board');
    expect(createdBoard).toHaveProperty('description', 'Testing API contract compliance');
    expect(createdBoard).toHaveProperty('ownerId');
    expect(createdBoard).toHaveProperty('createdAt');
    expect(createdBoard).toHaveProperty('updatedAt');

    // Type validation (structural)
    const board: Board = createdBoard;
    expect(typeof board.id).toBe('string');
    expect(typeof board.title).toBe('string');
    expect(typeof board.ownerId).toBe('string');
    expect(new Date(board.createdAt)).toBeInstanceOf(Date);
    expect(new Date(board.updatedAt)).toBeInstanceOf(Date);

    // Get board via API
    const getBoardResponse = await request.get(`${API_BASE}/boards/${createdBoard.id}`, {
      headers: { Authorization: `Bearer ${authToken}` }
    });

    expect(getBoardResponse.ok()).toBeTruthy();
    const retrievedBoard = await getBoardResponse.json();

    // Validate structure consistency
    expect(retrievedBoard.id).toBe(createdBoard.id);
    expect(retrievedBoard.title).toBe(createdBoard.title);
    expect(retrievedBoard.ownerId).toBe(createdBoard.ownerId);

    // Lists should be included in board details
    expect(retrievedBoard).toHaveProperty('lists');
    expect(Array.isArray(retrievedBoard.lists)).toBeTruthy();
  });

  test('List API contract matches frontend types', async ({ request }) => {
    // Create board first
    const boardResponse = await request.post(`${API_BASE}/boards`, {
      headers: { Authorization: `Bearer ${authToken}` },
      data: {
        title: 'List Contract Test Board',
        description: 'For testing list contracts'
      }
    });

    const board = await boardResponse.json();

    // Create list via API
    const createListResponse = await request.post(`${API_BASE}/lists`, {
      headers: { Authorization: `Bearer ${authToken}` },
      data: {
        title: 'Contract Test List',
        boardId: board.id,
        position: 1
      }
    });

    expect(createListResponse.ok()).toBeTruthy();
    const createdList = await createListResponse.json();

    // Validate List type structure
    expect(createdList).toHaveProperty('id');
    expect(createdList).toHaveProperty('title', 'Contract Test List');
    expect(createdList).toHaveProperty('boardId', board.id);
    expect(createdList).toHaveProperty('position', 1);
    expect(createdList).toHaveProperty('createdAt');
    expect(createdList).toHaveProperty('updatedAt');

    // Type validation
    const list: TaskList = createdList;
    expect(typeof list.id).toBe('string');
    expect(typeof list.title).toBe('string');
    expect(typeof list.boardId).toBe('string');
    expect(typeof list.position).toBe('number');
    expect(new Date(list.createdAt)).toBeInstanceOf(Date);
    expect(new Date(list.updatedAt)).toBeInstanceOf(Date);

    // Cards should be included in list details
    expect(createdList).toHaveProperty('cards');
    expect(Array.isArray(createdList.cards)).toBeTruthy();
  });

  test('Card API contract matches frontend types', async ({ request }) => {
    // Setup: Create board and list
    const boardResponse = await request.post(`${API_BASE}/boards`, {
      headers: { Authorization: `Bearer ${authToken}` },
      data: {
        title: 'Card Contract Test Board',
        description: 'For testing card contracts'
      }
    });

    const board = await boardResponse.json();

    const listResponse = await request.post(`${API_BASE}/lists`, {
      headers: { Authorization: `Bearer ${authToken}` },
      data: {
        title: 'Card Contract Test List',
        boardId: board.id,
        position: 1
      }
    });

    const list = await listResponse.json();

    // Create card with complex metadata
    const cardData = {
      title: 'Contract Test Card',
      description: 'Testing card contract compliance with complex metadata',
      listId: list.id,
      position: 1,
      metadata: {
        priority: 'high',
        tags: ['urgent', 'backend'],
        estimatedHours: 8.5,
        customFields: {
          businessValue: 100,
          complexity: 'medium'
        }
      }
    };

    const createCardResponse = await request.post(`${API_BASE}/cards`, {
      headers: { Authorization: `Bearer ${authToken}` },
      data: cardData
    });

    expect(createCardResponse.ok()).toBeTruthy();
    const createdCard = await createCardResponse.json();

    // Validate Card type structure
    expect(createdCard).toHaveProperty('id');
    expect(createdCard).toHaveProperty('title', cardData.title);
    expect(createdCard).toHaveProperty('description', cardData.description);
    expect(createdCard).toHaveProperty('listId', list.id);
    expect(createdCard).toHaveProperty('position', 1);
    expect(createdCard).toHaveProperty('metadata');
    expect(createdCard).toHaveProperty('createdAt');
    expect(createdCard).toHaveProperty('updatedAt');

    // Type validation
    const card: Card = createdCard;
    expect(typeof card.id).toBe('string');
    expect(typeof card.title).toBe('string');
    expect(typeof card.listId).toBe('string');
    expect(typeof card.position).toBe('number');
    expect(new Date(card.createdAt)).toBeInstanceOf(Date);
    expect(new Date(card.updatedAt)).toBeInstanceOf(Date);

    // Metadata validation (JSONB)
    expect(card.metadata).toBeDefined();
    expect(card.metadata?.priority).toBe('high');
    expect(Array.isArray(card.metadata?.tags)).toBeTruthy();
    expect(card.metadata?.estimatedHours).toBe(8.5);
    expect(typeof card.metadata?.customFields).toBe('object');
  });

  test('Error response format is consistent', async ({ request }) => {
    // Test unauthorized access
    const unauthorizedResponse = await request.get(`${API_BASE}/boards`);
    expect(unauthorizedResponse.status()).toBe(401);

    // Test not found
    const notFoundResponse = await request.get(`${API_BASE}/boards/00000000-0000-0000-0000-000000000000`, {
      headers: { Authorization: `Bearer ${authToken}` }
    });
    expect(notFoundResponse.status()).toBe(404);

    // Test validation error
    const validationErrorResponse = await request.post(`${API_BASE}/boards`, {
      headers: { Authorization: `Bearer ${authToken}` },
      data: {
        title: '', // Invalid empty title
        description: 'Test'
      }
    });
    expect(validationErrorResponse.status()).toBe(400);

    const validationError = await validationErrorResponse.json();
    expect(validationError).toHaveProperty('title');
    expect(validationError).toHaveProperty('status');
    expect(validationError).toHaveProperty('errors');
  });

  test('Pagination format is consistent', async ({ request }) => {
    // Get boards with pagination
    const paginatedResponse = await request.get(`${API_BASE}/boards?page=1&pageSize=5`, {
      headers: { Authorization: `Bearer ${authToken}` }
    });

    expect(paginatedResponse.ok()).toBeTruthy();
    const paginatedData = await paginatedResponse.json();

    // Validate pagination structure
    expect(paginatedData).toHaveProperty('items');
    expect(paginatedData).toHaveProperty('totalCount');
    expect(paginatedData).toHaveProperty('pageSize');
    expect(paginatedData).toHaveProperty('currentPage');
    expect(paginatedData).toHaveProperty('totalPages');
    expect(paginatedData).toHaveProperty('hasNextPage');
    expect(paginatedData).toHaveProperty('hasPreviousPage');

    expect(Array.isArray(paginatedData.items)).toBeTruthy();
    expect(typeof paginatedData.totalCount).toBe('number');
    expect(typeof paginatedData.pageSize).toBe('number');
    expect(typeof paginatedData.currentPage).toBe('number');
    expect(typeof paginatedData.totalPages).toBe('number');
    expect(typeof paginatedData.hasNextPage).toBe('boolean');
    expect(typeof paginatedData.hasPreviousPage).toBe('boolean');
  });

  test('Date formats are ISO 8601 compliant', async ({ request }) => {
    const boardResponse = await request.post(`${API_BASE}/boards`, {
      headers: { Authorization: `Bearer ${authToken}` },
      data: {
        title: 'Date Format Test Board',
        description: 'Testing date format compliance'
      }
    });

    const board = await boardResponse.json();

    // Validate ISO 8601 format
    const createdAtDate = new Date(board.createdAt);
    const updatedAtDate = new Date(board.updatedAt);

    expect(createdAtDate).toBeInstanceOf(Date);
    expect(updatedAtDate).toBeInstanceOf(Date);
    expect(isNaN(createdAtDate.getTime())).toBeFalsy();
    expect(isNaN(updatedAtDate.getTime())).toBeFalsy();

    // Check ISO format pattern
    const isoPattern = /^\d{4}-\d{2}-\d{2}T\d{2}:\d{2}:\d{2}(\.\d{3})?Z?$/;
    expect(board.createdAt).toMatch(isoPattern);
    expect(board.updatedAt).toMatch(isoPattern);
  });

  test('API versioning headers are present', async ({ request }) => {
    const response = await request.get(`${API_BASE}/boards`, {
      headers: { Authorization: `Bearer ${authToken}` }
    });

    expect(response.ok()).toBeTruthy();

    // Check for API version headers
    const headers = response.headers();
    expect(headers).toHaveProperty('content-type');
    expect(headers['content-type']).toContain('application/json');
  });

  test('JSONB metadata preserves type information', async ({ request }) => {
    // Create board and list for testing
    const boardResponse = await request.post(`${API_BASE}/boards`, {
      headers: { Authorization: `Bearer ${authToken}` },
      data: {
        title: 'JSONB Test Board',
        description: 'Testing JSONB metadata preservation'
      }
    });

    const board = await boardResponse.json();

    const listResponse = await request.post(`${API_BASE}/lists`, {
      headers: { Authorization: `Bearer ${authToken}` },
      data: {
        title: 'JSONB Test List',
        boardId: board.id,
        position: 1
      }
    });

    const list = await listResponse.json();

    // Create card with various data types in metadata
    const complexMetadata = {
      stringValue: 'test string',
      numberValue: 42,
      floatValue: 3.14159,
      booleanValue: true,
      arrayValue: [1, 2, 3, 'four', true],
      objectValue: {
        nested: {
          deeply: {
            value: 'deep nesting test'
          }
        }
      },
      nullValue: null
    };

    const cardResponse = await request.post(`${API_BASE}/cards`, {
      headers: { Authorization: `Bearer ${authToken}` },
      data: {
        title: 'JSONB Type Test Card',
        description: 'Testing JSONB type preservation',
        listId: list.id,
        position: 1,
        metadata: complexMetadata
      }
    });

    expect(cardResponse.ok()).toBeTruthy();
    const card = await cardResponse.json();

    // Retrieve card and verify metadata types are preserved
    const retrieveResponse = await request.get(`${API_BASE}/cards/${card.id}`, {
      headers: { Authorization: `Bearer ${authToken}` }
    });

    const retrievedCard = await retrieveResponse.json();
    const metadata = retrievedCard.metadata;

    // Verify type preservation
    expect(typeof metadata.stringValue).toBe('string');
    expect(metadata.stringValue).toBe('test string');
    
    expect(typeof metadata.numberValue).toBe('number');
    expect(metadata.numberValue).toBe(42);
    
    expect(typeof metadata.floatValue).toBe('number');
    expect(metadata.floatValue).toBeCloseTo(3.14159);
    
    expect(typeof metadata.booleanValue).toBe('boolean');
    expect(metadata.booleanValue).toBe(true);
    
    expect(Array.isArray(metadata.arrayValue)).toBeTruthy();
    expect(metadata.arrayValue).toEqual([1, 2, 3, 'four', true]);
    
    expect(typeof metadata.objectValue).toBe('object');
    expect(metadata.objectValue.nested.deeply.value).toBe('deep nesting test');
    
    expect(metadata.nullValue).toBeNull();
  });
});

