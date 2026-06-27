# Tasks: Consistent, Fully Responsive, Maximum Accessibility UI

**Input**: Design documents from `specs/002-material-responsive-ui/`

**Prerequisites**: [plan.md](plan.md) · [spec.md](spec.md) · [research.md](research.md) · [data-model.md](data-model.md) · [contracts/](contracts/)

**Tests**: No test tasks — Principle XV prohibits all test projects.

**Organization**: Tasks are grouped by user story to enable independent implementation and testing of each story. No user story has a database dependency — this is a pure Angular frontend feature.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no shared dependency in flight)
- **[Story]**: Which user story this task belongs to
- All paths relative to repo root

---

## Phase 1: Setup (SCSS Infrastructure + Layout Primitives)

**Purpose**: Create the shared SCSS partials, design token constants, and the reactive layout service that every subsequent phase depends on.

- [X] T001 Create `src/bsdcpolls-frontend/src/styles/` directory and `_tokens.scss` with spacing scale (`$space-1` through `$space-16`), content-width constants (`$content-max-width-xl`, `$nav-drawer-width-lg`, `$nav-drawer-width-xl`, `$nav-rail-width`, `$bottom-nav-height`), and `.visually-hidden` utility class per [contracts/design-tokens.md](contracts/design-tokens.md)
- [X] T002 [P] Create `src/bsdcpolls-frontend/src/styles/_breakpoints.scss` with named breakpoint mixins (`xs`, `sm`, `md`, `lg`, `xl`, `xs-sm`, `md-up`, `lg-up`) aligned to CDK `Breakpoints.*` thresholds per [contracts/design-tokens.md](contracts/design-tokens.md)
- [X] T003 [P] Create `src/bsdcpolls-frontend/src/styles/_motion.scss` with `reduce-motion` and `motion-safe-transition` SCSS mixins per [research.md](research.md) R-005
- [X] T004 Create `src/bsdcpolls-frontend/src/app/core/layout/breakpoint.enum.ts` with `Breakpoint` enum (`XSmall | Small | Medium | Large | XLarge`) matching CDK Breakpoints values per [data-model.md](data-model.md) Section 1
- [X] T005 [P] Create `src/bsdcpolls-frontend/src/app/core/layout/ibreakpoint.service.ts` with `IBreakpointService` interface exposing `current: Signal<Breakpoint>` per [data-model.md](data-model.md) Section 1

---

## Phase 2: Foundational (App Shell + Navigation — BLOCKS All User Stories)

**Purpose**: The adaptive navigation shell and routing infrastructure must be in place before any screen-level responsive work can begin.

**⚠️ CRITICAL**: No user story implementation can start until this phase is complete.

