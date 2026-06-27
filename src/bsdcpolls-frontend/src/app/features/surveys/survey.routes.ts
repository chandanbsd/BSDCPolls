import { inject } from '@angular/core';
import { ResolveFn, Routes } from '@angular/router';
import { firstValueFrom } from 'rxjs';
import { BsdcPollsApiClient } from '../../generated/api';

const surveyTitleResolver: ResolveFn<string> = async (route) => {
  const client = inject(BsdcPollsApiClient);
  try {
    const survey = await firstValueFrom(client.surveys_GetByUid(route.paramMap.get('surveyUid') ?? ''));
    return `${survey.title} — BSDCPolls`;
  } catch {
    return 'Survey — BSDCPolls';
  }
};

const surveyResultsTitleResolver: ResolveFn<string> = async (route) => {
  const client = inject(BsdcPollsApiClient);
  try {
    const survey = await firstValueFrom(client.surveys_GetByUid(route.paramMap.get('surveyUid') ?? ''));
    return `${survey.title} Results — BSDCPolls`;
  } catch {
    return 'Survey Results — BSDCPolls';
  }
};

/** Lazy routes for the surveys feature. */
export const surveyRoutes: Routes = [
  {
    path: 'new',
    title: 'Create Survey — BSDCPolls',
    loadComponent: () => import('./builder/survey-builder.component').then((m) => m.SurveyBuilderComponent),
  },
  {
    path: ':surveyUid',
    title: surveyTitleResolver,
    loadComponent: () => import('./respondent/survey-respondent.component').then((m) => m.SurveyRespondentComponent),
  },
  {
    path: ':surveyUid/results',
    title: surveyResultsTitleResolver,
    loadComponent: () => import('./results/survey-results.component').then((m) => m.SurveyResultsComponent),
  },
];
