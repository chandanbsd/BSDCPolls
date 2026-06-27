import { InjectionToken, Signal } from '@angular/core';
import { Breakpoint } from './breakpoint.enum';

/** Contract for the BreakpointObserver wrapper service. */
export interface IBreakpointService {
  /** The currently active viewport classification. */
  readonly current: Signal<Breakpoint>;
}

/** DI token used to inject IBreakpointService without a concrete class dependency. */
export const BREAKPOINT_SERVICE = new InjectionToken<IBreakpointService>('IBreakpointService');
