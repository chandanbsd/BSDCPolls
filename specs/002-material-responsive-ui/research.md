# Research: Consistent, Fully Responsive, Maximum Accessibility UI

## R-001: Angular CDK BreakpointObserver with M3 Breakpoints

**Decision**: Use `BreakpointObserver` from `@angular/cdk/layout` with Angular Material's exported `Breakpoints` constants, wrapped in a `BreakpointService` that maps `LayoutChange` events to a `Breakpoint` enum. Convert the stream to an Angular Signal via `toSignal` and surface it through `LayoutStore`.

**Rationale**: `BreakpointObserver` is the CDK-endorsed approach for reactive viewport detection. It uses `window.matchMedia` under the hood and emits only on changes (no polling). The pre-exported `Breakpoints` object (`Breakpoints.XSmall`, `Breakpoints.Small`, etc.) uses values that match Angular Material's M3 layout grid definitions exactly, ensuring the component SCSS breakpoints and the TypeScript logic share the same threshold values without duplication. `toSignal` integrates cleanly with NgRX Signal Store computed signals.

**Alternatives considered**:
- `ResizeObserver` on the root element: Fires too frequently (every resize frame); requires debouncing; does not integrate with Angular's change detection as cleanly.
- Flex Layout (`@angular/flex-layout`): Deprecated; removed from Angular ecosystem as of 2024. Not an option.
- CSS-only container queries: Cannot drive TypeScript layout logic (e.g., which nav component to render); must be paired with a TypeScript signal anyway.

**Implementation note**: `BreakpointService` must be `providedIn: 'root'` and implement `IBreakpointService`. The `Breakpoint` enum values are `XSmall | Small | Medium | Large | XLarge`. The service exposes a `current: Signal<Breakpoint>` property.

---

## R-002: Angular Material M3 Dark Mode (prefers-color-scheme)

**Decision**: Use Angular Material's `mat.theme()` mixin with two theme invocations inside `@media (prefers-color-scheme: dark)` in `styles.scss`. Both themes use the same `primary: mat.$azure-palette` but differ in `theme-type: light` vs `theme-type: dark`.

**Rationale**: Angular Material 17+ (M3 API) supports light/dark theming by applying `mat.theme()` inside a media query. This means every M3 colour role (`--mat-sys-primary`, `--mat-sys-surface`, etc.) is automatically swapped by the browser without any Angular runtime involvement — zero JavaScript overhead, instant switching, and no flash of wrong colour scheme. The `prefers-color-scheme` media query is the spec-compliant mechanism (FR-010). Dark mode is entirely handled in CSS; no `LayoutStore` dark-mode signal is required unless an in-app toggle is added in a future feature.

**Alternatives considered**:
- Class-based theming (`.dark-theme` class toggled by JavaScript): Requires a JavaScript toggle mechanism and cannot respond to OS preference without an event listener. More complex and slower than the CSS media-query approach.
- Angular Material 17 `m3.define-theme()` with `@include mat.all-component-themes($dark-theme)`: Older API pattern that duplicates the entire component theme output. The newer single `mat.theme()` call is more efficient and idiomatic for M3.

**Implementation note**: The existing `styles.scss` already has `html { @include mat.theme(...) }` for light mode. Add a `@media (prefers-color-scheme: dark) { html { @include mat.theme((..., color: (..., theme-type: dark))) } }` block immediately after.

---

## R-003: Angular Material M3 Navigation Patterns by Breakpoint

**Decision**: Implement three separate navigation components conditioned on the current `Breakpoint` signal from `LayoutStore`:

| Breakpoint | Pattern | Angular Material Component |
|---|---|---|
| `XSmall` (320–599 px) | Bottom navigation bar | Custom `mat-tab-nav-bar` or `MatTabGroup` in bottom position, or a `<nav>` with `mat-icon-button` items |
| `Small` (600–959 px) | Bottom navigation bar (same as xs) | Same component as xs |
| `Medium` (960–1279 px) | Navigation rail (icon + label, left side) | `mat-sidenav` in collapsed/icon-only mode |
| `Large` (1280–1919 px) | Full navigation drawer (persistent) | `mat-sidenav` with `mode="side"` and `opened=true` |
| `XLarge` (1920 px+) | Full navigation drawer (persistent, wider) | Same as lg, content max-width constrained |