- [X] T006 Implement `BreakpointService` in `src/bsdcpolls-frontend/src/app/core/layout/breakpoint.service.ts` — implements `IBreakpointService`, injects `BreakpointObserver`, watches all five `Breakpoints.*` constants, maps results to `Breakpoint` enum, exposes `current: Signal<Breakpoint>` via `toSignal`; register as `providedIn: 'root'` bound to `IBreakpointService` token
- [X] T007 Create `LayoutStore` NgRX Signal Store in `src/bsdcpolls-frontend/src/app/core/layout/layout.store.ts` — state: `{ breakpoint: Breakpoint }`, computed signals: `isXsOrSm`, `isMd`, `isLgOrXl`, `isPhone`, `isDesktop`; initialised from `BreakpointService.current` signal via `withHooks` per [data-model.md](data-model.md) Section 1
- [X] T008 [P] Create `SkipLinkComponent` in `src/bsdcpolls-frontend/src/app/shared/skip-link/skip-link.component.ts` — renders `<a href="#main-content">Skip to main content</a>`, standalone component, no inputs; add companion `skip-link.component.scss` using `.skip-link` class from [contracts/accessibility.md](contracts/accessibility.md) Section 1a
- [X] T009 Create `BottomNavComponent` in `src/bsdcpolls-frontend/src/app/shared/nav/bottom-nav/bottom-nav.component.ts` — fixed-bottom `<nav aria-label="Main navigation">` with four `<a mat-button>` items (Feed, Polls, Surveys, Profile), `routerLinkActive="active"`, `aria-current="page"` binding, M3 active indicator pill; add `bottom-nav.component.scss` per [contracts/navigation-layout.md](contracts/navigation-layout.md) XSmall/Small spec; min 44×44 px touch targets; icons `aria-hidden="true"`
- [X] T010 Create `NavRailComponent` in `src/bsdcpolls-frontend/src/app/shared/nav/nav-rail/nav-rail.component.ts` — wraps `mat-sidenav-container` + `mat-sidenav mode="side" opened` at 80 px width with vertical icon+label nav items, `<ng-content>` for page content; add `nav-rail.component.scss` per [contracts/navigation-layout.md](contracts/navigation-layout.md) Medium spec
- [X] T011 Create `NavDrawerComponent` in `src/bsdcpolls-frontend/src/app/shared/nav/nav-drawer/nav-drawer.component.ts` — wraps `mat-sidenav-container` + persistent `mat-sidenav` using `mat-list-item` nav items, drawer header with app name in `title-large` token, `<ng-content>` for page content; add `nav-drawer.component.scss` per [contracts/navigation-layout.md](contracts/navigation-layout.md) Large/XLarge spec
- [X] T012 Refactor `src/bsdcpolls-frontend/src/app/app.component.ts` — remove existing `mat-toolbar`, inline `style="flex: 1 1 auto;"` (PROHIBITED inline style), and current static template; replace with: (1) `SkipLinkComponent` as first child, (2) `@if (layoutStore.isXsOrSm())` → `BottomNavComponent` + `<main id="main-content" tabindex="-1">`, (3) `@if (layoutStore.isMd())` → `NavRailComponent` wrapping main, (4) `@if (layoutStore.isLgOrXl())` → `NavDrawerComponent` wrapping main; add `NavigationEnd` listener to focus `#main-content` and retain `NotificationHubService.connect()` call per [contracts/navigation-layout.md](contracts/navigation-layout.md) AppComponent Shell spec
- [X] T013 Update `src/bsdcpolls-frontend/src/styles.scss` — forward `@use` for `_tokens`, `_breakpoints`, `_motion` partials; apply `html, body` background and colour M3 tokens; add `.skip-link` ruleset with `position: absolute; top: -40px` default + `:focus-visible { top: $space-2 }` reveal per [contracts/design-tokens.md](contracts/design-tokens.md); remove any remaining hardcoded `margin: 0` in favour of M3 reset

**Checkpoint**: App shell renders correctly at all breakpoints — bottom nav at xs/sm, rail at md, drawer at lg/xl. Skip link appears on first Tab. Route navigation changes `<main>` focus.

---

## Phase 3: User Story 1 — Access any screen on a phone (Priority: P1) 🎯 MVP

**Goal**: Every screen is fully usable at 320–599 px (xs) and 600–959 px (sm) with no horizontal scrolling, no clipped content, and touch targets ≥ 44×44 px.

**Independent Test**: DevTools responsive mode at 320 px → navigate through login → feed → open poll → cast vote → view results; no horizontal scrollbar; all buttons reachable with one thumb; `axe` reports zero violations.

