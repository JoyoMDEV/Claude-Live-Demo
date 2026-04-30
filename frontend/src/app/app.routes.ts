import { Routes } from '@angular/router';

export const routes: Routes = [
  {
    path: '',
    loadComponent: () =>
      import('./components/todo-list/todo-list.component').then((m) => m.TodoListComponent),
  },
  {
    path: 'todos/new',
    loadComponent: () =>
      import('./components/todo-form/todo-form.component').then((m) => m.TodoFormComponent),
  },
  {
    path: 'todos/:id/edit',
    loadComponent: () =>
      import('./components/todo-form/todo-form.component').then((m) => m.TodoFormComponent),
  },
  {
    path: 'todos/:id',
    loadComponent: () =>
      import('./components/todo-detail/todo-detail.component').then(
        (m) => m.TodoDetailComponent,
      ),
  },
  { path: '**', redirectTo: '' },
];
