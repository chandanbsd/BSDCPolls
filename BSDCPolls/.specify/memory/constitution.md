<!--
SYNC IMPACT REPORT
==================
Version change: 1.0.0 → 1.1.0

Reason for MINOR bump: Two new principles added (VI, VII). Technical Constraints section
fully resolved — TODO(BACKEND_STACK) replaced with concrete stack. Principle III updated
to name SignalR explicitly as the mandated real-time transport.

Modified principles:
  - III. Real-Time First — added SignalR as the mandated transport; removed generic
    "WebSockets or SSE" language now that the stack is decided.

Added principles:
  - VI. BFF Architecture (NON-NEGOTIABLE) — frontend talks only to BFF, never to backend API.
  - VII. Infrastructure as Code & Environment Parity — Aspire + Podman, local = production.

Removed sections: None

Templates reviewed:
  - .specify/templates/plan-template.md     ✅ No changes needed — Constitution Check gate
                                               already present; plan authors must list Aspire
                                               service declarations as a task.
  - .specify/templates/spec-template.md     ✅ No changes needed — technology-agnostic.
  - .specify/templates/tasks-template.md    ✅ No changes needed — phased delivery model aligns.

Follow-up TODOs:
  - TODO(SIGNALR_HUB_TOPOLOGY): Whether the BFF proxies raw SignalR frames to the backend
    or translates to an internal gRPC/HTTP call is an architectural decision to be made at
    the first real-time feature plan. Record the decision here via PATCH amendment.
-->

# BSDCPolls Constitution

## Core Principles

### I. Angular Material Only (NON-NEGOTIABLE)

All UI MUST be built exclusively with Angular Material components and its theming system.
Custom CSS of any kind is PROHIBITED — this includes inline styles (`[style]` bindings),
component-scoped stylesheets, global CSS overrides, and utility-class libraries (e.g., Tailwind).
Layout MUST be achieved through Angular Material layout primitives (CDK, Angular Flex Layout, or
Material's built-in structural components). Theming customisation MUST use Angular Material's
design token / theming API only.

**Rationale**: A zero-custom-CSS rule eliminates style drift, enforces visual consistency, and
ensures every UI element inherits accessibility and theming behaviour from the design system
without manual intervention.

### II. Reactive-First Architecture (NON-NEGOTIABLE)

All shared application state MUST be managed through NgRX Signal Store. Imperative state
mutations outside of store actions are PROHIBITED. Async data flows MUST use RxJS Observables or
Angular Signals; raw Promise chains in component code are PROHIBITED. Local component state
(transient UI flags, form control state) is permitted only when the data is not consumed by any
other component or service.

**Rationale**: A single reactive state layer makes real-time data propagation predictable,
keeps components as pure projections of state, and prevents cascading change-detection issues
in a high-frequency voting context.

### III. Real-Time First

Poll vote counts and live participant presence MUST update in real-time without a page reload or
manual refresh. **SignalR is the sole mandated real-time transport** for this project — the
Angular frontend MUST use `@microsoft/signalr` to connect to the BFF's SignalR hub, and the BFF
MUST relay live events from the backend API via SignalR. HTTP polling as a substitute for
push-based updates is PROHIBITED. Real-time hub contracts (hub name, method names, payload
shapes) MUST be documented in `specs/<feature>/contracts/` before any frontend hub integration
begins.

**Rationale**: SignalR is the idiomatic real-time transport for ASP.NET Core and has a mature
Angular client. Mandating it avoids fragmented transport choices (raw WS, SSE, polling) across
features, and its built-in fallback negotiation handles network variance transparently.

### IV. Performance & Bundle Discipline

Every Angular feature module MUST be lazy-loaded unless it is strictly part of the application
shell (root layout, auth guard, core services). Bundle size MUST be audited at each feature
merge using Angular's build analyzer; unexplained regressions are a blocking review failure.
Third-party dependencies require explicit justification in the plan's Constitution Check section;
Angular, Angular Material, RxJS, NgRX Signal Store, and `@microsoft/signalr` are the only
pre-approved frontend libraries. All other library additions MUST be approved via constitution
amendment or plan-level exemption.

**Rationale**: A polling app is inherently public-facing and must load fast on mobile networks.
Lazy loading and strict dependency control are the primary levers for keeping Time-to-Interactive
low and the bundle lean.

### V. Accessibility & Responsive Design

All screens MUST be fully functional and visually correct at every standard breakpoint from 320px
(phone portrait) through 1920px+ (large desktop). All interactive elements MUST meet WCAG 2.1 AA
standards. Angular Material's built-in a11y behaviour (focus management, ARIA roles, keyboard
navigation) MUST NOT be overridden or bypassed. Responsive layouts MUST be verified on at least
three breakpoints (mobile / tablet / desktop) before a feature is marked complete.

**Rationale**: BSDCPolls is used across devices. Accessibility is a baseline requirement, not an
afterthought — Angular Material provides it for free only when its patterns are not circumvented.

### VI. BFF Architecture (NON-NEGOTIABLE)

