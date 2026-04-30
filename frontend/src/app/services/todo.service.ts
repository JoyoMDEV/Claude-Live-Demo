import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable, computed, inject, signal } from '@angular/core';
import { Observable, tap } from 'rxjs';
import {
  CreateTodoRequest,
  Todo,
  TodoQueryParams,
  UpdateTodoRequest,
} from '../models/todo.model';

@Injectable({ providedIn: 'root' })
export class TodoService {
  private readonly http = inject(HttpClient);
  private readonly apiUrl = 'http://localhost:5005/api/todos';

  readonly todos = signal<Todo[]>([]);
  readonly loading = signal(false);

  readonly total = computed(() => this.todos().length);
  readonly completed = computed(() => this.todos().filter((t) => t.isCompleted).length);
  readonly open = computed(() => this.todos().filter((t) => !t.isCompleted).length);

  loadAll(query?: TodoQueryParams): Observable<Todo[]> {
    let params = new HttpParams();
    if (query?.search) params = params.set('search', query.search);
    if (query?.sortBy) params = params.set('sortBy', query.sortBy);
    if (query?.order) params = params.set('order', query.order);

    this.loading.set(true);
    return this.http.get<Todo[]>(this.apiUrl, { params }).pipe(
      tap({
        next: (todos) => {
          this.todos.set(todos);
          this.loading.set(false);
        },
        error: () => this.loading.set(false),
      }),
    );
  }

  getById(id: string): Observable<Todo> {
    return this.http.get<Todo>(`${this.apiUrl}/${id}`);
  }

  create(request: CreateTodoRequest): Observable<Todo> {
    return this.http.post<Todo>(this.apiUrl, request).pipe(
      tap((todo) => this.todos.update((list) => [todo, ...list])),
    );
  }

  update(id: string, request: UpdateTodoRequest): Observable<Todo> {
    return this.http.put<Todo>(`${this.apiUrl}/${id}`, request).pipe(
      tap((updated) =>
        this.todos.update((list) => list.map((t) => (t.id === id ? updated : t))),
      ),
    );
  }

  delete(id: string): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`).pipe(
      tap(() => this.todos.update((list) => list.filter((t) => t.id !== id))),
    );
  }
}
