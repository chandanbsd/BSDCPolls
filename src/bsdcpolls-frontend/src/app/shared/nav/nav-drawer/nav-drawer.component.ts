import { Component, inject } from '@angular/core';
import { RouterLink, RouterLinkActive } from '@angular/router';
import { MatIconModule } from '@angular/material/icon';
import { MatListModule } from '@angular/material/list';
import { MatSidenavModule } from '@angular/material/sidenav';
import { LayoutStore } from '../../../core/layout/layout.store';

/** M3 persistent navigation drawer for lg/xl viewports. */
@Component({
  selector: 'app-nav-drawer',
  standalone: true,
  imports: [RouterLink, RouterLinkActive, MatIconModule, MatListModule, MatSidenavModule],
  templateUrl: './nav-drawer.component.html',
  styleUrl: './nav-drawer.component.scss',
})
export class NavDrawerComponent {
  protected readonly layoutStore = inject(LayoutStore);
}
