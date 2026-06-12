import { Component, inject } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatDialogModule, MatDialog } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { AuthStore } from '../../../store/auth.store';
import { UsernameDialogComponent } from './username-dialog.component';

@Component({
  selector: 'app-register',
  standalone: true,
  imports: [
    ReactiveFormsModule,
    RouterLink,
    MatButtonModule,
    MatCardModule,
    MatDialogModule,
    MatFormFieldModule,
    MatInputModule,
    MatProgressSpinnerModule,
  ],
  templateUrl: './register.component.html',
})
export class RegisterComponent {
  private readonly authStore = inject(AuthStore);
  private readonly router = inject(Router);
  private readonly dialog = inject(MatDialog);
  private readonly fb = inject(FormBuilder);

  readonly form = this.fb.nonNullable.group({
    password: ['', [Validators.required, Validators.minLength(12)]],
  });

  get isLoading() {
    return this.authStore.isLoading();
  }

  get error() {
    return this.authStore.error();
  }

  async onSubmit(): Promise<void> {
    if (this.form.invalid) return;

    const { password } = this.form.getRawValue();
    const username = await this.authStore.register({ password });

    const dialogRef = this.dialog.open(UsernameDialogComponent, {
      disableClose: true,
      data: { username },
    });

    dialogRef.afterClosed().subscribe(() => {
      this.router.navigate(['/feed']);
    });
  }
}
