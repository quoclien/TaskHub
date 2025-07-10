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

export interface Board {
  id: string;
  title: string;
  description?: string | null;
  isPublic: boolean;
  lists: List[];
}
