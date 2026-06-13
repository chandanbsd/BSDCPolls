import { inject } from '@angular/core';
import { signalStore, withState, withMethods, patchState } from '@ngrx/signals';
import { BsdcPollsApiClient, PollFeedItem, SurveyFeedItem } from '../generated/api';
import { firstValueFrom } from 'rxjs';

interface FeedState {
  polls: PollFeedItem[];
  surveys: SurveyFeedItem[];
  pollsTotal: number;
  surveysTotal: number;
  activeTab: 'polls' | 'surveys' | 'results';
  isLoading: boolean;
  error: string | null;
}

const initialState: FeedState = {
  polls: [],
  surveys: [],
  pollsTotal: 0,
  surveysTotal: 0,
  activeTab: 'polls',
  isLoading: false,
  error: null,
};

/** NgRx Signal Store for the home feed — polls, surveys, and results tabs. */
export const FeedStore = signalStore(
  { providedIn: 'root' },
  withState(initialState),
  withMethods((store) => {
    const apiClient = inject(BsdcPollsApiClient);
    return {
      setActiveTab(tab: FeedState['activeTab']): void {
        patchState(store, { activeTab: tab });
      },

      async loadPolls(page = 1, pageSize = 20): Promise<void> {
        patchState(store, { isLoading: true, error: null });
        try {
          const result = await firstValueFrom(apiClient.polls_GetFeed(undefined, page, pageSize));
          patchState(store, {
            polls: result.items ?? [],
            pollsTotal: result.totalCount ?? 0,
            isLoading: false,
          });
        } catch (err: unknown) {
          patchState(store, {
            isLoading: false,
            error: err instanceof Error ? err.message : 'Failed to load polls.',
          });
        }
      },

      async loadSurveys(page = 1, pageSize = 20): Promise<void> {
        patchState(store, { isLoading: true, error: null });
        try {
          const result = await firstValueFrom(apiClient.surveys_GetFeed(undefined, page, pageSize));
          patchState(store, {
            surveys: result.items ?? [],
            surveysTotal: result.totalCount ?? 0,
            isLoading: false,
          });
        } catch (err: unknown) {
          patchState(store, {
            isLoading: false,
            error: err instanceof Error ? err.message : 'Failed to load surveys.',
          });
        }
      },
    };
  }),
);

export type FeedStore = InstanceType<typeof FeedStore>;
