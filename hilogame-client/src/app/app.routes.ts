import {Routes} from '@angular/router';
import {authGuard} from './guards/auth-guard';
import {MainLayout} from './components/layouts/main-layout/main-layout';

export const routes: Routes = [
  {
    path: 'auth',
    loadComponent: () => import("./components/layouts/auth-layout/auth-layout").then(c => c.AuthLayout),
    children: [
      {
        path: 'login',
        loadComponent: () => import('./components/auth/login/login').then(c => c.Login),
        pathMatch: 'full'
      },
      {
        path: 'register',
        loadComponent: () => import('./components/auth/register/register').then(c => c.Register),
        pathMatch: 'full'
      },
      {
        path: '',
        pathMatch: 'full',
        redirectTo: 'login'
      },
    ]
  },
  {
    path: '',
    component: MainLayout,
    canMatch: [authGuard],

    children: [
      { path: 'rooms', loadComponent: () => import('./components/hilo-home/hilo-home.component').then(c => c.HiloHomeComponent) },
      { path: '', redirectTo: 'rooms', pathMatch: 'full' }, // ← remplace 'home' inexistant
    ]
  },
  {path: '', redirectTo: 'home', pathMatch: 'full'},

];
