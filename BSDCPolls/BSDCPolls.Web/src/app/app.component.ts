import { Component, inject, OnInit } from '@angular/core';
import { Router, RouterOutlet } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatSnackBarModule } from '@angular/material/snack-bar';
import { MatToolbarModule } from '@angular/material/toolbar';
import { NotificationBellComponent } from './shared/notification-bell/notification-bell.component';
import { NotificationHubService } from './core/notifications/notification-hub.service';
import { AuthStore } from './store/auth.store';
import { CommonModule } from '@angular/common';

/** Root shell component with app bar, notification bell, and user avatar navigation. */
@Component({
  selector: 'app-root',
  standalone: true,
  imports: [
    CommonModule,
    RouterOutlet,
    MatButtonModule,
    MatIconModule,
    MatSnackBarModule,
    MatToolbarModule,
    NotificationBellComponent,
  ],
  template: `
    @if (authStore.isAuthenticated()) {
      <mat-toolbar color="primary">
        <span>BSDCPolls</span>
        <span style="flex: 1 1 auto;"></span>
        <app-notification-bell></app-notification-bell>
        <button mat-icon-button (click)="navigateToProfile()" [attr.aria-label]="'Profile: ' + authStore.username()">
          <mat-icon>account_circle</mat-icon>
        </button>
      </mat-toolbar>
    }
    <router-outlet />
  `,
})
export class AppComponent implements OnInit {
  readonly authStore = inject(AuthStore);
  private readonly notificationHubService = inject(NotificationHubService);
  private readonly router = inject(Router);

  ngOnInit(): void {
    if (this.authStore.isAuthenticated()) {
      this.notificationHubService.connect();
    }
  }

  navigateToProfile(): void {
    this.router.navigate(['/profile']);
  }
}
