export interface Task {
  id: string;
  title: string;
  completed: boolean;
  description?: string;
  dueDate?: string;
  priority?: 'low' | 'medium' | 'high';
  createdAt?: string;
  updatedAt?: string;
}

export type CreateTaskDTO = Omit<Task, 'id' | 'createdAt' | 'updatedAt'>;
export type UpdateTaskDTO = Partial<Omit<Task, 'id' | 'createdAt' | 'updatedAt'>> & { id: string };
