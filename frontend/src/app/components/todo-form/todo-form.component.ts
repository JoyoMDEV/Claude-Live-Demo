import { CommonModule } from '@angular/common';
import { Component, OnInit, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { TodoService } from '../../services/todo.service';

@Component({
  selector: 'app-todo-form',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './todo-form.component.html',
  styleUrl: './todo-form.component.scss',
})
export class TodoFormComponent implements OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly router = inject(Router);
  private readonly route = inject(ActivatedRoute);
  private readonly todoService = inject(TodoService);

  readonly isEditMode = signal(false);
  readonly editId = signal<string | null>(null);
  readonly submitting = signal(false);
  readonly error = signal<string | null>(null);

  readonly form = this.fb.group({
    title: ['', [Validators.required, Validators.maxLength(200)]],
    description: [''],
  });

  ngOnInit(): void {
    const id = this.route.snapshot.paramMap.get('id');
    if (id) {
      this.isEditMode.set(true);
      this.editId.set(id);
      this.todoService.getById(id).subscribe({
        next: (todo) =>
          this.form.patchValue({ title: todo.title, description: todo.description ?? '' }),
        error: () => this.router.navigate(['/']),
      });
    }
  }

  get titleControl() {
    return this.form.get('title')!;
  }

  submit(): void {
    console.log('Submit wurde geklickt!');
    console.log('Formular-Status:', this.form.status);
    console.log('Formular-Werte:', this.form.value);

    if (this.form.invalid) {
      console.warn('Formular ist INVALID!', this.form.errors);
      this.form.markAllAsTouched();
      return;
    }

    this.submitting.set(true);
    this.error.set(null);

    const title = this.form.value.title!;
    const description = this.form.value.description?.trim() || null;

    if (this.isEditMode()) {
      const id = this.editId()!;
      const existing = this.todoService.todos().find((t) => t.id === id);
      this.todoService
        .update(id, {
          title,
          description,
          isCompleted: existing?.isCompleted ?? false,
        })
        .subscribe({
          next: () => this.router.navigate(['/']),
          error: () => {
            this.error.set('Fehler beim Speichern. Bitte erneut versuchen.');
            this.submitting.set(false);
          },
        });
    } else {
      this.todoService.create({ title, description }).subscribe({
        next: () => this.router.navigate(['/']),
        error: () => {
          this.error.set('Fehler beim Erstellen. Bitte erneut versuchen.');
          this.submitting.set(false);
        },
      });
    }
  }

  cancel(): void {
    this.router.navigate(['/']);
  }
}
