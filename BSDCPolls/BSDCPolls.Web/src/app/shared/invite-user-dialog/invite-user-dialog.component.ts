import { Component, inject } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { MAT_DIALOG_DATA, MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { BsdcPollsApiClient, InvitationResponse } from '../../generated/api';
import { firstValueFrom } from 'rxjs';
import { CommonModule } from '@angular/common';

export interface InviteUserDialogData {
  pollUid?: string;
  surveyUid?: string;
}

/** Dialog for inviting a user to a poll or survey by username. */
@Component({
  selector: 'app-invite-user-dialog',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatButtonModule,
    MatDialogModule,
    MatFormFieldModule,
    MatInputModule,
    MatProgressSpinnerModule,
  ],
  templateUrl: './invite-user-dialog.component.html',
})
export class InviteUserDialogComponent {
  private readonly apiClient = inject(BsdcPollsApiClient);
  private readonly dialogRef = inject(MatDialogRef<InviteUserDialogComponent>);
  readonly data: InviteUserDialogData = inject(MAT_DIALOG_DATA);
  private readonly fb = inject(FormBuilder);

  readonly form = this.fb.group({
    targetUsername: ['', [Validators.required, Validators.maxLength(60)]],
  });

  isSubmitting = false;
  errorMessage: string | null = null;

  async submit(): Promise<void> {
    if (this.form.invalid) return;

    this.isSubmitting = true;
    this.errorMessage = null;

    const targetUsername = this.form.value.targetUsername!;

    try {
      let result: InvitationResponse;
      if (this.data.pollUid) {
        result = await firstValueFrom(
          this.apiClient.invitations_CreatePollInvitation(this.data.pollUid, { targetUsername }),
        );
      } else if (this.data.surveyUid) {
        result = await firstValueFrom(
          this.apiClient.invitations_CreateSurveyInvitation(this.data.surveyUid, { targetUsername }),
        );
      } else {
        throw new Error('No poll or survey UID provided.');
      }
      this.dialogRef.close(result);
    } catch (err: unknown) {
      this.errorMessage = err instanceof Error ? err.message : 'Invitation failed.';
      this.isSubmitting = false;
    }
  }

  cancel(): void {
    this.dialogRef.close();
  }
}
