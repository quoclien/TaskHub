import type { Board } from '@taskhub/shared';

const API_BASE = process.env.NEXT_PUBLIC_API_BASE_URL || 'http://localhost:5205/api';

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
  if (process.env.NODE_ENV === 'development' && !DISABLE_MOCKS) {
    // Simulate API delay
    await new Promise((resolve) => setTimeout(resolve, 500));
    return mockBoards;
  }

  const response = await fetch(`${API_BASE}/boards`, {
    headers: {
      'Content-Type': 'application/json',
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
    },
    credentials: 'include', // Include cookies for session-based auth
    body: JSON.stringify({ title, position }),
  });

  if (!response.ok) {
    throw new Error('Failed to create list');
  }
};

export const createCard = async (
  boardId: string,
  listId: string,
  title: string,
  position: number
) => {
  const response = await fetch(`${API_BASE}/boards/${boardId}/lists/${listId}/cards`, {
    method: 'POST',
    headers: {
      'Content-Type': 'application/json',
    },
    credentials: 'include', // Include cookies for session-based auth
    body: JSON.stringify({ title, position }),
  });

  if (!response.ok) {
    throw new Error('Failed to create card');
  }

  return response.json();
};

export const getCardById = async (cardId: string) => {
  const response = await fetch(`${API_BASE}/cards/${cardId}`, {
    method: 'GET',
    headers: {
      'Content-Type': 'application/json',
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
  if (process.env.NODE_ENV === 'development' && !DISABLE_MOCKS) {
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
  newListId: string,
  index: number,
  token?: string
) => {
  const response = await fetch(`/api/boards/${boardId}/cards/${cardId}/move`, {
    method: 'PUT',
    headers: {
      'Content-Type': 'application/json',
      ...(token && { Authorization: `Bearer ${token}` }),
    },
    body: JSON.stringify({
      newListId,
      index,
    }),
  });

  if (!response.ok) {
    throw new Error('Failed to move card');
  }

  return response.json();
};

/**
 * Move a list to a new position
 */
export const moveList = async (
  boardId: string,
  oldIndex: number,
  newIndex: number,
  token?: string
) => {
  const response = await fetch(`/api/boards/${boardId}/lists/move`, {
    method: 'PUT',
    headers: {
      'Content-Type': 'application/json',
      ...(token && { Authorization: `Bearer ${token}` }),
    },
    body: JSON.stringify({
      oldIndex,
      newIndex,
    }),
  });

  if (!response.ok) {
    throw new Error('Failed to move list');
  }

  return response.json();
};