- [X] T014 [P] [US1] Make login screen responsive at xs/sm in `src/bsdcpolls-frontend/src/app/features/auth/login/` — centre a single `mat-card` at full-width on xs with `$space-4` horizontal padding; constrain card to 480 px on sm+; ensure form fields stack vertically; add `login.component.scss` using `@include bp.xs` and `@include bp.sm` mixins; ensure `mat-label` on every field (no placeholder-only per [contracts/accessibility.md](contracts/accessibility.md) 3a)
- [X] T015 [P] [US1] Make register screen responsive at xs/sm in `src/bsdcpolls-frontend/src/app/features/auth/register/` — same card pattern as login; all form fields full-width stacked; submit button full-width on xs; add `register.component.scss`
- [X] T016 [P] [US1] Make feed responsive at xs/sm in `src/bsdcpolls-frontend/src/app/features/feed/` — single-column card list layout; `padding-bottom: $bottom-nav-height` so content is not occluded by bottom nav bar; cards use `mat-card` with M3 surface tokens; add/update `feed.component.scss`
- [X] T017 [P] [US1] Make create-poll form responsive at xs/sm in `src/bsdcpolls-frontend/src/app/features/polls/create-poll/` — all sections stacked vertically; inputs full-width; floating action button or sticky submit footer ≥ 44 px; add/update `create-poll.component.scss`
- [X] T018 [P] [US1] Make poll session responsive at xs/sm in `src/bsdcpolls-frontend/src/app/features/polls/session/` — vote options stacked vertically above live results; submit button full-width with min-height 44 px; radio buttons/cards ≥ 44 px touch target; add/update `session.component.scss`
- [X] T019 [P] [US1] Make survey builder responsive at xs/sm in `src/bsdcpolls-frontend/src/app/features/surveys/builder/` — accordion-style question list (single column); add/delete question buttons ≥ 44 px; add/update `builder.component.scss`
- [X] T020 [P] [US1] Make survey respondent responsive at xs/sm in `src/bsdcpolls-frontend/src/app/features/surveys/respondent/` — single-column question list; full-width submit button ≥ 44 px; add/update `respondent.component.scss`
- [X] T021 [P] [US1] Make survey results responsive at xs/sm in `src/bsdcpolls-frontend/src/app/features/surveys/results/` — chart stacked above metadata; chart fills full width; add/update `results.component.scss`
- [X] T022 [P] [US1] Make profile screen responsive at xs/sm in `src/bsdcpolls-frontend/src/app/features/profile/` — single-column layout; avatar centred; edit action button ≥ 44 px; add/update `profile.component.scss`

**Checkpoint**: All 9 screens pass manual verification at 320 px per SC-006 checklist in [quickstart.md](quickstart.md). Bottom nav visible and not obscuring content.

---

## Phase 4: User Story 2 — Use the app with keyboard only (Priority: P1)

**Goal**: Every interactive element on every screen is reachable via Tab and operable via Enter/Space. Focus indicator always visible. Focus trapped in dialogs.

**Independent Test**: Load any page, disconnect mouse, Tab through all elements → every element shows a focus ring; pressing Enter/Space on any control triggers its action; Escape closes any open dialog and returns focus to trigger.

- [X] T023 [US2] Audit and fix keyboard focus indicators globally — in `src/bsdcpolls-frontend/src/styles.scss` add `:focus-visible` outline rule for `<a>` and any custom focusable elements not covered by M3; ensure `outline: 2px solid var(--mat-sys-primary); outline-offset: 2px` per [contracts/accessibility.md](contracts/accessibility.md) 4b; verify 3:1 contrast in both light and dark modes
- [X] T024 [P] [US2] Fix keyboard operability of `BottomNavComponent` — verify each nav item is in natural Tab order, `Enter` navigates route, no custom `keydown` handler is needed; ensure no `tabindex=-1` accidentally applied; update `src/bsdcpolls-frontend/src/app/shared/nav/bottom-nav/bottom-nav.component.ts` if required
- [X] T025 [P] [US2] Fix keyboard operability of `NavRailComponent` and `NavDrawerComponent` — confirm `mat-sidenav` does not steal or break Tab flow into `<mat-sidenav-content>`; confirm nav items Tab correctly; update `src/bsdcpolls-frontend/src/app/shared/nav/nav-rail/nav-rail.component.ts` and `nav-drawer.component.ts` if required
- [X] T026 [P] [US2] Verify and fix keyboard operability of all poll forms — in `src/bsdcpolls-frontend/src/app/features/polls/create-poll/` and `session/`: ensure `mat-radio-group` uses arrow keys between options; `mat-select` opens with Enter/Space and navigates with arrow keys; no `mousedown`-only handlers
- [X] T027 [P] [US2] Verify and fix keyboard operability of survey builder and respondent forms — in `src/bsdcpolls-frontend/src/app/features/surveys/builder/` and `respondent/`: all drag-handle alternatives provided as keyboard actions (add-above/add-below buttons); `mat-checkbox` and `mat-radio` keyboard operable
- [X] T028 [US2] Audit all `MatDialog` usages across the app — add `restoreFocus: true` and `autoFocus: 'first-tabbable'` to every `MatDialog.open()` call; ensure `ariaLabel` is set on dialog config; add `mat-dialog-close` on cancel button and `(click)="dialogRef.close()"` pattern for programmatic closes per [contracts/accessibility.md](contracts/accessibility.md) Section 6
- [X] T029 [P] [US2] Add `aria-disabled="true"` and visual disabled styling to all disabled controls — audit all templates for `[disabled]="..."` bindings; verify Angular Material applies `aria-disabled`; for any non-Material disabled element add `aria-disabled` explicitly per [contracts/accessibility.md](contracts/accessibility.md) 4a

