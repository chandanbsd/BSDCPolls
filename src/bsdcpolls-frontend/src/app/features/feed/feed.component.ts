import { Component, inject, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { MatTabsModule } from '@angular/material/tabs';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatChipsModule } from '@angular/material/chips';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatIconModule } from '@angular/material/icon';
import { FeedStore } from '../../store/feed.store';
import { PollStatus, SurveyStatus } from '../../generated/api';

/** Home feed with tabs for Polls, Surveys, and Results. */
@Component({
  selector: 'app-feed',
  standalone: true,
  imports: [
    MatTabsModule,
    MatCardModule,
    MatButtonModule,
    MatChipsModule,
    MatProgressSpinnerModule,
    MatIconModule,
  ],
  templateUrl: './feed.component.html',
  styleUrl: './feed.component.scss',
})
export class FeedComponent implements OnInit {
  readonly store = inject(FeedStore);
  private readonly router = inject(Router);

  readonly PollStatus = PollStatus;
  readonly SurveyStatus = SurveyStatus;

  ngOnInit(): void {
    this.store.loadPolls();
    this.store.loadSurveys();
  }

  onTabChange(index: number): void {
    const tabs: Array<'polls' | 'surveys' | 'results'> = ['polls', 'surveys', 'results'];
    this.store.setActiveTab(tabs[index]);
  }

  openPoll(pollUid: string): void {
    this.router.navigate(['/polls', pollUid]);
  }

  openSurvey(surveyUid: string): void {
    this.router.navigate(['/surveys', surveyUid]);
  }

  createPoll(): void {
    this.router.navigate(['/polls', 'new']);
  }

  createSurvey(): void {
    this.router.navigate(['/surveys', 'new']);
  }
}
