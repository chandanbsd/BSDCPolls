import { Routes } from '@angular/router';

export const feedRoutes: Routes = [
  {
    path: '',
    title: 'Feed — BSDCPolls',
    loadComponent: () => import('./feed.component').then((m) => m.FeedComponent),
  },
];
