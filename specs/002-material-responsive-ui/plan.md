# Implementation Plan: Consistent, Fully Responsive, Maximum Accessibility UI

**Branch**: `002-material-responsive-ui` | **Date**: 2026-06-26 | **Spec**: [spec.md](spec.md)

**Input**: Feature specification from `specs/002-material-responsive-ui/spec.md`

## Summary

Retrofit every screen in the BSDCPolls Angular application with a fully responsive layout that works at all five M3 breakpoints (xs → xl), meet WCAG 2.1 Level AA (AAA where feasible), implement an adaptive navigation shell (bottom nav / nav rail / nav drawer), enforce M3 design token usage throughout, and add dark mode support driven by `prefers-color-scheme`. No backend or .NET changes are required — this is a pure Angular/SCSS frontend feature.

## Technical Context

**Language/Version**: TypeScript 5.x, Angular 19 (strict mode, standalone components)

**Primary Dependencies**:
- `@angular/material` (M3 theming, all component modules) — pre-approved
- `@angular/cdk` (BreakpointObserver, A11yModule, FocusTrap) — pre-approved
- `@ngrx/signals` (layout store for current breakpoint + dark mode signal) — pre-approved
- `rxjs` (BreakpointObserver stream → toSignal) — pre-approved
- No new third-party dependencies required

**Storage**: N/A — UI layer only; no database schema changes

**Testing**: None (Principle XV — no test projects)

**Target Platform**: Web browser — latest 2 versions of Chrome, Safari, Firefox, Edge; viewport range 320 px → 2560 px+

**Project Type**: Single Page Application — Angular frontend feature

**Performance Goals**: < 100 ms layout recalculation on viewport resize, 60 fps CSS transitions (suppressed under `prefers-reduced-motion`), no layout shift (CLS 0) on breakpoint change

**Constraints**:
- All 5 breakpoints fully functional and manually verified per screen
- WCAG 2.1 AA zero automated violations (axe-core)
- Zero hardcoded colour / font-size / spacing values in SCSS — M3 tokens only
- No `::ng-deep` anywhere
- No inline styles

**Scale/Scope**: 7 screens (login, register, feed, create-poll, poll-session, survey-builder, survey-respondent, survey-results, profile) × 5 breakpoints = 45 verified layout states; 3 navigation patterns (bottom nav, nav rail, nav drawer)

## Constitution Check

*GATE: Must pass before Phase 0 research.*

| Principle | Status | Notes |
|-----------|--------|-------|
| **I — Angular Material + Controlled SCSS** | ✅ PASS | SCSS used only for M3 theming (`mat.define-theme`), responsive overrides at named breakpoints, and component-scoped layout properties. No `::ng-deep`. No inline styles. No utility-class libraries. No hardcoded colour/font/shadow values. |
| **II — Reactive-First** | ✅ PASS | `BreakpointObserver` stream converted to signal via `toSignal`. Layout state (active breakpoint, dark mode) managed in NgRX Signal Store `LayoutStore`. No imperative mutations outside store actions. |
| **III — Real-Time First** | ✅ PASS | No new real-time integrations. Existing SignalR vote-count updates must retain `aria-live` regions — compliance verified at implementation. |
| **IV — Performance & Bundle Discipline** | ✅ PASS | All feature modules already lazy-loaded. No new third-party libraries added. Angular Material and CDK are pre-approved and already bundled. |
| **V — Accessibility, Responsive Design & M3** | ✅ PASS | This feature is the direct implementation of Principle V. All WCAG 2.1 AA requirements, M3 design token mandate, and 5-breakpoint verification are the core deliverables. |
| **VI — BFF Architecture** | ✅ N/A | No changes to BFF or API call patterns. |
| **VII — IaC & Environment Parity** | ✅ N/A | No new services or containers. |
| **VIII — Layered .NET Architecture** | ✅ N/A | No .NET changes. |
| **IX — Code Quality First** | ✅ PASS | All SCSS blocks carry inline comments explaining why Angular Material primitives are insufficient. No shortcuts, no commented-out code. |
| **X — Contract-Driven Validation** | ✅ N/A | No new DTOs or form contracts. Existing FluentValidation unchanged. |
| **XI — Observability** | ✅ N/A | No new error paths. Existing `ErrorHandler` already covers unhandled Angular errors. |
| **XII — Code Style & Linting** | ✅ PASS | `ng lint --max-warnings 0`, `prettier --check`, and Stylelint (if configured) must pass for all new/modified files. |
| **XIII — EF Core Conventions** | ✅ N/A | No data layer changes. |
| **XIV — Interface-Driven Design** | ✅ PASS | `BreakpointService` implements `IBreakpointService`; `ILayoutStore` token type defined. |
| **XV — No Test Projects** | ✅ PASS | No test files created. |

