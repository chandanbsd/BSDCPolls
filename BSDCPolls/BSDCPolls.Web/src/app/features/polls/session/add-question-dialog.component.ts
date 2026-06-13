import { Component, inject } from '@angular/core';
import { FormArray, FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { MAT_DIALOG_DATA, MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatIconModule } from '@angular/material/icon';
import { BsdcPollsApiClient } from '../../../generated/api';
import { PollSessionStore } from '../../../store/poll-session.store';
import { firstValueFrom } from 'rxjs';

interface AddQuestionDialogData {
  pollUid: string;
}

/** Dialog for adding a question to an active poll session. */
@Component({
  selector: 'app-add-question-dialog',
  standalone: true,
  imports: [
    ReactiveFormsModule,
    MatButtonModule,
    MatCheckboxModule,
    MatDialogModule,
    MatFormFieldModule,
    MatIconModule,
    MatInputModule,
    MatProgressSpinnerModule,
  ],
  templateUrl: './add-question-dialog.component.html',
})
export class AddQuestionDialogComponent {
  private readonly fb = inject(FormBuilder);
  private readonly dialogRef = inject(MatDialogRef<AddQuestionDialogComponent>);
  readonly data: AddQuestionDialogData = inject(MAT_DIALOG_DATA);
  private readonly apiClient = inject(BsdcPollsApiClient);
  private readonly pollStore = inject(PollSessionStore);

  readonly form = this.fb.nonNullable.group({
    text: ['', [Validators.required, Validators.maxLength(500)]],
    pushImmediately: [true],
    options: this.fb.array([
      this.createOptionControl(),
      this.createOptionControl(),
    ]),
  });

  isLoading = false;
  error: string | null = null;

  get options(): FormArray {
    return this.form.get('options') as FormArray;
  }

  addOption(): void {
    if (this.options.length < 10) {
      this.options.push(this.createOptionControl());
    }
  }

  removeOption(index: number): void {
    if (this.options.length > 2) {
      this.options.removeAt(index);
    }
  }

  async onSubmit(): Promise<void> {
    if (this.form.invalid) return;
    this.isLoading = true;
    this.error = null;
    try {
      const raw = this.form.getRawValue();
      const request = {
        text: raw.text,
        pushImmediately: raw.pushImmediately,
        options: raw.options.map((text: string, i: number) => ({ text, orderIndex: i })),
      };
      const question = await firstValueFrom(
        this.apiClient.polls_AddQuestion(this.data.pollUid, request),
      );
      this.pollStore.addQuestion(question);
      if (request.pushImmediately) {
        this.pollStore.setActiveQuestion(question);
      }
      this.dialogRef.close(question);
    } catch (err: unknown) {
      this.error = err instanceof Error ? err.message : 'Failed to add question.';
    } finally {
      this.isLoading = false;
    }
  }

  cancel(): void {
    this.dialogRef.close();
  }

  private createOptionControl() {
    return this.fb.nonNullable.control('', [Validators.required, Validators.maxLength(200)]);
  }
}
