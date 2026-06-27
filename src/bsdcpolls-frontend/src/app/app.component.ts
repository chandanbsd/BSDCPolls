import { Component, inject, OnInit } from '@angular/core';
import { NavigationEnd, Router, RouterOutlet } from '@angular/router';
import { filter } from 'rxjs';
import { MatSnackBarModule } from '@angular/material/snack-bar';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { SkipLinkComponent } from './shared/skip-link/skip-link.component';
import { BottomNavComponent } from './shared/nav/bottom-nav/bottom-nav.component';
import { NavRailComponent } from './shared/nav/nav-rail/nav-rail.component';
import { NavDrawerComponent } from './shared/nav/nav-drawer/nav-drawer.component';
import { NotificationBellComponent } from './shared/notification-bell/notification-bell.component';
import { NotificationHubService } from './core/notifications/notification-hub.service';
import { AuthStore } from './store/auth.store';
import { LayoutStore } from './core/layout/layout.store';

/** Root shell — skip link, adaptive nav, main content area with focus management on route change. */
@Component({
  selector: 'app-root',
  standalone: true,
  imports: [
    RouterOutlet,
    MatSnackBarModule,
    MatIconModule,
    MatButtonModule,
    SkipLinkComponent,
    BottomNavComponent,
    NavRailComponent,
    NavDrawerComponent,
    NotificationBellComponent,
  ],
  templateUrl: './app.component.html',
  styleUrl: './app.component.scss',
})
export class AppComponent implements OnInit {
  readonly authStore = inject(AuthStore);
  protected readonly layoutStore = inject(LayoutStore);
  private readonly notificationHubService = inject(NotificationHubService);
  private readonly router = inject(Router);

  ngOnInit(): void {
    if (this.authStore.isAuthenticated()) {
      this.notificationHubService.connect();
    }

    // Move focus to <main> on every route change so screen readers announce the new page.
    this.router.events
      .pipe(filter((e): e is NavigationEnd => e instanceof NavigationEnd))
      .subscribe(() => {
        document.getElementById('main-content')?.focus();
      });
  }

  navigateToProfile(): void {
    this.router.navigate(['/profile']);
  }
}
