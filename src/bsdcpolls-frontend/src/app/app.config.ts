import { ApplicationConfig, ErrorHandler, provideZoneChangeDetection } from '@angular/core';
import { provideRouter } from '@angular/router';
import { provideAnimationsAsync } from '@angular/platform-browser/animations/async';
import { provideHttpClient, withInterceptors } from '@angular/common/http';

import { routes } from './app.routes';
import { GlobalErrorHandler } from './core/global-error-handler';
import { traceparentInterceptor } from './core/traceparent.interceptor';
import { tokenInterceptor } from './core/auth/token.interceptor';
import { BsdcPollsApiClient } from './generated/api';
import { breakpointServiceProvider } from './core/layout/breakpoint.service';

export const appConfig: ApplicationConfig = {
  providers: [
    provideZoneChangeDetection({ eventCoalescing: true }),
    provideRouter(routes),
    provideAnimationsAsync(),
    provideHttpClient(withInterceptors([traceparentInterceptor, tokenInterceptor])),
    { provide: ErrorHandler, useClass: GlobalErrorHandler },
    BsdcPollsApiClient,
    breakpointServiceProvider,
  ],
};
