import { Routes } from '@angular/router';

export const profileRoutes: Routes = [
  {
    path: '',
    title: 'Profile — BSDCPolls',
    loadComponent: () => import('./profile.component').then((m) => m.ProfileComponent),
  },
];