**Checkpoint**: Tab through entire poll lifecycle (login → feed → poll session → vote → results) without mouse. Every step reachable and operable via keyboard. SC-002 flow completes in under 5 minutes.

---

## Phase 5: User Story 3 — Use the app with a screen reader (Priority: P1)

**Goal**: Screen reader users can complete the full poll participation journey with all state changes announced, all controls labelled, and all forms associated with their errors.

**Independent Test**: Enable VoiceOver (macOS) or NVDA (Windows) → complete login → find a poll → cast a vote → hear vote confirmation → observe live count update announced without moving focus.

- [X] T030 [US3] Wire Angular `Title` service for every route — update `src/bsdcpolls-frontend/src/app/app.routes.ts` to use Angular 17+ `title` property on each route definition (e.g. `title: 'Feed — BSDCPolls'`); for dynamic titles (poll name, survey name) implement `ResolveFn<string>` in the relevant feature route files per [contracts/accessibility.md](contracts/accessibility.md) 1b title table
- [X] T031 [P] [US3] Implement heading hierarchy on every screen — audit each feature component template; add `<h1>` matching page title on every route; add `<h2>` for major sections; add `<h3>` for card/item headings; no skipped levels per [contracts/accessibility.md](contracts/accessibility.md) Section 2
- [X] T032 [P] [US3] Add aria-labels to all icon-only interactive elements — audit `app.component.ts` (notification bell button, profile button), `bottom-nav`, `nav-rail`, `nav-drawer` for any `mat-icon-button` without visible text; add `aria-label="[purpose]"` and `aria-hidden="true"` on child `mat-icon`; include `[attr.aria-label]` for dynamic values per [contracts/accessibility.md](contracts/accessibility.md) 7a
- [X] T033 [P] [US3] Verify `mat-label` on every `mat-form-field` across all feature forms — search all templates for `mat-form-field` lacking `mat-label`; add labels to any found; convert any placeholder-only inputs to `mat-label` pattern; ensure `mat-error` present for validatable fields per [contracts/accessibility.md](contracts/accessibility.md) 3a
- [X] T034 [US3] Implement ARIA live regions for vote counts in `src/bsdcpolls-frontend/src/app/features/polls/session/` — add unconditional `<div role="status" aria-live="polite" aria-atomic="true" aria-label="Live vote results">` in poll session template; bind option text + vote count inside; region must exist in DOM before first data arrives per [contracts/accessibility.md](contracts/accessibility.md) 5a and [data-model.md](data-model.md) Section 5
- [X] T035 [P] [US3] Add poll status change live region to `src/bsdcpolls-frontend/src/app/features/polls/session/` — add `<div role="alert" aria-live="assertive" aria-atomic="true">{{ pollStatusMessage() }}</div>` bound to a `Signal<string | null>` that emits when poll status changes to Closed; clear signal after 5 seconds per [contracts/accessibility.md](contracts/accessibility.md) 5b
- [X] T036 [P] [US3] Add form submission confirmation live regions to auth and voting forms — add `<div role="status" aria-live="polite" class="visually-hidden">{{ confirmationMessage() }}</div>` to login (`src/app/features/auth/login/`), register (`register/`), poll session vote submission (`polls/session/`), and survey respondent submit (`surveys/respondent/`); signal auto-clears after 4 seconds per [contracts/accessibility.md](contracts/accessibility.md) 5c
- [X] T037 [US3] Add accessible description to survey results chart in `src/bsdcpolls-frontend/src/app/features/surveys/results/` — if chart is SVG/Canvas: add visually-hidden `<table>` with same data as fallback; if Angular Material progress bars used: ensure each has `aria-label` or `aria-labelledby` referencing the option label per [contracts/accessibility.md](contracts/accessibility.md) 7a

