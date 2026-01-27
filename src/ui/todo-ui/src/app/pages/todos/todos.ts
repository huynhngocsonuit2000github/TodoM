// import { ChangeDetectorRef, Component, OnInit } from '@angular/core';
// import { Todo, TodoService } from '../../services/todo';
// import { FormsModule } from '@angular/forms';
// import { CommonModule } from '@angular/common';
// import { finalize } from 'rxjs';

// @Component({
//   selector: 'app-todos',
//   imports: [FormsModule, CommonModule],
//   templateUrl: './todos.html',
//   styleUrls: ['./todos.scss']
// })
// export class TodosComponent implements OnInit {
//   todos: Todo[] = [{ id: 1, title: "jnajaj", isDone: false }];
//   title = '';
//   error = '';
//   loading = false;


//   constructor(private todoSvc: TodoService, private cdr: ChangeDetectorRef) { }

//   ngOnInit(): void {
//     this.load();
//   }

//   load() {
//     this.loading = true;
//     this.todoSvc.getTodos().subscribe({

//       next: r => {
//         this.todos = r;
//         this.loading = false;
//         this.cdr.detectChanges();
//       },
//       error: _ => {
//         this.error = 'Load failed';
//         this.loading = false;
//       }
//     });
//   }

//   loadState() {
//     this.cdr.detectChanges();
//   }

//   create() {
//     if (!this.title.trim()) return;

//     this.todoSvc.createTodo(this.title).subscribe({
//       next: t => {

//         this.todos.unshift(t);
//         this.title = '';

//         this.cdr.detectChanges();
//       },
//       error: _ => this.error = 'Create failed'
//     });
//   }
// }

import { Component, OnInit } from '@angular/core';
import { Todo, TodoService } from '../../services/todo';
import { FormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { BehaviorSubject } from 'rxjs';

@Component({
  selector: 'app-todos',
  standalone: true,
  imports: [FormsModule, CommonModule],
  templateUrl: './todos.html',
  styleUrls: ['./todos.scss']
})
export class TodosComponent implements OnInit {

  todos$ = new BehaviorSubject<Todo[]>([]);

  title = '';
  error = '';
  loading = false;

  constructor(private todoSvc: TodoService) {
  }

  ngOnInit(): void {

    this.load();
  }

  load() {
    this.loading = true;

    this.todoSvc.getTodos().subscribe({
      next: r => {
        this.todos$.next(r);
        this.loading = false;
      },
      error: _ => {
        this.error = 'Load failed';
        this.loading = false;
      }
    });
  }

  create() {
    if (!this.title.trim()) return;

    this.todoSvc.createTodo(this.title).subscribe({
      next: t => {
        this.todos$.next([t, ...this.todos$.value]);
        this.title = '';
      },
      error: _ => this.error = 'Create failed'
    });
  }
}
