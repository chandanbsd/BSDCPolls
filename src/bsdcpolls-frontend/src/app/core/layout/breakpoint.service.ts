import { Injectable, Signal } from '@angular/core';
import { BreakpointObserver, Breakpoints } from '@angular/cdk/layout';
import { toSignal } from '@angular/core/rxjs-interop';
import { map } from 'rxjs';
import { Breakpoint } from './breakpoint.enum';
import { BREAKPOINT_SERVICE, IBreakpointService } from './ibreakpoint.service';

/** Maps CDK BreakpointObserver results to the Breakpoint enum signal. */
@Injectable({ providedIn: 'root' })
export class BreakpointService implements IBreakpointService {
  /** The currently active viewport classification, updated on viewport change. */
  readonly current: Signal<Breakpoint>;

  constructor(private readonly observer: BreakpointObserver) {
    const breakpoint$ = this.observer
      .observe([
        Breakpoints.XSmall,
        Breakpoints.Small,
        Breakpoints.Medium,
        Breakpoints.Large,
        Breakpoints.XLarge,
      ])
      .pipe(
        map((state) => {
          if (state.breakpoints[Breakpoints.XLarge]) return Breakpoint.XLarge;
          if (state.breakpoints[Breakpoints.Large]) return Breakpoint.Large;
          if (state.breakpoints[Breakpoints.Medium]) return Breakpoint.Medium;
          if (state.breakpoints[Breakpoints.Small]) return Breakpoint.Small;
          return Breakpoint.XSmall;
        }),
      );

    this.current = toSignal(breakpoint$, { initialValue: Breakpoint.XSmall });
  }
}

/** Provider binding for DI token. Register in appConfig providers. */
export const breakpointServiceProvider = {
  provide: BREAKPOINT_SERVICE,
  useExisting: BreakpointService,
};
