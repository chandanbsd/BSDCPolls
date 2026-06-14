import { Injectable, inject } from '@angular/core';
import { HubConnection, HubConnectionBuilder, LogLevel } from '@microsoft/signalr';
import { AuthStore } from '../../../store/auth.store';
import { PollSessionStore } from '../../../store/poll-session.store';
import { BsdcPollsApiClient, PollResultsOptionResponse } from '../../../generated/api';
import { firstValueFrom } from 'rxjs';

/**
 * Manages the SignalR connection to the BFF PollHub for a single poll session.
 * Subscribes to QuestionPushed, VoteCountUpdated, and PollStatusChanged events
 * and updates PollSessionStore accordingly.
 */
@Injectable({ providedIn: 'root' })
export class PollHubService {
  private connection: HubConnection | null = null;
  private currentPollUid: string | null = null;

  private readonly authStore = inject(AuthStore);
  private readonly pollSessionStore = inject(PollSessionStore);
  private readonly apiClient = inject(BsdcPollsApiClient);

  /** Connects to the PollHub for the given poll UID. */
  async connect(pollUid: string): Promise<void> {
    if (this.connection) {
      await this.disconnect();
    }

    this.currentPollUid = pollUid;
    const token = this.authStore.accessToken() ?? '';

    this.connection = new HubConnectionBuilder()
      .withUrl(`/hubs/poll?pollUid=${pollUid}&access_token=${token}`)
      .withAutomaticReconnect({
        nextRetryDelayInMilliseconds: () => 2000,
      })
      .configureLogging(LogLevel.Warning)
      .build();

    this.connection.on('QuestionPushed', (payload: { pollUid: string; question: object }) => {
      const q = payload.question as Parameters<PollSessionStore['addQuestion']>[0];
      this.pollSessionStore.addQuestion(q);
      this.pollSessionStore.setActiveQuestion(q);
    });

    this.connection.on(
      'VoteCountUpdated',
      (payload: { pollUid: string; questionUid: string; options: PollResultsOptionResponse[] }) => {
        this.pollSessionStore.updateVoteCounts(payload.questionUid, payload.options);
      },
    );

    this.connection.on('PollStatusChanged', (payload: { pollUid: string; newStatus: string }) => {
      if (payload.newStatus === 'Closed') {
        this.pollSessionStore.closePoll();
      }
    });

    this.connection.onreconnected(async () => {
      if (this.currentPollUid) {
        await this.pollSessionStore.loadPoll(this.currentPollUid);
      }
      this.pollSessionStore.setConnected(true);
    });

    this.connection.onclose(() => {
      this.pollSessionStore.setConnected(false);
    });

    await this.connection.start();
    this.pollSessionStore.setConnected(true);
  }

  /** Disconnects from the PollHub. */
  async disconnect(): Promise<void> {
    if (this.connection) {
      await this.connection.stop();
      this.connection = null;
    }
    this.currentPollUid = null;
    this.pollSessionStore.setConnected(false);
  }

  /** Submits a vote via SignalR. */
  async submitVote(questionUid: string, optionUid: string): Promise<void> {
    if (!this.connection) throw new Error('Not connected to PollHub.');
    await this.connection.invoke('SubmitVote', questionUid, optionUid);
  }
}
