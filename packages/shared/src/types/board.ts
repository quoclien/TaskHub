export interface Card {
  id: string;
  title: string;
  description?: string | null;
  position: number;
  listId: string;
}

export interface List {
  id: string;
  title: string;
  position: number;
  boardId: string;
  cards: Card[];
}

export interface BoardMember {
  userId: string;
  userName: string;
  userEmail: string;
  userAvatar?: string | null;
  role: 'Owner' | 'Admin' | 'Member' | 'Viewer';
  joinedAt: string;
}

export interface Board {
  id: string;
  title: string;
  description?: string | null;
  isPublic?: boolean;
  ownerId?: string;
  createdAt: string;
  updatedAt: string;
  lists?: List[];
  members?: BoardMember[];
}