**Rationale**: Material Design 3 mandates these exact navigation patterns per viewport class. The M3 spec document "Navigation drawer" and "Navigation rail" directly map these patterns to breakpoints. Using `mat-sidenav` for both rail and drawer is idiomatic — the rail is a narrow `mat-sidenav` with only icons visible; the drawer is the same component with labels and wider width.

**Alternatives considered**:
- Single `mat-sidenav` with all behaviour in one component: Harder to maintain, accessibility contract is murkier (sidenav has different ARIA semantics than a bottom nav bar).
- `@angular/material/navigation` third-party wrappers: Do not exist; the CDK and Material components are the right primitives.

**Implementation note**: The `AppComponent` shell renders `<app-bottom-nav>`, `<app-nav-rail>`, or `<app-nav-drawer>` based on `@if (layoutStore.isXsOrSm())`, `@if (layoutStore.isMd())`, `@if (layoutStore.isLgOrXl())` — one is always visible, others hidden. The `<main>` element must have `id="main-content"` as the skip-link target.

---

## R-004: ARIA Live Regions in Angular for Real-Time Vote Counts

**Decision**: Wrap the vote count display elements in a `<div role="status" aria-live="polite" aria-atomic="true">` live region. Use `aria-label` on the region to provide a meaningful name. For critical state changes (e.g., poll closed), use `aria-live="assertive"`.

**Rationale**: `role="status"` is equivalent to `aria-live="polite"` and is widely supported. `aria-atomic="true"` tells screen readers to announce the entire region when any part changes, preventing partial announcements of vote count strings. This directly satisfies FR-009 and SC-010 without requiring focus movement. VoiceOver (macOS) and NVDA both honour `aria-live="polite"` with `aria-atomic="true"` on elements that already exist in the DOM when the live region is established.

**Critical constraint**: Live regions **must be in the DOM before content changes**. In Angular, this means the `<div aria-live="polite">` must render unconditionally (not inside an `@if`). Use an empty string as the initial content and update it via a bound property.

**Alternatives considered**:
- `aria-live` on individual number spans: Leads to fragmented announcements ("3", "votes") instead of "3 votes for Option A".
- Angular CDK `LiveAnnouncer`: A programmatic alternative that injects announcements into a visually-hidden `aria-live` region. Appropriate when the element driving the announcement is not permanently in the DOM (e.g., toast notifications). For persistent vote counts, direct ARIA attributes on the element are preferred as they also convey current state to users who navigate with the virtual cursor.

---

## R-005: prefers-reduced-motion in Angular Material

**Decision**: Use an SCSS mixin `@mixin reduce-motion` that wraps `@media (prefers-reduced-motion: reduce)` and sets `transition-duration: 0.01ms !important; animation-duration: 0.01ms !important;` on all animated elements. Apply the mixin to all component SCSS files that introduce custom transitions. Angular Material's own animations are handled by `AnimationsModule` — import `provideAnimationsAsync()` and Angular Material respects the media query internally for M3 components.

**Rationale**: Angular Material's component animations check `prefers-reduced-motion` at the CSS level — no Angular code changes are needed for built-in Material animations. Custom CSS transitions in component SCSS are not automatically suppressed and require the explicit `@media (prefers-reduced-motion: reduce)` block. A shared SCSS mixin in `_motion.scss` avoids duplicating the media query in every component.

**Alternatives considered**:
- `ANIMATION_MODULE_TYPE` injection token to disable Angular animations entirely: Disables all Material animations regardless of user preference, which is overly broad. The spec requires suppression only when the preference is set (FR-011).
- Per-component `@HostListener('window:mediaChange')`: Fragile, requires JavaScript to mirror what CSS can do natively and more efficiently.

---

## R-006: Skip-to-Main-Content Link

