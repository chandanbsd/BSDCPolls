import { Component, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatBadgeModule } from '@angular/material/badge';
import { MatMenuModule } from '@angular/material/menu';
import { MatDividerModule } from '@angular/material/divider';
import { NotificationsStore } from '../../store/notifications.store';

/** Notification bell icon with unread badge and dropdown menu of recent notifications. */
@Component({
  selector: 'app-notification-bell',
  standalone: true,
  imports: [
    CommonModule,
    MatIconModule,
    MatButtonModule,
    MatBadgeModule,
    MatMenuModule,
    MatDividerModule,
  ],
  templateUrl: './notification-bell.component.html',
})
export class NotificationBellComponent implements OnInit {
  readonly store = inject(NotificationsStore);
  private readonly router = inject(Router);

  ngOnInit(): void {
    this.store.loadNotifications();
  }

  openNotifications(): void {
    this.store.loadNotifications();
  }

  markRead(notificationUid: string): void {
    this.store.markRead(notificationUid);
  }

  markAllRead(): void {
    this.store.markAllRead();
  }

  navigateTo(item: { invitation: { pollUid?: string; surveyUid?: string } }): void {
    if (item.invitation.pollUid) {
      this.router.navigate(['/polls', item.invitation.pollUid]);
    } else if (item.invitation.surveyUid) {
      this.router.navigate(['/surveys', item.invitation.surveyUid]);
    }
  }
}