**Checkpoint**: VoiceOver/NVDA journey: login → feed → poll session → vote → live result update announced without focus movement → SC-003 and SC-010 pass.

---

## Phase 6: User Story 6 — View and interact on a tablet (Priority: P2)

**Goal**: Layouts at 960–1279 px (md) use available space appropriately — 2-column grids, side-by-side panels, nav rail — with adequate touch targets.

**Independent Test**: DevTools responsive mode at 768 px → feed shows 2-column grid; nav rail visible on left (80 px); poll session has side-by-side layout; no horizontal scroll; axe reports zero violations.

- [X] T038 [P] [US6] Update feed to 2-column grid at md in `src/bsdcpolls-frontend/src/app/features/feed/feed.component.scss` — add `@include bp.md { display: grid; grid-template-columns: repeat(2, 1fr); gap: $space-6; }` inside feed grid container
- [X] T039 [P] [US6] Update poll session to side-by-side at md in `src/bsdcpolls-frontend/src/app/features/polls/session/session.component.scss` — vote options column left, live results column right using CSS Grid `grid-template-columns: 1fr 1fr`; gap: `$space-8`; add `@include bp.md-up` guard
- [X] T040 [P] [US6] Update survey builder to split-pane at md in `src/bsdcpolls-frontend/src/app/features/surveys/builder/builder.component.scss` — question list panel left (fixed ~280 px), editor panel right (flex-grow: 1); add `@include bp.md-up` guard
- [X] T041 [P] [US6] Update survey results to side-by-side at md in `src/bsdcpolls-frontend/src/app/features/surveys/results/results.component.scss` — chart left (60%), metadata right (40%); add `@include bp.md-up` guard
- [X] T042 [P] [US6] Update create-poll to two-column form at md in `src/bsdcpolls-frontend/src/app/features/polls/create-poll/create-poll.component.scss` — primary fields left column, secondary options right column; `@include bp.md-up` guard; max-width 800 px on form container
- [X] T043 [P] [US6] Update profile to two-column at md in `src/bsdcpolls-frontend/src/app/features/profile/profile.component.scss` — avatar + name left column, detail fields right column; `@include bp.md-up` guard

**Checkpoint**: 768 px viewport — poll list shows 2 columns, nav rail at left, poll session side-by-side, all touch targets ≥ 44 px.

---

## Phase 7: User Story 4 — View the app on a large desktop monitor (Priority: P2)

**Goal**: Layouts at 1280 px+ use space productively — 3-column feed grid, wide panels, nav drawer — and line length stays ≤ ~80 characters for body text. Content is max 1440 px wide at xl (1920 px+).

**Independent Test**: DevTools responsive mode at 1920 px → feed shows 3-column grid; nav drawer visible (256 px); content centred within 1440 px max-width; no element stretched edge-to-edge; axe zero violations.

