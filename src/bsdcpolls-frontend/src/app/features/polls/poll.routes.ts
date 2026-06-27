import { inject } from '@angular/core';
import { ResolveFn, Routes } from '@angular/router';
import { firstValueFrom } from 'rxjs';
import { BsdcPollsApiClient } from '../../generated/api';

const pollTitleResolver: ResolveFn<string> = async (route) => {
  const client = inject(BsdcPollsApiClient);
  try {
    const poll = await firstValueFrom(client.polls_GetByUid(route.paramMap.get('pollUid') ?? ''));
    return `${poll.title} — BSDCPolls`;
  } catch {
    return 'Poll — BSDCPolls';
  }
};

export const pollRoutes: Routes = [
  {
    path: 'new',
    title: 'Create Poll — BSDCPolls',
    loadComponent: () => import('./create-poll/create-poll.component').then((m) => m.CreatePollComponent),
  },
  {
    path: ':pollUid',
    title: pollTitleResolver,
    loadComponent: () => import('./session/poll-session.component').then((m) => m.PollSessionComponent),
  },
];
