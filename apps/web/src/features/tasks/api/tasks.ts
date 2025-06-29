import { Task, CreateTaskDTO, UpdateTaskDTO } from '../types';

const API_BASE_URL = '/api/tasks';

/**
 * Fetches all tasks from the API
 * @returns Promise with an array of tasks
 * @throws Error if the request fails
 */
export const getTasks = async (): Promise<Task[]> => {
  const response = await fetch(API_BASE_URL);
  
  if (!response.ok) {
    throw new Error('Failed to fetch tasks');
  }
  
  return response.json();
};

/**
 * Creates a new task
 * @param task The task data to create
 * @returns Promise with the created task
 * @throws Error if the request fails
 */
export const createTask = async (task: CreateTaskDTO): Promise<Task> => {
  const response = await fetch(API_BASE_URL, {
    method: 'POST',
    headers: {
      'Content-Type': 'application/json',
    },
    body: JSON.stringify(task),
  });
  
  if (!response.ok) {
    throw new Error('Failed to create task');
  }
  
  return response.json();
};

/**
 * Updates an existing task
 * @param task The task data to update (must include id)
 * @returns Promise with the updated task
 * @throws Error if the request fails
 */
export const updateTask = async (task: UpdateTaskDTO): Promise<Task> => {
  const { id, ...updates } = task;
  const response = await fetch(`${API_BASE_URL}/${id}`, {
    method: 'PUT',
    headers: {
      'Content-Type': 'application/json',
    },
    body: JSON.stringify(updates),
  });
  
  if (!response.ok) {
    throw new Error('Failed to update task');
  }
  
  return response.json();
};

/**
 * Deletes a task by ID
 * @param id The ID of the task to delete
 * @returns Promise that resolves when the task is deleted
 * @throws Error if the request fails
 */
export const deleteTask = async (id: string): Promise<void> => {
  const response = await fetch(`${API_BASE_URL}/${id}`, {
    method: 'DELETE',
  });
  
  if (!response.ok) {
    throw new Error('Failed to delete task');
  }
  
  return response.json();
};
