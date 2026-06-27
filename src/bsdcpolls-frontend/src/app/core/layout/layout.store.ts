import { computed, effect, inject } from '@angular/core';
import { patchState, signalStore, withComputed, withHooks, withState } from '@ngrx/signals';
import { Breakpoint } from './breakpoint.enum';
import { BreakpointService } from './breakpoint.service';

interface LayoutState {
  breakpoint: Breakpoint;
}

/** NgRX Signal Store for reactive viewport state. Driven by BreakpointService. */
export const LayoutStore = signalStore(
  { providedIn: 'root' },
  withState<LayoutState>({ breakpoint: Breakpoint.XSmall }),
  withComputed(({ breakpoint }) => ({
    /** True when phone portrait or landscape — shows bottom nav bar. */
    isXsOrSm: computed(() => breakpoint() === Breakpoint.XSmall || breakpoint() === Breakpoint.Small),
    /** True at tablet viewport — shows nav rail. */
    isMd: computed(() => breakpoint() === Breakpoint.Medium),
    /** True at desktop or large desktop — shows persistent nav drawer. */
    isLgOrXl: computed(() => breakpoint() === Breakpoint.Large || breakpoint() === Breakpoint.XLarge),
    /** Alias for isXsOrSm where "phone" is the clearer intent. */
    isPhone: computed(() => breakpoint() === Breakpoint.XSmall || breakpoint() === Breakpoint.Small),
    /** Alias for isLgOrXl. */
    isDesktop: computed(() => breakpoint() === Breakpoint.Large || breakpoint() === Breakpoint.XLarge),
    /** True only at xl — triggers max-width content constraint. */
    isXl: computed(() => breakpoint() === Breakpoint.XLarge),
  })),
  withHooks({
    onInit(store) {
      const breakpointService = inject(BreakpointService);
      // Propagate CDK BreakpointObserver changes into the NgRX store state.
      effect(
        () => {
          patchState(store, { breakpoint: breakpointService.current() });
        },
        { allowSignalWrites: true },
      );
    },
  }),
);
