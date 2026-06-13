import { Routes } from '@angular/router';

export const pollRoutes: Routes = [
  {
    path: 'new',
    loadComponent: () => import('./create-poll/create-poll.component').then((m) => m.CreatePollComponent),
  },
  {
    path: ':pollUid',
    loadComponent: () => import('./session/poll-session.component').then((m) => m.PollSessionComponent),
  },
];
