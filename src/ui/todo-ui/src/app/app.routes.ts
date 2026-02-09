import { Routes } from '@angular/router';
import { Todos } from './pages/todos/todos';
import { Home } from './pages/home/home';
import { Login } from './pages/login/login';
import { About } from './pages/about/about';
import { RoleGuard } from './role.guard';

export const routes: Routes = [
    { path: '', component: Home },
    { path: 'todos', component: Todos, canActivate: [RoleGuard], data: { role: 'admin' } },
    { path: 'about', component: About },
    { path: 'login', component: Login },
    { path: '**', redirectTo: '' },
];