import { Component, inject } from '@angular/core';
import { MAT_DIALOG_DATA, MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';

interface UsernameDialogData {
  username: string;
}

@Component({
  selector: 'app-username-dialog',
  standalone: true,
  imports: [MatButtonModule, MatDialogModule],
  template: `
    <h2 mat-dialog-title>Your username</h2>
    <mat-dialog-content>
      <p>Your unique username has been generated:</p>
      <p>
        <strong>{{ data.username }}</strong>
      </p>
      <p>Save this — you will need it to log in. It cannot be recovered if lost.</p>
    </mat-dialog-content>
    <mat-dialog-actions align="end">
      <button mat-flat-button (click)="close()">Got it, continue</button>
    </mat-dialog-actions>
  `,
})
export class UsernameDialogComponent {
  readonly data: UsernameDialogData = inject(MAT_DIALOG_DATA);
  private readonly dialogRef = inject(MatDialogRef<UsernameDialogComponent>);

  close(): void {
    this.dialogRef.close();
  }
}