The Angular frontend MUST connect exclusively to the BFF (Backend For Frontend) ASP.NET Core
project. Direct calls from the frontend to the backend API are PROHIBITED. The BFF is the sole
internet-facing .NET service; the backend API MUST be network-isolated to internal service
communication only. The BFF is responsible for: forwarding authenticated requests to the backend
API, hosting the SignalR hub that the frontend connects to, and enforcing Supabase auth token
validation at the edge. No business logic MUST be duplicated between BFF and backend — the BFF
acts as a thin translation and security layer.

**Rationale**: The BFF pattern keeps the public attack surface minimal, allows frontend-specific
concerns (auth edge, aggregation, SignalR fan-out) to evolve independently of the core domain
API, and avoids CORS configuration sprawl by providing a single frontend origin.

### VII. Infrastructure as Code & Environment Parity (NON-NEGOTIABLE)

Every service, database, auth provider, and configuration value MUST be declared in the .NET
Aspire AppHost project. The local development environment MUST be a 1:1 functional replica of
the production topology — no manual setup steps, no environment-only services, and no
"works on my machine" exceptions are permitted. All container images MUST be run via Podman;
Docker MUST NOT be a dependency. Self-hosted Supabase (PostgreSQL and Auth) MUST be provisioned
as Podman container images wired through Aspire. Secrets in local dev MUST mirror the structure
of production secrets (values may differ; shape and key names MUST NOT). The Aspire AppHost is
the single source of truth for service topology, connection strings, and inter-service references.

**Rationale**: Production incidents caused by local-only configuration are eliminated when the
local environment is structurally identical to production. Aspire + Podman provides reproducible
orchestration without requiring a cloud account or Docker Desktop license.

## Technical Constraints

- **Frontend**: Angular (latest stable LTS), Angular Material, RxJS, NgRX Signal Store, Angular
  Signals, `@microsoft/signalr`. No other UI or state libraries without a constitution amendment.
- **Real-time transport**: SignalR. Frontend uses `@microsoft/signalr`; BFF and backend use
  ASP.NET Core SignalR hubs.
- **Backend services** (both latest stable):
  - *BFF*: ASP.NET Core Web API, C# — internet-facing, hosts SignalR hub, validates Supabase
    JWT, proxies to backend API.
  - *Backend API*: ASP.NET Core Web API, C#, Entity Framework Core — internal only, owns domain
    logic and database access.
- **Orchestration**: .NET Aspire (latest stable). All services, containers, environment
  variables, and inter-service references MUST be declared in the Aspire AppHost project. No
  service may be started outside of Aspire during local development.
- **Containers**: Podman. All container images use Podman. Docker MUST NOT be required.
- **Database**: Self-hosted Supabase (PostgreSQL). Provisioned as a Podman container via Aspire.
  EF Core targets this PostgreSQL instance. No managed cloud DB in local or CI environments.
- **Auth**: Self-hosted Supabase Auth. Provisioned as a Podman container via Aspire. The BFF
  validates Supabase JWTs. No external auth provider network calls during local development.
- **CSS**: Zero custom CSS, zero inline styles — enforced at code review. Angular ESLint rules
  SHOULD be configured to catch `[style]` binding violations automatically.
- **Testing**: Angular Testing Library + Jest for frontend unit tests; Playwright for e2e.
  xUnit for .NET unit and integration tests. All test tooling MUST be declared in the Aspire
  AppHost or documented in `plan.md` Technical Context before implementation begins.
- **TODO(SIGNALR_HUB_TOPOLOGY)**: Whether the BFF proxies raw SignalR to the backend or
  translates to an internal call is undecided. Resolve at first real-time feature plan.

## Development Workflow

All feature work MUST follow the speckit flow: `/speckit-specify` → `/speckit-plan` →
`/speckit-tasks` → `/speckit-implement`. The Constitution Check gate in `plan.md` MUST be
completed and passing before Phase 0 research begins. API contracts and SignalR hub contracts
MUST be defined in `specs/<feature>/contracts/` before any frontend integration starts.
Every plan MUST include a task for declaring new services or containers in the Aspire AppHost.
Features are delivered incrementally by user story, with each story independently testable
and demonstrable against the full local Aspire stack.

## Governance

This constitution supersedes all other practices, style guides, and conventions. Any conflict
between this document and another guideline resolves in favour of this constitution.

Amendments require:
1. A documented rationale explaining why the existing principle is insufficient.
2. A semantic version bump (MAJOR for principle removal/redefinition, MINOR for additions,
   PATCH for clarifications).
3. Propagation of the change to all affected templates and this Sync Impact Report.
4. Review and explicit acceptance before the next feature spec is opened.

All PRs MUST verify compliance with Principles I (Angular Material Only), II (Reactive-First),
VI (BFF Architecture), and VII (IaC & Environment Parity) in the Constitution Check section of
`plan.md`. Complexity exceptions MUST be justified in the plan's Complexity Tracking table.
Use `CLAUDE.md` for runtime agent guidance.

**Version**: 1.1.0 | **Ratified**: 2026-06-10 | **Last Amended**: 2026-06-10
