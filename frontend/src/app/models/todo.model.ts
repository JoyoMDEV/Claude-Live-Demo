export interface Todo {
  id: string;
  title: string;
  description: string | null;
  isCompleted: boolean;
  createdAt: string;
  updatedAt: string;
}

export interface CreateTodoRequest {
  title: string;
  description: string | null;
}

export interface UpdateTodoRequest {
  title: string;
  description: string | null;
  isCompleted: boolean;
}

export interface TodoQueryParams {
  search?: string;
  sortBy?: 'title' | 'createdAt' | 'isCompleted';
  order?: 'asc' | 'desc';
}