- [X] T044 [US4] Apply xl content max-width constraint in `src/bsdcpolls-frontend/src/app/app.component.scss` — add `@include bp.xl { .content--xl-constrained { max-width: $content-max-width-xl; margin-inline: auto; } }` on the `<main>` wrapper; update AppComponent template to apply class `content--xl-constrained` on main elements per [contracts/navigation-layout.md](contracts/navigation-layout.md) XLarge spec
- [X] T045 [P] [US4] Update feed to 3-column grid at lg/xl in `src/bsdcpolls-frontend/src/app/features/feed/feed.component.scss` — add `@include bp.lg-up { grid-template-columns: repeat(3, 1fr); }`
- [X] T046 [P] [US4] Update poll session to wide layout at lg/xl in `src/bsdcpolls-frontend/src/app/features/polls/session/session.component.scss` — increase vote options panel width; add max-width 1000 px on session container; `@include bp.lg-up` guard
- [X] T047 [P] [US4] Update survey builder to wide split at lg/xl in `src/bsdcpolls-frontend/src/app/features/surveys/builder/builder.component.scss` — widen editor panel; question list expands to 320 px; `@include bp.lg-up` guard
- [X] T048 [P] [US4] Update survey results to wide layout at lg/xl in `src/bsdcpolls-frontend/src/app/features/surveys/results/results.component.scss` — chart 65%, metadata 35%; max-width 1200 px on results container; `@include bp.lg-up` guard
- [X] T049 [P] [US4] Update profile to full two-column wide layout at lg/xl in `src/bsdcpolls-frontend/src/app/features/profile/profile.component.scss` — avatar panel left 30%, details panel right 70%; max-width 900 px; `@include bp.lg-up` guard

**Checkpoint**: 1920 px viewport — feed 3-column grid; nav drawer open (280 px); content centred with equal gutters; body text ≤ ~80 chars wide; SC-001 zero axe violations.

---

## Phase 8: User Story 5 — Switch between dark mode and light mode (Priority: P2)

**Goal**: `@media (prefers-color-scheme: dark)` activates a fully coherent dark theme. No hardcoded colours appear; all role pairs meet WCAG AA.

**Independent Test**: Toggle OS dark mode → reload app → entire UI switches to dark theme with no light-coloured artefacts; axe dark-mode audit shows zero colour contrast violations.

- [X] T050 [US5] Add dark theme block to `src/bsdcpolls-frontend/src/styles.scss` — add `@media (prefers-color-scheme: dark) { html { @include mat.theme(( color: ( primary: mat.$azure-palette, theme-type: dark ), typography: Roboto, density: 0, )); } }` immediately after the light theme `html { }` block per [research.md](research.md) R-002 and [contracts/design-tokens.md](contracts/design-tokens.md) Theming Architecture
- [X] T051 [P] [US5] Verify poll session chart colours in dark mode in `src/bsdcpolls-frontend/src/app/features/polls/session/` — if custom chart colours are used (not M3 tokens), replace with token-based alternatives or add `@media (prefers-color-scheme: dark)` overrides using M3 colour role tokens; no hardcoded hex allowed
- [X] T052 [P] [US5] Verify survey results chart colours in dark mode in `src/bsdcpolls-frontend/src/app/features/surveys/results/` — same check as T051; chart legend and bar/segment colours must use M3 tokens or have explicit dark overrides that maintain 3:1 contrast

**Checkpoint**: OS dark mode → all screens dark; axe dark-mode scan zero violations; light→dark OS toggle updates app within one interaction without reload.

---

## Phase 9: User Story 7 — Use the app when colour is removed (Priority: P3)

**Goal**: Every status distinction, chart segment, and error state is communicated by an icon, pattern, or text label — not colour alone.

**Independent Test**: Apply `filter: grayscale(100%)` in DevTools on `<html>` → all poll statuses, chart segments, and form errors still distinguishable; SC-009 checklist passes.

