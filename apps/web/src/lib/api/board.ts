import type { Board } from '@taskhub/shared';

const API_BASE = process.env.NEXT_PUBLIC_API_BASE_URL || 'http://localhost:5205/api';

// Temporary development token for testing
const DEV_TOKEN = 'eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIwMTk4ZmE3NS1kZTBlLTcwNjAtOGY0Ny1iN2JhNzEzMjIzZmQiLCJqdGkiOiJkYjY0NzZlNC04MjE3LTQ2ZGUtYjgwYS01YTZlNTAyMmVmYzQiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1lIjoiMDE5OGZhNzUtZGUwZS03MDYwLThmNDctYjdiYTcxMzIyM2ZkIiwiaHR0cDovL3NjaGVtYXMubWljcm9zb2Z0LmNvbS93cy8yMDA4LzA2L2lkZW50aXR5L2NsYWltcy9yb2xlIjoiYWRtaW4iLCJleHAiOjE3NTkxNDA2ODUsImlzcyI6IlRhc2tIdWItRGV2IiwiYXVkIjoiVGFza0h1Yi1EZXYtVXNlcnMifQ.2K66oSO3bZZoZ7gzjk23x_tUTZPPaY1_6ikaNsrz8pk';

// Mock data for development
const mockBoards = [
  {
    id: '1',
    title: 'Project Planning',
    description: 'Planning and organizing our next project',
    created_at: new Date().toISOString(),
    updated_at: new Date().toISOString(),
  },
  {
    id: '2',
    title: 'Marketing Campaign',
    description: 'Q1 2024 marketing campaign tasks',
    created_at: new Date().toISOString(),
    updated_at: new Date().toISOString(),
  },
  {
    id: '3',
    title: 'Website Redesign',
    description: 'Complete website overhaul and modernization',
    created_at: new Date().toISOString(),
    updated_at: new Date().toISOString(),
  },
  {
    id: '4',
    title: 'Bug Fixes',
    description: 'Track and fix current bugs',
    created_at: new Date().toISOString(),
    updated_at: new Date().toISOString(),
  },
];

export const fetchBoards = async () => {
  // For development, return mock data unless explicitly disabled for E2E testing
  const DISABLE_MOCKS = process.env.NEXT_PUBLIC_DISABLE_MOCKS === 'true';
  if (false && process.env.NODE_ENV === 'development' && !DISABLE_MOCKS) {
    // Simulate API delay
    await new Promise((resolve) => setTimeout(resolve, 500));
    return mockBoards;
  }

  const response = await fetch(`${API_BASE}/boards`, {
    headers: {
      'Content-Type': 'application/json',
      'Authorization': `Bearer ${DEV_TOKEN}`,
    },
    credentials: 'include', // Include cookies for session-based auth
  });

  if (!response.ok) {
    // Fallback to mock data if API fails
    console.warn('API request failed, falling back to mock data');
    return mockBoards;
  }

  return response.json();
};

export const createBoard = async (board: { title: string; description?: string }) => {
  const response = await fetch(`${API_BASE}/boards`, {
    method: 'POST',
    headers: {
      'Content-Type': 'application/json',
      'Authorization': `Bearer ${DEV_TOKEN}`,
    },
    credentials: 'include', // Include cookies for session-based auth
    body: JSON.stringify(board),
  });

  if (!response.ok) {
    throw new Error('Failed to create board');
  }

  return response.json();
};

export const deleteBoard = async (boardId: string) => {
  const response = await fetch(`${API_BASE}/boards/${boardId}`, {
    method: 'DELETE',
    headers: {
      'Content-Type': 'application/json',
      'Authorization': `Bearer ${DEV_TOKEN}`,
    },
    credentials: 'include', // Include cookies for session-based auth
  });

  if (!response.ok) {
    throw new Error('Failed to delete board');
  }
};

