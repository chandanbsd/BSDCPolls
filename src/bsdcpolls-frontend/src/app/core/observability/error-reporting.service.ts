import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { EMPTY, catchError } from 'rxjs';

interface ClientErrorPayload {
  message: string;
  stack?: string;
  url: string;
  timestamp: string;
}

@Injectable({ providedIn: 'root' })
export class ErrorReportingService {
  private readonly http = inject(HttpClient);

  report(error: unknown): void {
    const payload: ClientErrorPayload = {
      message: error instanceof Error ? error.message : String(error),
      stack: error instanceof Error ? error.stack : undefined,
      url: window.location.href,
      timestamp: new Date().toISOString(),
    };

    this.http
      .post('/api/client-errors', payload)
      .pipe(catchError(() => EMPTY))
      .subscribe();
  }
}
