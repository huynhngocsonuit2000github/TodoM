# Bind value
- Sync value: we use [(ngModel)] to achieve two ways binding
``` cs
title = '';

<input [(ngModel)]="title"/>

<!-- Equals -->
<input [ngModel]="title" (ngModelChange)="title = $event">
```

- Async value: we use [ngModel] withing (ngModelChange)="title$.next($event)" to achieve two ways binding
``` md
title$ = new BehaviorSubject<string>('');

<input [ngModel]="title$ | async" (ngModelChange)="title$.next($event)">
```

- There are some kinds of binding {{ }}
  - html read data into text
    <h1>{{ title }}</h1>
  - Property binding []
    <img [src]="avatarUrl">
    <button [disabled]="isSaving">Save</button>
  - Event binding ()
    <button (click)="save()">Save</button>
    <input (input)="title = ($event.target as HTMLInputElement).value">
  - Two-way binding
    <input [(ngModel)]="title">
  - Class/style binding
    <div [class.active]="isActive"></div>
    <div [style.fontSize.px]="size"></div>
  - Structural directives *
    <div *ngIf="error">{{ error }}</div>
    <li *ngFor="let t of todos">{{ t }}</li>
  - Observable binding with async
    <li *ngFor="let t of (todos$ | async)">{{ t.title }}</li>
