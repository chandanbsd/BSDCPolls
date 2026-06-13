import { Component, inject, OnInit } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { MatSnackBarModule } from '@angular/material/snack-bar';
import { MatToolbarModule } from '@angular/material/toolbar';
import { NotificationBellComponent } from './shared/notification-bell/notification-bell.component';
import { NotificationHubService } from './core/notifications/notification-hub.service';
import { AuthStore } from './store/auth.store';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [
    CommonModule,
    RouterOutlet,
    MatSnackBarModule,
    MatToolbarModule,
    NotificationBellComponent,
  ],
  template: `
    @if (authStore.isAuthenticated()) {
      <mat-toolbar color="primary">
        <span>BSDCPolls</span>
        <span style="flex: 1"></span>
        <app-notification-bell></app-notification-bell>
      </mat-toolbar>
    }
    <router-outlet />
  `,
})
export class AppComponent implements OnInit {
  readonly authStore = inject(AuthStore);
  private readonly notificationHubService = inject(NotificationHubService);

  ngOnInit(): void {
    if (this.authStore.isAuthenticated()) {
      this.notificationHubService.connect();
    }
  }
}
