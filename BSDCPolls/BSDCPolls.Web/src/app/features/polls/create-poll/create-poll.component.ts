import { Component, inject } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSlideToggleModule } from '@angular/material/slide-toggle';
import { BsdcPollsApiClient } from '../../../generated/api';
import { firstValueFrom } from 'rxjs';

/** Form for creating a new poll. */
@Component({
  selector: 'app-create-poll',
  standalone: true,
  imports: [
    ReactiveFormsModule,
    MatButtonModule,
    MatCardModule,
    MatFormFieldModule,
    MatInputModule,
    MatProgressSpinnerModule,
    MatSlideToggleModule,
  ],
  templateUrl: './create-poll.component.html',
})
export class CreatePollComponent {
  private readonly apiClient = inject(BsdcPollsApiClient);
  private readonly router = inject(Router);
  private readonly fb = inject(FormBuilder);

  readonly form = this.fb.nonNullable.group({
    title: ['', [Validators.required, Validators.maxLength(200)]],
    isPublic: [false],
  });

  isLoading = false;
  error: string | null = null;

  async onSubmit(): Promise<void> {
    if (this.form.invalid) return;
    this.isLoading = true;
    this.error = null;
    try {
      const poll = await firstValueFrom(
        this.apiClient.polls_Create(this.form.getRawValue()),
      );
      await this.router.navigate(['/polls', poll.pollUid]);
    } catch (err: unknown) {
      this.error = err instanceof Error ? err.message : 'Failed to create poll.';
    } finally {
      this.isLoading = false;
    }
  }
}
