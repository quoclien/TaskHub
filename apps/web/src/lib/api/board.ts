import { Board } from "@taskhub/shared";

const API_BASE = process.env.NEXT_PUBLIC_API_BASE_URL || 'http://localhost:5000/api';

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

export const fetchBoards = async (token: string | null) => {
  // For development, return mock data if no token or if API is not available
  if (!token || process.env.NODE_ENV === 'development') {
    // Simulate API delay
    await new Promise(resolve => setTimeout(resolve, 500));
    return mockBoards;
  }

  const response = await fetch(`${API_BASE}/boards`, {
    headers: {
      'Authorization': `Bearer ${token}`,
      'Content-Type': 'application/json',
    },
  });

  if (!response.ok) {
    // Fallback to mock data if API fails
    console.warn('API request failed, falling back to mock data');
    return mockBoards;
  }

  return response.json();
};

export const createBoard = async (board: { title: string; description?: string }, token: string | null) => {
  if (!token) throw new Error('Authentication required');

  const response = await fetch(`${API_BASE}/boards`, {
    method: 'POST',
    headers: {
      'Authorization': `Bearer ${token}`,
      'Content-Type': 'application/json',
    },
    body: JSON.stringify(board),
  });

  if (!response.ok) {
    throw new Error('Failed to create board');
  }

  return response.json();
};

export const deleteBoard = async (boardId: string, token: string | null) => {
  if (!token) throw new Error('Authentication required');

  const response = await fetch(`${API_BASE}/boards/${boardId}`, {
    method: 'DELETE',
    headers: {
      'Authorization': `Bearer ${token}`,
      'Content-Type': 'application/json',
    },
  });

  if (!response.ok) {
    throw new Error('Failed to delete board');
  }
};

export const createList = async (
  boardId: string,
  title: string,
  position: number,
  token: string | null
): Promise<void> => {
  if (!token) {
    throw new Error('Not authenticated');
  }

  const response = await fetch(`${API_BASE}/boards/${boardId}/lists`, {
    method: 'POST',
    headers: {
      'Authorization': `Bearer ${token}`,
      'Content-Type': 'application/json',
    },
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
  position: number,
  token: string | null
) => {
  if (!token) {
    throw new Error('Not authenticated');
  }

  const response = await fetch(`${API_BASE}/boards/${boardId}/lists/${listId}/cards`, {
    method: 'POST',
    headers: {
      'Content-Type': 'application/json',
      'Authorization': `Bearer ${token}`,
    },
    body: JSON.stringify({ title, position }),
  });

  if (!response.ok) {
    throw new Error('Failed to create card');
  }

  return response.json();
};

export const fetchBoardById = async (boardId: string, token: string | null): Promise<Board> => {
  if (!token) {
    throw new Error('Not authenticated');
  }

  const response = await fetch(`${API_BASE}/boards/${boardId}`, {
    headers: {
      'Authorization': `Bearer ${token}`,
      'Content-Type': 'application/json',
    },
  });

  if (!response.ok) {
    throw new Error('Failed to fetch board');
  }

  return response.json();
};