**All constitution gates pass. Proceeding to Phase 0.**

*Post-design re-check*: No violations introduced. The `LayoutStore` is a standard NgRX Signal Store feature (Principle II). All SCSS is theme-and-layout-scoped (Principle I). All new services implement interfaces (Principle XIV).

## Project Structure

### Documentation (this feature)

```text
specs/002-material-responsive-ui/
├── plan.md              # This file
├── research.md          # Phase 0 output
├── data-model.md        # Phase 1 output — design token taxonomy + breakpoint model
├── quickstart.md        # Phase 1 output — manual validation guide
├── contracts/
│   ├── navigation-layout.md    # Navigation pattern per breakpoint
│   ├── design-tokens.md        # M3 colour, typography, spacing token catalogue
│   └── accessibility.md        # ARIA contracts, live-region specs, focus management
└── tasks.md             # Phase 2 output (created by /speckit-tasks)
```

### Source Code (repository root)

```text
src/bsdcpolls-frontend/
├── src/
│   ├── styles.scss                          # Global M3 theme: light + dark, reset, skip link
│   ├── styles/
│   │   ├── _tokens.scss                     # SCSS token forwarding (M3 roles → SCSS vars)
│   │   ├── _breakpoints.scss                # Named breakpoint mixin library
│   │   └── _motion.scss                     # prefers-reduced-motion mixins
│   └── app/
│       ├── app.component.ts                 # Shell: skip link, responsive nav, router outlet
│       ├── app.component.scss               # Shell responsive layout only
│       ├── core/
│       │   └── layout/
│       │       ├── layout.store.ts          # NgRX Signal Store: breakpoint + colour-scheme
│       │       ├── ibreakpoint.service.ts   # Interface
│       │       └── breakpoint.service.ts    # CDK BreakpointObserver → Breakpoint enum
│       ├── shared/
│       │   ├── skip-link/
│       │   │   └── skip-link.component.ts   # "Skip to main content" first focusable element
│       │   └── nav/
│       │       ├── bottom-nav/              # xs/sm: MatBottomNav or custom mat-tab-nav-bar
│       │       │   └── bottom-nav.component.ts
│       │       ├── nav-rail/                # md: compact side-nav
│       │       │   └── nav-rail.component.ts
│       │       └── nav-drawer/              # lg/xl: full mat-sidenav
│       │           └── nav-drawer.component.ts
│       └── features/
│           ├── auth/
│           │   ├── login/                   # Responsive centred card form
│           │   └── register/                # Responsive centred card form
│           ├── feed/                        # Responsive grid: 1-col xs/sm, 2-col md, 3-col lg/xl
│           ├── polls/
│           │   ├── create-poll/             # Responsive form, full-width xs, constrained lg+
│           │   └── session/                 # Voting UI + live results with aria-live regions
│           ├── surveys/
│           │   ├── builder/                 # Responsive survey builder
│           │   ├── respondent/              # Responsive survey form
│           │   └── results/                 # Results chart: stacked xs/sm, side-by-side lg/xl
│           └── profile/                     # Responsive profile layout
```

**Structure Decision**: Single Angular project with dedicated `core/layout/` for breakpoint/theme services and `shared/nav/` for the three navigation components. All SCSS partials live in `src/styles/` to keep global token definitions separate from component SCSS. No new routing or module boundaries needed.

## Complexity Tracking

> No constitution violations. No complexity exceptions required.