export const createList = async (
  boardId: string,
  title: string,
  position: number
): Promise<void> => {
  const response = await fetch(`${API_BASE}/boards/${boardId}/lists`, {
    method: 'POST',
    headers: {
      'Content-Type': 'application/json',
      'Authorization': `Bearer ${DEV_TOKEN}`,
    },
    credentials: 'include', // Include cookies for session-based auth
    body: JSON.stringify({ Title: title, Position: position }), // Fix: Use PascalCase for .NET API
  });

  if (!response.ok) {
    const errorText = await response.text();
    console.error('Create list error:', errorText);
    throw new Error('Failed to create list');
  }
};

export const createCard = async (
  boardId: string,
  listId: string,
  title: string,
  position: number,
  description?: string,
  token?: string // Keep token param for backward compatibility
) => {
  console.log('[CreateCard] Starting request with params:', {
    boardId,
    listId,
    title,
    position,
    hasDescription: !!description,
  });

  const requestBody = { Title: title, Description: description, Position: position };
  console.log('[CreateCard] Request body:', requestBody);
  console.log('[CreateCard] Request URL:', `${API_BASE}/boards/${boardId}/lists/${listId}/cards`);

  try {
    const response = await fetch(`${API_BASE}/boards/${boardId}/lists/${listId}/cards`, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
        'Authorization': `Bearer ${DEV_TOKEN}`, // Always use DEV_TOKEN for consistency
      },
      credentials: 'include',
      body: JSON.stringify(requestBody),
    });

    console.log('[CreateCard] Response status:', response.status);
    
    if (!response.ok) {
      const errorText = await response.text();
      console.error('[CreateCard] Error response:', {
        status: response.status,
        statusText: response.statusText,
        body: errorText || 'No error body received'
      });
      throw new Error(`Failed to create card: ${response.status} ${response.statusText}${errorText ? ` - ${errorText}` : ''}`);
    }

    const data = await response.json();
    console.log('[CreateCard] Success response:', data);
    return data;
  } catch (error) {
    console.error('[CreateCard] Request failed:', error);
    throw error;
  }
};

// Mock cards for development - extracted from mockLists for easier access
const mockCards = [
  {
    id: 'card-1',
    title: 'Set up project board',
    description: 'Initialize the project board with basic lists and configure the initial workflow',
    listId: 'list-1',
    position: 0,
    createdAt: new Date().toISOString(),
    updatedAt: new Date().toISOString(),
    metadata: { labels: ['setup'] },
    dueDate: new Date(Date.now() + 3 * 24 * 60 * 60 * 1000).toISOString(),
    isArchived: false,
    assignedTo: 'user-1',
    comments: [
      { content: 'Started working on this task', createdAt: new Date().toISOString() },
      { content: 'Making good progress', createdAt: new Date().toISOString() }
    ],
  },
  {
    id: 'card-2',
    title: 'Review design mockups',
    description: 'Go through the latest UI/UX designs and provide feedback on the proposed interface changes',
    listId: 'list-1',
    position: 1,
    createdAt: new Date().toISOString(),
    updatedAt: new Date().toISOString(),
    metadata: { labels: ['design', 'review'] },
    dueDate: null,
    isArchived: false,
    assignedTo: null,
    comments: [],
  },
  {
    id: 'card-3',
    title: 'Implement authentication',
    description: 'Set up JWT authentication flow with proper session management and refresh token handling',
    listId: 'list-2',
    position: 0,
    createdAt: new Date().toISOString(),
    updatedAt: new Date().toISOString(),
    metadata: { labels: ['backend'] },
    dueDate: new Date(Date.now() + 24 * 60 * 60 * 1000).toISOString(),
    isArchived: false,
    assignedTo: 'user-1',
    comments: [
      { content: 'Authentication endpoint created', createdAt: new Date().toISOString() }
    ],
  },
  {
    id: 'card-4',
    title: 'Project kickoff',
    description: 'Initial project setup and planning session with all stakeholders to define goals and timeline',
    listId: 'list-4',
    position: 0,
    createdAt: new Date(Date.now() - 2 * 24 * 60 * 60 * 1000).toISOString(),
    updatedAt: new Date(Date.now() - 24 * 60 * 60 * 1000).toISOString(),
    metadata: { labels: ['planning'] },
    dueDate: null,
    isArchived: false,
    assignedTo: 'user-1',
    comments: [
      { content: 'Kickoff meeting completed successfully', createdAt: new Date().toISOString() },
      { content: 'All team members are aligned on the goals', createdAt: new Date().toISOString() }
    ],
  },
];

