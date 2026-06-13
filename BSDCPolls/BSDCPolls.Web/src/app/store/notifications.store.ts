import { inject } from '@angular/core';
import { signalStore, withState, withMethods, patchState } from '@ngrx/signals';
import { BsdcPollsApiClient, NotificationItem } from '../generated/api';
import { firstValueFrom } from 'rxjs';

interface NotificationsState {
  items: NotificationItem[];
  unreadCount: number;
  isLoading: boolean;
  error: string | null;
}

const initialState: NotificationsState = {
  items: [],
  unreadCount: 0,
  isLoading: false,
  error: null,
};

/** NgRx Signal Store for in-app notifications and the unread badge count. */
export const NotificationsStore = signalStore(
  { providedIn: 'root' },
  withState(initialState),
  withMethods((store) => {
    const apiClient = inject(BsdcPollsApiClient);
    return {
      /** Prepends a real-time notification received via SignalR without a server round-trip. */
      addNotification(item: NotificationItem): void {
        patchState(store, {
          items: [item, ...store.items()],
          unreadCount: store.unreadCount() + 1,
        });
      },

      async loadNotifications(page = 1, pageSize = 20): Promise<void> {
        patchState(store, { isLoading: true, error: null });
        try {
          const result = await firstValueFrom(
            apiClient.notifications_GetNotifications(false, page, pageSize),
          );
          patchState(store, {
            items: result.items ?? [],
            unreadCount: result.unreadCount ?? 0,
            isLoading: false,
          });
        } catch (err: unknown) {
          patchState(store, {
            isLoading: false,
            error: err instanceof Error ? err.message : 'Failed to load notifications.',
          });
        }
      },

      async markRead(notificationUid: string): Promise<void> {
        await firstValueFrom(apiClient.notifications_MarkRead(notificationUid));
        patchState(store, {
          items: store.items().map((n) =>
            n.notificationUid === notificationUid ? { ...n, isRead: true } : n,
          ),
          unreadCount: Math.max(0, store.unreadCount() - 1),
        });
      },

      async markAllRead(): Promise<void> {
        await firstValueFrom(apiClient.notifications_MarkAllRead());
        patchState(store, {
          items: store.items().map((n) => ({ ...n, isRead: true })),
          unreadCount: 0,
        });
      },
    };
  }),
);

export type NotificationsStore = InstanceType<typeof NotificationsStore>;
