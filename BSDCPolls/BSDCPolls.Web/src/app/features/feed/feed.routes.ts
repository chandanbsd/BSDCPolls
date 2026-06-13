import { Routes } from '@angular/router';

export const feedRoutes: Routes = [
  {
    path: '',
    loadComponent: () =>
      import('./feed.component').then((m) => m.FeedComponent),
  },
];