export const getCardById = async (cardId: string) => {
  // For development, return mock data unless explicitly disabled for E2E testing
  const DISABLE_MOCKS = process.env.NEXT_PUBLIC_DISABLE_MOCKS === 'true';
  if (false && process.env.NODE_ENV === 'development' && !DISABLE_MOCKS) {
    // Simulate API delay
    await new Promise((resolve) => setTimeout(resolve, 300));
    
    const card = mockCards.find((c) => c.id === cardId);
    if (!card) {
      throw new Error('Card not found');
    }
    
    return card;
  }

  const response = await fetch(`${API_BASE}/cards/${cardId}`, {
    method: 'GET',
    headers: {
      'Content-Type': 'application/json',
      'Authorization': `Bearer ${DEV_TOKEN}`,
    },
    credentials: 'include', // Include cookies for session-based auth
  });

  if (!response.ok) {
    throw new Error('Failed to fetch card');
  }

  return response.json();
};

// Mock data for development
const mockLists = [
  {
    id: 'list-1',
    title: 'To Do',
    position: 0,
    boardId: '1',
    cards: [
      {
        id: 'card-1',
        title: 'Set up project board',
        position: 0,
        description: 'Initialize the project board with basic lists',
        listId: 'list-1',
        boardId: '1',
        createdAt: new Date().toISOString(),
        updatedAt: new Date().toISOString(),
        createdBy: 'user-1',
        labels: ['setup'],
        dueDate: new Date(Date.now() + 3 * 24 * 60 * 60 * 1000).toISOString(),
      },
      {
        id: 'card-2',
        title: 'Review design mockups',
        position: 1,
        description: 'Go through the latest UI/UX designs',
        listId: 'list-1',
        boardId: '1',
        createdAt: new Date().toISOString(),
        updatedAt: new Date().toISOString(),
        createdBy: 'user-1',
        labels: ['design', 'review'],
      },
    ],
  },
  {
    id: 'list-2',
    title: 'In Progress',
    position: 1,
    boardId: '1',
    cards: [
      {
        id: 'card-3',
        title: 'Implement authentication',
        position: 0,
        description: 'Set up JWT authentication flow',
        listId: 'list-2',
        boardId: '1',
        createdAt: new Date().toISOString(),
        updatedAt: new Date().toISOString(),
        createdBy: 'user-1',
        labels: ['backend'],
        dueDate: new Date(Date.now() + 24 * 60 * 60 * 1000).toISOString(),
      },
    ],
  },
  {
    id: 'list-3',
    title: 'In Review',
    position: 2,
    boardId: '1',
    cards: [],
  },
  {
    id: 'list-4',
    title: 'Done',
    position: 3,
    boardId: '1',
    cards: [
      {
        id: 'card-4',
        title: 'Project kickoff',
        position: 0,
        description: 'Initial project setup and planning',
        listId: 'list-4',
        boardId: '1',
        createdAt: new Date(Date.now() - 2 * 24 * 60 * 60 * 1000).toISOString(),
        updatedAt: new Date(Date.now() - 24 * 60 * 60 * 1000).toISOString(),
        createdBy: 'user-1',
        labels: ['planning'],
      },
    ],
  },
];

