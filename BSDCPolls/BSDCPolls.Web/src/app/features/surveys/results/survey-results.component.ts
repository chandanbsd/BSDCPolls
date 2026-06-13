import { Component, OnInit, inject } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { MatBadgeModule } from '@angular/material/badge';
import { MatCardModule } from '@angular/material/card';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatTableModule } from '@angular/material/table';
import { BsdcPollsApiClient, SurveyAnswerType, SurveyResultsResponse } from '../../../generated/api';
import { firstValueFrom } from 'rxjs';

/** Displays aggregated survey results for the creator. */
@Component({
  selector: 'app-survey-results',
  standalone: true,
  imports: [
    MatBadgeModule,
    MatCardModule,
    MatProgressSpinnerModule,
    MatTableModule,
  ],
  templateUrl: './survey-results.component.html',
})
export class SurveyResultsComponent implements OnInit {
  private readonly route = inject(ActivatedRoute);
  private readonly apiClient = inject(BsdcPollsApiClient);

  readonly SurveyAnswerType = SurveyAnswerType;
  readonly choiceTallyColumns = ['text', 'count'];

  results: SurveyResultsResponse | null = null;
  error: string | null = null;
  isLoading = true;

  async ngOnInit(): Promise<void> {
    const surveyUid = this.route.snapshot.paramMap.get('surveyUid') ?? '';
    try {
      this.results = await firstValueFrom(this.apiClient.surveys_GetResults(surveyUid));
    } catch (err: unknown) {
      this.error = err instanceof Error ? err.message : 'Failed to load results.';
    } finally {
      this.isLoading = false;
    }
  }
}