- [X] T053 [P] [US7] Add icon + text to poll status badges wherever they appear (feed cards, poll session header) — replace colour-only status indicators with `<mat-icon aria-hidden="true">[icon]</mat-icon> [StatusText]` pattern using icons `check_circle` (Active), `cancel` (Closed), `edit` (Draft) per [contracts/accessibility.md](contracts/accessibility.md) Section 9; update relevant templates in `src/bsdcpolls-frontend/src/app/features/feed/` and `polls/session/`
- [X] T054 [P] [US7] Add non-colour distinguishers to survey results chart in `src/bsdcpolls-frontend/src/app/features/surveys/results/` — if using a bar chart: add data labels on or beside each bar; ensure legend entries include index numbers or patterns in addition to colour fills; for accessible rendering consider adding a summary `<table>` alternative (already required by T037)
- [X] T055 [P] [US7] Add error icons to form error states across all forms — in all components using `<mat-error>`: prepend a `<mat-icon aria-hidden="true">error</mat-icon>` inside the `mat-error` so the error is indicated by icon + text, not colour alone; update templates in `src/app/features/auth/login/`, `register/`, `polls/create-poll/`, `surveys/builder/`, `surveys/respondent/` per [contracts/accessibility.md](contracts/accessibility.md) 9 table

**Checkpoint**: DevTools grayscale filter → all statuses readable; chart segments labelled; form errors show icon + text; SC-007 passes (zero WCAG 1.4.1 violations).

---

## Phase 10: Polish & Cross-Cutting Concerns

**Purpose**: Linting, final accessibility audit, prefers-reduced-motion verification, token compliance sweep.

- [X] T056 Run `ng lint --max-warnings 0` from `src/bsdcpolls-frontend/` and fix all Angular ESLint violations introduced during this feature; zero lint errors is a merge requirement
- [X] T057 [P] Run `prettier --check "src/**/*.{ts,html,scss,json}"` from `src/bsdcpolls-frontend/` and fix all formatting violations; commit all Prettier-formatted files
- [X] T058 [P] Search all component SCSS files for prohibited values — run `grep -rn '#[0-9a-fA-F]\{3,6\}' src/bsdcpolls-frontend/src/app/**/*.scss`, `grep -rn 'font-size:\s*[0-9]'`, `grep -rn 'border-radius:\s*[0-9]'`, `grep -rn '::ng-deep'`; fix any matches to use M3 tokens or SCSS variables per SC-005 and [contracts/design-tokens.md](contracts/design-tokens.md) Prohibited Values
- [X] T059 [P] Verify `prefers-reduced-motion` suppression — toggle OS Reduce Motion on; navigate through all screens checking that no slide/fade/expand animations occur; fix any component SCSS that has custom `transition` or `animation` without the `@include motion.reduce-motion` guard per [research.md](research.md) R-005
- [X] T060 Run axe DevTools audit at all 5 breakpoints (320, 600, 960, 1280, 1920 px) for all 9 screens — follow SC-001 checklist in [quickstart.md](quickstart.md); log any remaining violations and fix before marking complete
- [X] T061 Complete full [quickstart.md](quickstart.md) validation checklist — SC-001 through SC-010; run keyboard-only flow (SC-002); run screen reader journey (SC-003); test 200% zoom (SC-004); verify dark mode (SC-007); verify reduced motion (SC-008); verify touch targets (SC-009); mark each SC item as passed

---

## Dependencies & Execution Order

### Phase Dependencies

```
Phase 1 (Setup)        — No dependencies, start immediately
   ↓
Phase 2 (Foundational) — Depends on Phase 1 complete (BLOCKS all user stories)
   ↓
Phase 3 (US1 Mobile)   — Depends on Phase 2 complete
Phase 4 (US2 Keyboard) — Depends on Phase 3 complete (builds on HTML structure)
Phase 5 (US3 Screen Reader) — Depends on Phase 3 complete (adds ARIA to existing structure)
   ↓
Phase 6 (US6 Tablet)   — Depends on Phase 3 (adds md breakpoint to xs/sm layouts)
Phase 7 (US4 Desktop)  — Depends on Phase 3 (adds lg/xl breakpoint to xs/sm layouts)
Phase 8 (US5 Dark Mode) — Depends on Phase 2 (styles.scss baseline must exist)
   ↓
Phase 9 (US7 Colour)   — Depends on Phase 3, 5 (chart markup from US3 T037 needed)
   ↓
Phase 10 (Polish)      — Depends on all phases complete
```

