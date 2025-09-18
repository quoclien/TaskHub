import { z } from 'zod';

export type UserRole = 'owner' | 'member' | 'viewer';

export interface User {
  id: string;
  name: string;
  email: string;
  image?: string;
  createdAt: Date;
  updatedAt: Date;
}

export interface BoardMember {
  userId: string;
  role: UserRole;
  joinedAt: Date;
}

export interface Board {
  id: string;
  title: string;
  description?: string;
  isPublic: boolean;
  createdAt: Date;
  updatedAt: Date;
  createdBy: string;
  members: BoardMember[];
}

export interface List {
  id: string;
  title: string;
  position: number;
  boardId: string;
  createdAt: Date;
  updatedAt: Date;
}

export interface Card {
  id: string;
  title: string;
  description?: string;
  position: number;
  listId: string;
  boardId: string;
  createdAt: Date;
  updatedAt: Date;
  createdBy: string;
  assignedTo?: string[];
  dueDate?: Date;
  labels?: string[];
  attachments?: string[];
  metadata?: Record<string, unknown>;
}

export interface Comment {
  id: string;
  content: string;
  cardId: string;
  userId: string;
  createdAt: Date;
  updatedAt: Date;
  parentId?: string;
}

// Zod schemas for runtime validation
export const userRoleSchema = z.enum(['owner', 'member', 'viewer']);

export const boardMemberSchema = z.object({
  userId: z.string().uuid(),
  role: userRoleSchema,
  joinedAt: z.date(),
});

export const boardSchema = z.object({
  id: z.string().uuid(),
  title: z.string().min(1).max(100),
  description: z.string().max(500).optional(),
  isPublic: z.boolean().default(false),
  createdAt: z.date(),
  updatedAt: z.date(),
  createdBy: z.string().uuid(),
  members: z.array(boardMemberSchema).min(1),
});

export const listSchema = z.object({
  id: z.string().uuid(),
  title: z.string().min(1).max(100),
  position: z.number().int().nonnegative(),
  boardId: z.string().uuid(),
  createdAt: z.date(),
  updatedAt: z.date(),
});

export const cardSchema = z.object({
  id: z.string().uuid(),
  title: z.string().min(1).max(100),
  description: z.string().max(2000).optional(),
  position: z.number().int().nonnegative(),
  listId: z.string().uuid(),
  boardId: z.string().uuid(),
  createdAt: z.date(),
  updatedAt: z.date(),
  createdBy: z.string().uuid(),
  assignedTo: z.array(z.string().uuid()).optional(),
  dueDate: z.date().optional(),
  labels: z.array(z.string()).optional(),
  attachments: z.array(z.string().url()).optional(),
  metadata: z.record(z.unknown()).optional(),
});

export const commentSchema = z.object({
  id: z.string().uuid(),
  content: z.string().min(1).max(1000),
  cardId: z.string().uuid(),
  userId: z.string().uuid(),
  createdAt: z.date(),
  updatedAt: z.date(),
  parentId: z.string().uuid().optional(),
});

// Type guards
export function isBoard(data: unknown): data is Board {
  return boardSchema.safeParse(data).success;
}

export function isCard(data: unknown): data is Card {
  return cardSchema.safeParse(data).success;
}

export function isComment(data: unknown): data is Comment {
  return commentSchema.safeParse(data).success;
}
