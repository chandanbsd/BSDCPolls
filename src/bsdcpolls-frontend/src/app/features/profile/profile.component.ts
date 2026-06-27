import { Component, inject, OnInit, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { HttpClient } from '@angular/common/http';
import { firstValueFrom } from 'rxjs';
import { CommonModule } from '@angular/common';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatChipsModule } from '@angular/material/chips';
import { MatDialogModule } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatRadioModule } from '@angular/material/radio';
import { MatSlideToggleModule } from '@angular/material/slide-toggle';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { AuthStore } from '../../store/auth.store';

/** Invite permission enum values matching the server contract. */
export enum InvitePermission {
  Everyone = 0,
  AllowlistOnly = 1,
  Nobody = 2,
}

interface AllowlistEntry {
  userUid: string;
  username: string;
}

interface PrivacySettingsResponse {
  showPublicContent: boolean;
  invitePermission: InvitePermission;
  allowlist: AllowlistEntry[];
}

/** Profile page with username display, username change, and privacy controls. */
@Component({
  selector: 'app-profile',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatButtonModule,
    MatCardModule,
    MatChipsModule,
    MatDialogModule,
    MatFormFieldModule,
    MatIconModule,
    MatInputModule,
    MatProgressSpinnerModule,
    MatRadioModule,
    MatSlideToggleModule,
    MatSnackBarModule,
  ],
  templateUrl: './profile.component.html',
  styleUrl: './profile.component.scss',
})
export class ProfileComponent implements OnInit {
  readonly authStore = inject(AuthStore);
  private readonly http = inject(HttpClient);
  private readonly snackBar = inject(MatSnackBar);
  private readonly fb = inject(FormBuilder);

  readonly InvitePermission = InvitePermission;
  readonly isLoading = signal(false);
  readonly isSaving = signal(false);
  readonly isChangingUsername = signal(false);
  readonly privacySettings = signal<PrivacySettingsResponse | null>(null);
  readonly allowlistUsernameInput = this.fb.nonNullable.control('', [Validators.required, Validators.maxLength(60)]);

  async ngOnInit(): Promise<void> {
    await this.loadPrivacySettings();
  }

  async loadPrivacySettings(): Promise<void> {
    this.isLoading.set(true);
    try {
      const settings = await firstValueFrom(this.http.get<PrivacySettingsResponse>('/api/users/me/privacy'));
      this.privacySettings.set(settings);
    } catch {
      this.snackBar.open('Failed to load privacy settings.', 'Close', { duration: 3000 });
    } finally {
      this.isLoading.set(false);
    }
  }

  async changeUsername(): Promise<void> {
    this.isChangingUsername.set(true);
    try {
      const result = await firstValueFrom(this.http.post<{ newUsername: string }>('/api/users/me/username/change', {}));
      this.snackBar.open(
        `Your new username is: ${result.newUsername}. Please save it — you will need it to log in.`,
        'Got it',
        { duration: 12000 },
      );
    } catch {
      this.snackBar.open('Failed to change username. Try again later.', 'Close', { duration: 3000 });
    } finally {
      this.isChangingUsername.set(false);
    }
  }

  async updateShowPublicContent(value: boolean): Promise<void> {
    const current = this.privacySettings();
    if (!current) return;
    await this.savePrivacySettings({ ...current, showPublicContent: value });
  }

  async updateInvitePermission(permission: InvitePermission): Promise<void> {
    const current = this.privacySettings();
    if (!current) return;
    await this.savePrivacySettings({ ...current, invitePermission: permission });
  }

  async addAllowlistEntry(): Promise<void> {
    if (this.allowlistUsernameInput.invalid) return;

    const targetUsername = this.allowlistUsernameInput.value;
    this.isSaving.set(true);
    try {
      const entry = await firstValueFrom(
        this.http.post<AllowlistEntry>('/api/users/me/privacy/allowlist', { username: targetUsername }),
      );
      const current = this.privacySettings();
      if (current) {
        const alreadyPresent = current.allowlist.some((e) => e.userUid === entry.userUid);
        if (!alreadyPresent) {
          this.privacySettings.set({ ...current, allowlist: [...current.allowlist, entry] });
        }
      }
      this.allowlistUsernameInput.reset();
    } catch {
      this.snackBar.open(`User "${targetUsername}" not found or already on your allowlist.`, 'Close', {
        duration: 3000,
      });
    } finally {
      this.isSaving.set(false);
    }
  }

  async removeAllowlistEntry(userUid: string): Promise<void> {
    this.isSaving.set(true);
    try {
      await firstValueFrom(this.http.delete(`/api/users/me/privacy/allowlist/${userUid}`));
      const current = this.privacySettings();
      if (current) {
        this.privacySettings.set({
          ...current,
          allowlist: current.allowlist.filter((e) => e.userUid !== userUid),
        });
      }
    } catch {
      this.snackBar.open('Failed to remove allowlist entry.', 'Close', { duration: 3000 });
    } finally {
      this.isSaving.set(false);
    }
  }

  private async savePrivacySettings(settings: PrivacySettingsResponse): Promise<void> {
    this.isSaving.set(true);
    try {
      const updated = await firstValueFrom(
        this.http.put<PrivacySettingsResponse>('/api/users/me/privacy', {
          showPublicContent: settings.showPublicContent,
          invitePermission: settings.invitePermission,
        }),
      );
      this.privacySettings.set(updated);
    } catch {
      this.snackBar.open('Failed to save privacy settings.', 'Close', { duration: 3000 });
    } finally {
      this.isSaving.set(false);
    }
  }
}