**Decision**: Place `<a href="#main-content" class="skip-link">Skip to main content</a>` as the **first child of `<body>`** (i.e., the first element inside `AppComponent`'s template). Style it visually hidden by default (`position: absolute; top: -40px`) and reveal it on `:focus` (`top: 0`). The `<main>` element receives `id="main-content"` and `tabindex="-1"` so focus is accepted programmatically.

**Rationale**: WCAG 2.4.1 (Bypass Blocks, Level A) requires that keyboard users can skip repetitive navigation. Angular's router does not reset focus to the top of the document on navigation, making a skip link doubly important. The `:focus-visible` CSS variant ensures the link only shows for keyboard focus, not mouse clicks. `tabindex="-1"` on `<main>` is required for `focus()` to work when the skip link is activated — without it, programmatic focus to a non-interactive element is silently ignored in some browsers.

**Implementation note**: The `AppComponent` template prefix is the correct insertion point so the skip link is the DOM's very first focusable element regardless of which feature route is active.

---

## R-007: Touch Target Sizing (44 × 44 px minimum)

**Decision**: Use Angular Material M3's `density` configuration set to `0` (current default) which renders buttons and form controls at their M3-specified touch-friendly sizes. For any icons or links that do not automatically meet 44 × 44 px, add `min-width: 44px; min-height: 44px; display: inline-flex; align-items: center; justify-content: center;` in component SCSS with an inline comment explaining the Material primitive gap.

**Rationale**: Material Design 3 specifies a minimum touch target of 48 × 48 dp for interactive elements, which exceeds the WCAG 2.5.5 requirement of 44 × 44 CSS px. `mat-icon-button` at density 0 renders at 40 px visual size but has a 48 px touch target via transparent padding — no override needed. Custom navigation items in the bottom nav bar need explicit sizing.

**Alternatives considered**:
- `density: -1` or lower to increase visual density: Counterproductive for touch — lower density = smaller targets.
- `padding` overrides: The preferred approach when a Material component's touch area is verified to be insufficient; must be documented with a comment.

---

## R-008: Focus Management in Modal Dialogs

**Decision**: Angular Material's `MatDialog` already implements focus trapping via CDK `FocusTrap` and restores focus to the trigger element on close — no custom code required. When programmatically opening dialogs, pass the trigger element reference to `MatDialog.open()` via `MatDialogConfig.restoreFocus`.

**Rationale**: Angular Material 17+ M3 dialogs comply with ARIA 1.2 modal dialog pattern out of the box. The CDK `FocusTrap` directive traps Tab and Shift+Tab within the open dialog. `MatDialog` also handles Escape to close. The only developer obligation is to ensure dialogs are opened via `MatDialog.open()` rather than custom overlay constructions, and that `aria-labelledby` and `aria-describedby` are set on the `<mat-dialog-container>` via `MatDialogConfig.ariaLabel` or the dialog template.

---

## R-009: WCAG 2.1 AA Contrast Verification Strategy

**Decision**: Use the browser extension `axe DevTools` during development for automated checks. Before feature sign-off, run `@axe-core/cli` against the locally running app at each breakpoint. M3 colour roles (`--mat-sys-on-surface`, `--mat-sys-primary`, etc.) are designed to meet WCAG AA by default, but custom colour pairings (chart colours, chart labels) must be individually verified.

**Rationale**: M3's colour system guarantees AA contrast for standard component roles. The risk area is poll results charts (bar/segment colours) and any custom badge or status indicator colours added during implementation. axe-core catches ~57% of WCAG issues automatically; manual checks cover the rest (FR-007, SC-001).

---

## R-010: Large Desktop Max-Width Constraint (xl breakpoint)

**Decision**: Apply `max-width: 1440px; margin-inline: auto;` to the `<main>` content wrapper at the `xl` breakpoint (1920 px+). The nav drawer width is constrained to 280 px at lg and xl.

**Rationale**: Unconstrained line lengths at 1920 px+ make text unreadable (SC-001 requires ≤ 80 chars per line for body text). A `max-width` on the content container is the simplest M3-compliant solution. M3's "compact/medium/expanded" layout grid maps to a maximum content width of approximately 1440 px for expanded viewports. `margin-inline: auto` centres the content with symmetric gutters.

**Alternatives considered**:
- `clamp()` on font-size: Scales type but does not solve layout overflow — still need the max-width.
- Grid container columns with `auto` outer columns: More complex; `max-width` + `margin: auto` is equivalent and simpler.
