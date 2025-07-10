import { Board } from "@task-hub/shared";

const API_BASE_URL = 'http://localhost:5204/api';

const getAuthHeaders = (token: string) => ({
  'Authorization': `Bearer ${token}`,
});

export const fetchBoards = async (token: string | null): Promise<Board[]> => {
  if (!token) {
    throw new Error('Not authenticated');
  }

  const response = await fetch(`${API_BASE_URL}/Boards`, {
    headers: getAuthHeaders(token),
  });

  if (!response.ok) {
    throw new Error('Failed to fetch boards');
  }

  return response.json();
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

  const response = await fetch(`${API_BASE_URL}/Boards/${boardId}/Lists`, {
    method: 'POST',
    headers: {
      ...getAuthHeaders(token),
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

  const response = await fetch(`${API_BASE_URL}/boards/${boardId}/lists/${listId}/cards`, {
    method: 'POST',
    headers: {
      'Content-Type': 'application/json',
      ...getAuthHeaders(token),
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

  const response = await fetch(`${API_BASE_URL}/Boards/${boardId}`, {
    headers: getAuthHeaders(token),
  });

  if (!response.ok) {
    throw new Error('Failed to fetch board');
  }

  return response.json();
};
