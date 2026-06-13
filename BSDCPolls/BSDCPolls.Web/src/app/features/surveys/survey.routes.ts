import { Routes } from '@angular/router';

/** Lazy routes for the surveys feature. */
export const surveyRoutes: Routes = [
  {
    path: 'new',
    loadComponent: () => import('./builder/survey-builder.component').then((m) => m.SurveyBuilderComponent),
  },
  {
    path: ':surveyUid',
    loadComponent: () => import('./respondent/survey-respondent.component').then((m) => m.SurveyRespondentComponent),
  },
  {
    path: ':surveyUid/results',
    loadComponent: () => import('./results/survey-results.component').then((m) => m.SurveyResultsComponent),
  },
];