### Within-Phase Ordering (exceptions to [P] parallelism)

- **Phase 2**: T006 → T007 (LayoutStore needs BreakpointService); T007 → T012 (AppComponent needs LayoutStore); T008/T009/T010/T011 can start after T007 (parallel)
- **Phase 3**: All T014–T022 are fully parallel (different component files)
- **Phase 5**: T030 should complete before T034/T035 (route titles and live region components should be in place); T031–T033 are parallel
- **Phase 9**: T054 best done after T037 (survey results accessible table from US3 is the non-colour fallback)

---

## Parallel Opportunities

### Phase 1 Parallel Block

```
T001 (_tokens.scss) ║ T002 (_breakpoints.scss) ║ T003 (_motion.scss)
T004 (Breakpoint enum) ║ T005 (IBreakpointService)
```

### Phase 2 Parallel Block (after T007)

```
T008 (SkipLink) ║ T009 (BottomNav) ║ T010 (NavRail) ║ T011 (NavDrawer)
```

### Phase 3 Parallel Block (all 9 screens)

```
T014 (Login) ║ T015 (Register) ║ T016 (Feed) ║ T017 (CreatePoll)
T018 (PollSession) ║ T019 (SurveyBuilder) ║ T020 (SurveyRespondent)
T021 (SurveyResults) ║ T022 (Profile)
```

### Phase 6 + 7 Parallel Blocks

Phase 6 and Phase 7 breakpoint tasks for different screens can run in parallel with each other once Phase 3 establishes the base structure.

---

## Implementation Strategy

### MVP First (P1 Stories Only)

1. Complete Phase 1 + 2 (Setup + Foundation) — ~1 session
2. Complete Phase 3 (US1 — phone layouts) — ~1 session
3. Complete Phase 4 (US2 — keyboard) — ~0.5 session
4. Complete Phase 5 (US3 — screen reader) — ~0.5 session
5. **STOP AND VALIDATE**: Run SC-001 through SC-003 from [quickstart.md](quickstart.md)
6. App is now WCAG AA compliant at mobile and keyboard-operable — releasable MVP

### Incremental Delivery

7. Add Phase 6 (US6 — tablet) → validate at 768 px → SC-006 tablet rows
8. Add Phase 7 (US4 — desktop) → validate at 1280/1920 px → SC-006 lg/xl rows
9. Add Phase 8 (US5 — dark mode) → validate SC-007
10. Add Phase 9 (US7 — colour independence) → validate SC-009
11. Phase 10 (Polish) → all SCs pass → feature complete

### Single-Developer Sequence

```
T001 → T002 → T003 → T004 → T005 →
T006 → T007 → T008 → T009 → T010 → T011 → T012 → T013 →
[T014 through T022 in any order] →
[T023 through T029 in any order] →
[T030 through T037 in any order] →
T050 (dark mode early — low risk, self-contained) →
[T038 through T049 in any order] →
[T051 through T055 in any order] →
T056 → T057 → T058 → T059 → T060 → T061
```

---

## Notes

- `[P]` tasks operate on different files — safe to run in parallel in a single-agent session
- `[Story]` label maps each task to a user story for traceability back to [spec.md](spec.md)
- No test files are created or modified (Principle XV — no test projects)
- All SCSS values must use tokens from `_tokens.scss` or M3 CSS vars — no hardcoded primitives
- `::ng-deep` is permanently banned — do not introduce it to solve any styling problem
- Commit after each logical group (one phase or one [P] parallel block)
- Stop at any **Checkpoint** to validate the story independently before proceeding
