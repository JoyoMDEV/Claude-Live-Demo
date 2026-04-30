import { CommonModule } from '@angular/common';
import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { Todo, TodoQueryParams } from '../../models/todo.model';
import { TodoService } from '../../services/todo.service';

@Component({
  selector: 'app-todo-list',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './todo-list.component.html',
  styleUrl: './todo-list.component.scss',
})
export class TodoListComponent implements OnInit {
  private readonly router = inject(Router);
  readonly todoService = inject(TodoService);

  search = signal('');
  sortBy = signal<'createdAt' | 'title' | 'isCompleted'>('createdAt');
  order = signal<'asc' | 'desc'>('desc');
  filter = signal<'all' | 'open' | 'done'>('all');

  readonly filteredTodos = computed(() => {
    const todos = this.todoService.todos();
    const f = this.filter();
    if (f === 'open') return todos.filter((t) => !t.isCompleted);
    if (f === 'done') return todos.filter((t) => t.isCompleted);
    return todos;
  });

  readonly progressPercent = computed(() => {
    const total = this.todoService.total();
    if (total === 0) return 0;
    return Math.round((this.todoService.completed() / total) * 100);
  });

  ngOnInit(): void {
    this.reload();
  }

  reload(): void {
    const query: TodoQueryParams = {
      search: this.search() || undefined,
      sortBy: this.sortBy(),
      order: this.order(),
    };
    this.todoService.loadAll(query).subscribe();
  }

  onSearchChange(value: string): void {
    this.search.set(value);
    this.reload();
  }

  onSortChange(value: string): void {
    const [sort, ord] = value.split('-') as [
      'createdAt' | 'title' | 'isCompleted',
      'asc' | 'desc',
    ];
    this.sortBy.set(sort);
    this.order.set(ord);
    this.reload();
  }

  setFilter(f: 'all' | 'open' | 'done'): void {
    this.filter.set(f);
  }

  toggleDone(todo: Todo, event: Event): void {
    event.stopPropagation();
    this.todoService
      .update(todo.id, {
        title: todo.title,
        description: todo.description,
        isCompleted: !todo.isCompleted,
      })
      .subscribe();
  }

  deleteTodo(todo: Todo, event: Event): void {
    event.stopPropagation();
    this.todoService.delete(todo.id).subscribe();
  }

  openDetail(todo: Todo): void {
    this.router.navigate(['/todos', todo.id]);
  }

  openForm(id?: string): void {
    if (id) {
      this.router.navigate(['/todos', id, 'edit']);
    } else {
      this.router.navigate(['/todos', 'new']);
    }
  }

  formatDate(dateStr: string): string {
    const d = new Date(dateStr);
    return d.toLocaleDateString('de-DE', { day: '2-digit', month: '2-digit', year: 'numeric' });
  }
}