export const fetchBoardById = async (boardId: string): Promise<Board> => {
  // In development, return mock data unless explicitly disabled for E2E testing
  const DISABLE_MOCKS = process.env.NEXT_PUBLIC_DISABLE_MOCKS === 'true';
  if (false && process.env.NODE_ENV === 'development' && !DISABLE_MOCKS) {
    console.log('Using mock data for board:', boardId);
    await new Promise((resolve) => setTimeout(resolve, 500));

    const board = mockBoards.find((b) => b.id === boardId);

    if (!board) {
      throw new Error('Board not found');
    }

    // For the first board, add mock lists and cards
    if (boardId === '1') {
      return {
        ...board,
        lists: mockLists,
      } as unknown as Board;
    }

    // For other boards, return with empty lists
    return {
      ...board,
      lists: [],
    } as unknown as Board;
  }

  // Production/authenticated API call
  const response = await fetch(`${API_BASE}/boards/${boardId}`, {
    headers: {
      'Content-Type': 'application/json',
      'Authorization': `Bearer ${DEV_TOKEN}`,
    },
    credentials: 'include', // Include cookies for session-based auth
  });

  if (!response.ok) {
    throw new Error('Failed to fetch board');
  }

  return response.json();
};

/**
 * Move a card to a new list and position
 */
export const moveCard = async (
  boardId: string,
  cardId: string,
  currentListId: string,
  newListId: string,
  newPosition: number,
  token?: string
) => {
  const response = await fetch(`${API_BASE}/boards/${boardId}/lists/${currentListId}/cards/${cardId}/move`, {
    method: 'POST',
    headers: {
      'Content-Type': 'application/json',
      'Authorization': `Bearer ${DEV_TOKEN}`,
    },
    credentials: 'include',
    body: JSON.stringify({
      TargetListId: newListId,
      NewPosition: newPosition,
    }),
  });

  if (!response.ok) {
    throw new Error('Failed to move card');
  }

  // Backend returns 204 No Content on success, so no JSON to parse
  return;
};

/**
 * Move a list to a new position
 * TODO: Implement backend endpoint for list reordering
 */
export const moveList = async (
  boardId: string,
  oldIndex: number,
  newIndex: number,
  token?: string
) => {
  // Temporarily disable list reordering until backend endpoint is implemented
  console.warn('List reordering is not yet implemented on the backend');
  throw new Error('List reordering is not yet implemented');
  
  // TODO: Uncomment when backend endpoint is ready
  /*
  const response = await fetch(`${API_BASE}/boards/${boardId}/lists/move`, {
    method: 'PUT',
    headers: {
      'Content-Type': 'application/json',
      'Authorization': `Bearer ${DEV_TOKEN}`,
    },
    credentials: 'include',
    body: JSON.stringify({
      oldIndex,
      newIndex,
    }),
  });

  if (!response.ok) {
    throw new Error('Failed to move list');
  }

  // Backend returns 204 No Content on success, so no JSON to parse
  return;
  */
};

/**
 * Update a list's title
 */
export const updateList = async (
  boardId: string,
  listId: string,
  title: string
) => {
  const response = await fetch(`${API_BASE}/boards/${boardId}/lists/${listId}`, {
    method: 'PUT',
    headers: {
      'Content-Type': 'application/json',
      'Authorization': `Bearer ${DEV_TOKEN}`,
    },
    credentials: 'include',
    body: JSON.stringify({
      Title: title,
    }),
  });

  if (!response.ok) {
    const errorText = await response.text();
    console.error('Update list error:', errorText);
    throw new Error('Failed to update list');
  }
};

/**
 * Delete a list and all its cards
 */
export const deleteList = async (
  boardId: string,
  listId: string
) => {
  const response = await fetch(`${API_BASE}/boards/${boardId}/lists/${listId}`, {
    method: 'DELETE',
    headers: {
      'Content-Type': 'application/json',
      'Authorization': `Bearer ${DEV_TOKEN}`,
    },
    credentials: 'include',
  });

  if (!response.ok) {
    const errorText = await response.text();
    console.error('Delete list error:', errorText);
    throw new Error('Failed to delete list');
  }
};
