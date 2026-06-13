import { Routes } from '@angular/router';
import { authGuard } from './core/auth/auth.guard';

export const routes: Routes = [
  {
    path: 'auth',
    loadChildren: () =>
      import('./features/auth/auth.routes').then((m) => m.authRoutes),
  },
  {
    path: 'polls',
    canActivate: [authGuard],
    loadChildren: () =>
      import('./features/polls/poll.routes').then((m) => m.pollRoutes),
  },
  {
    path: 'surveys',
    canActivate: [authGuard],
    loadChildren: () =>
      import('./features/surveys/survey.routes').then((m) => m.surveyRoutes),
  },
  {
    path: 'feed',
    canActivate: [authGuard],
    loadChildren: () =>
      import('./features/feed/feed.routes').then((m) => m.feedRoutes),
  },
  {
    path: '',
    redirectTo: 'auth/login',
    pathMatch: 'full',
  },
  {
    path: '**',
    redirectTo: 'auth/login',
  },
];
