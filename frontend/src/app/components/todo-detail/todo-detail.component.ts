import { CommonModule } from '@angular/common';
import { Component, OnInit, inject, signal } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { Todo } from '../../models/todo.model';
import { TodoService } from '../../services/todo.service';

@Component({
  selector: 'app-todo-detail',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './todo-detail.component.html',
  styleUrl: './todo-detail.component.scss',
})
export class TodoDetailComponent implements OnInit {
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly todoService = inject(TodoService);

  readonly todo = signal<Todo | null>(null);
  readonly loading = signal(true);

  ngOnInit(): void {
    const id = this.route.snapshot.paramMap.get('id')!;
    this.todoService.getById(id).subscribe({
      next: (todo) => {
        this.todo.set(todo);
        this.loading.set(false);
      },
      error: () => this.router.navigate(['/']),
    });
  }

  toggleDone(): void {
    const t = this.todo();
    if (!t) return;
    this.todoService
      .update(t.id, { title: t.title, description: t.description, isCompleted: !t.isCompleted })
      .subscribe((updated) => this.todo.set(updated));
  }

  openEdit(): void {
    const t = this.todo();
    if (t) this.router.navigate(['/todos', t.id, 'edit']);
  }

  delete(): void {
    const t = this.todo();
    if (!t) return;
    this.todoService.delete(t.id).subscribe(() => this.router.navigate(['/']));
  }

  back(): void {
    this.router.navigate(['/']);
  }

  formatDate(dateStr: string): string {
    return new Date(dateStr).toLocaleDateString('de-DE', {
      day: '2-digit',
      month: '2-digit',
      year: 'numeric',
      hour: '2-digit',
      minute: '2-digit',
    });
  }
}
