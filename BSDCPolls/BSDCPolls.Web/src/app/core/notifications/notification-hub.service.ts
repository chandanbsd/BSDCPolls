import { Injectable, inject } from '@angular/core';
import { HubConnection, HubConnectionBuilder, LogLevel } from '@microsoft/signalr';
import { AuthStore } from '../../store/auth.store';
import { NotificationsStore } from '../../store/notifications.store';
import { NotificationItem } from '../../generated/api';

/**
 * Manages the SignalR connection to the BFF NotificationHub.
 * Subscribes to InvitationReceived events and updates NotificationsStore.
 */
@Injectable({ providedIn: 'root' })
export class NotificationHubService {
  private connection: HubConnection | null = null;

  private readonly authStore = inject(AuthStore);
  private readonly notificationsStore = inject(NotificationsStore);

  /** Establishes the notification hub connection for the current user. */
  async connect(): Promise<void> {
    if (this.connection) {
      return;
    }

    const token = this.authStore.accessToken() ?? '';

    this.connection = new HubConnectionBuilder()
      .withUrl(`/hubs/notifications?access_token=${token}`)
      .withAutomaticReconnect({
        nextRetryDelayInMilliseconds: () => 3000,
      })
      .configureLogging(LogLevel.Warning)
      .build();

    this.connection.on('InvitationReceived', (payload: {
      notificationUid: string;
      inviterUsername: string;
      pollUid: string | null;
      pollTitle: string | null;
      surveyUid: string | null;
      surveyTitle: string | null;
      createdOn: string;
    }) => {
      const item: NotificationItem = {
        notificationUid: payload.notificationUid,
        isRead: false,
        createdOn: payload.createdOn,
        invitation: {
          invitationUid: payload.notificationUid,
          inviterUsername: payload.inviterUsername,
          pollUid: payload.pollUid ?? undefined,
          pollTitle: payload.pollTitle ?? undefined,
          surveyUid: payload.surveyUid ?? undefined,
          surveyTitle: payload.surveyTitle ?? undefined,
        },
      };
      this.notificationsStore.addNotification(item);
    });

    await this.connection.start();
  }

  /** Disconnects from the NotificationHub. */
  async disconnect(): Promise<void> {
    if (this.connection) {
      await this.connection.stop();
      this.connection = null;
    }
  }
}
