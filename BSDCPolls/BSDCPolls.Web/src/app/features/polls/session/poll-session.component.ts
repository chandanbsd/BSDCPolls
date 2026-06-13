import { Component, OnDestroy, OnInit, inject } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { MatRadioModule } from '@angular/material/radio';
import { MatChipsModule } from '@angular/material/chips';
import { FormsModule } from '@angular/forms';
import { PollSessionStore } from '../../../store/poll-session.store';
import { PollHubService } from '../services/poll-hub.service';
import { BsdcPollsApiClient, PollStatus } from '../../../generated/api';
import { AddQuestionDialogComponent } from './add-question-dialog.component';
import { InviteUserDialogComponent } from '../../../shared/invite-user-dialog/invite-user-dialog.component';
import { firstValueFrom } from 'rxjs';

/** Displays the live poll session for both creator and participant roles. */
@Component({
  selector: 'app-poll-session',
  standalone: true,
  imports: [
    FormsModule,
    MatButtonModule,
    MatCardModule,
    MatProgressBarModule,
    MatProgressSpinnerModule,
    MatDialogModule,
    MatRadioModule,
    MatChipsModule,
  ],
  templateUrl: './poll-session.component.html',
})
export class PollSessionComponent implements OnInit, OnDestroy {
  readonly pollStore = inject(PollSessionStore);
  private readonly route = inject(ActivatedRoute);
  private readonly pollHubService = inject(PollHubService);
  private readonly apiClient = inject(BsdcPollsApiClient);
  private readonly dialog = inject(MatDialog);

  readonly PollStatus = PollStatus;
  selectedOptionUid: string | null = null;
  isActivating = false;
  isClosing = false;
  voteError: string | null = null;

  async ngOnInit(): Promise<void> {
    const pollUid = this.route.snapshot.paramMap.get('pollUid') ?? '';
    await this.pollStore.loadPoll(pollUid);
    try {
      await this.pollHubService.connect(pollUid);
    } catch {
      // Hub connection failure is non-fatal — page still shows REST-loaded data
    }
  }

  async ngOnDestroy(): Promise<void> {
    await this.pollHubService.disconnect();
  }

  async activatePoll(): Promise<void> {
    const poll = this.pollStore.poll();
    if (!poll) return;
    this.isActivating = true;
    try {
      await firstValueFrom(
        this.apiClient.polls_ChangeStatus(poll.pollUid, { status: PollStatus.Active }),
      );
      await this.pollStore.loadPoll(poll.pollUid);
    } finally {
      this.isActivating = false;
    }
  }

  async closePoll(): Promise<void> {
    const poll = this.pollStore.poll();
    if (!poll) return;
    this.isClosing = true;
    try {
      await firstValueFrom(
        this.apiClient.polls_ChangeStatus(poll.pollUid, { status: PollStatus.Closed }),
      );
      this.pollStore.closePoll();
    } finally {
      this.isClosing = false;
    }
  }

  openAddQuestionDialog(): void {
    const poll = this.pollStore.poll();
    if (!poll) return;
    this.dialog.open(AddQuestionDialogComponent, {
      data: { pollUid: poll.pollUid },
      width: '600px',
    });
  }

  openInviteDialog(): void {
    const poll = this.pollStore.poll();
    if (!poll) return;
    this.dialog.open(InviteUserDialogComponent, {
      data: { pollUid: poll.pollUid },
      width: '400px',
    });
  }

  async submitVote(): Promise<void> {
    if (!this.selectedOptionUid) return;
    const activeQuestion = this.pollStore.activeQuestion();
    if (!activeQuestion) return;
    this.voteError = null;
    try {
      await this.pollHubService.submitVote(activeQuestion.questionUid, this.selectedOptionUid);
      this.selectedOptionUid = null;
    } catch (err: unknown) {
      const message = err instanceof Error ? err.message : 'Vote failed.';
      this.voteError = message.includes('VOTE_DUPLICATE')
        ? 'You have already voted on this question.'
        : 'Failed to submit vote. Please try again.';
    }
  }

  getVotePercentage(questionUid: string, optionUid: string): number {
    const poll = this.pollStore.poll();
    if (!poll) return 0;
    // The results store is updated via VoteCountUpdated signal events
    return 0;
  }
}
