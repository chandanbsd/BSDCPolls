import { ErrorHandler, Injectable, inject } from '@angular/core';
import { MatSnackBar } from '@angular/material/snack-bar';

/**
 * Global Angular error handler. Catches all uncaught errors from components,
 * services, and promises, then surfaces a user-friendly snackbar message.
 * Never exposes internal error details to the UI.
 */
@Injectable()
export class GlobalErrorHandler implements ErrorHandler {
  private readonly snackBar = inject(MatSnackBar);

  handleError(error: unknown): void {
    console.error('Unhandled application error:', error);
    this.snackBar.open('Something went wrong. Please try again.', 'Dismiss', {
      duration: 5000,
    });
  }
}
