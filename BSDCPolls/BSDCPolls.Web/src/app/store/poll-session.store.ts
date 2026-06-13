import { inject } from '@angular/core';
import { signalStore, withState, withMethods, patchState } from '@ngrx/signals';
import { BsdcPollsApiClient, PollDetailResponse, PollQuestionResponse, PollResultsOptionResponse, PollStatus } from '../generated/api';
import { firstValueFrom } from 'rxjs';

interface PollSessionState {
  poll: PollDetailResponse | null;
  activeQuestion: PollQuestionResponse | null;
  isCreator: boolean;
  isConnected: boolean;
  error: string | null;
}

const initialState: PollSessionState = {
  poll: null,
  activeQuestion: null,
  isCreator: false,
  isConnected: false,
  error: null,
};

/**
 * NgRx Signal Store for an active poll session.
 * Holds real-time state updated by both REST calls and SignalR events.
 */
export const PollSessionStore = signalStore(
  { providedIn: 'root' },
  withState(initialState),
  withMethods((store) => {
    const apiClient = inject(BsdcPollsApiClient);
    return {
      /** Loads poll details from the BFF and sets creator flag. */
      async loadPoll(pollUid: string): Promise<void> {
        patchState(store, { error: null });
        try {
          const poll = await firstValueFrom(apiClient.polls_GetByUid(pollUid));
          patchState(store, {
            poll,
            isCreator: poll.isCreator ?? false,
            activeQuestion: null,
          });
        } catch (err: unknown) {
          patchState(store, { error: err instanceof Error ? err.message : 'Failed to load poll.' });
        }
      },

      /** Sets the currently active question (most recently pushed). */
      setActiveQuestion(question: PollQuestionResponse | null): void {
        patchState(store, { activeQuestion: question });
      },

      /** Appends a newly created question to the poll's question list. */
      addQuestion(question: PollQuestionResponse): void {
        const poll = store.poll();
        if (!poll) return;
        patchState(store, {
          poll: { ...poll, questions: [...poll.questions, question] },
        });
      },

      /** Updates the options array of a specific question with new vote counts. */
      updateVoteCounts(questionUid: string, options: PollResultsOptionResponse[]): void {
        const poll = store.poll();
        if (!poll) return;
        const updatedQuestions = poll.questions.map((q) =>
          q.questionUid === questionUid
            ? { ...q, options: options.map((o) => ({ optionUid: o.optionUid, text: o.text, orderIndex: 0 })) }
            : q,
        );
        patchState(store, { poll: { ...poll, questions: updatedQuestions } });
      },

      /** Marks the poll as closed. */
      closePoll(): void {
        const poll = store.poll();
        if (!poll) return;
        patchState(store, { poll: { ...poll, status: PollStatus.Closed } });
      },

      /** Updates the SignalR connection state. */
      setConnected(connected: boolean): void {
        patchState(store, { isConnected: connected });
      },
    };
  }),
);

export type PollSessionStore = InstanceType<typeof PollSessionStore>;
