# Implementation Plan: BSDCPolls — Real-Time Polls & Surveys Platform

**Branch**: `001-polls-surveys-platform` | **Date**: 2026-06-10 | **Spec**: [spec.md](./spec.md)

**Input**: Feature specification from `specs/001-polls-surveys-platform/spec.md`

## Summary

BSDCPolls is a real-time polling and survey platform built entirely from scratch. Users register with
auto-generated, privacy-preserving three-word usernames and no PII. They create or participate in
**live polls** (creator pushes questions in real time to connected participants via SignalR) and
**structured surveys** (conditional-branching questionnaires with multiple answer types including
PDF document upload, stored as JSONB in PostgreSQL). A home feed surfaces public and invited
content across three tabs (Polls, Surveys, Results). A notification bell alerts users to invitations.
A profile view exposes privacy controls for feed visibility and invite permissions.

Architecture: Angular SPA → BFF (ASP.NET Core, auth edge, SignalR hubs) → Backend API
(ASP.NET Core, domain logic, EF Core / PostgreSQL). All services orchestrated via .NET Aspire with
Podman containers. Supabase GoTrue handles auth; SigNoz handles observability.

## Technical Context

**Language/Version**:

- Frontend: TypeScript 5.x / Angular 19 (latest stable LTS)
- Backend: C# 13 / .NET 9 (latest stable)

**Primary Dependencies**:

- Frontend: Angular Material, RxJS, NgRX Signal Store, `@microsoft/signalr`, NSwag-generated API
  clients and types, Angular ESLint, Prettier
- Backend: ASP.NET Core 9, EF Core 9 (Npgsql provider), FluentValidation, NSwag, Serilog,
  OpenTelemetry .NET SDK, CSharpier, StyleCop.Analyzers
- Orchestration: .NET Aspire 9 (AppHost + resource annotations)

**Storage**:

- PostgreSQL via self-hosted Supabase (Podman container via Aspire)
- JSONB columns for survey question trees and response answer payloads
- PostgreSQL `bytea` columns for PDF uploads (≤ 10 MB per file)

**Testing**: None — Principle XV mandates zero test projects of any kind.

**Target Platform**: Web (desktop + mobile browsers); Angular SPA served as static files by BFF
or a Podman-hosted static file server wired through Aspire.

**Project Type**: Full-stack web application — Angular SPA + .NET BFF + .NET backend API.

**Performance Goals** (from Success Criteria):

- Real-time question delivery to participants ≤ 1 second (SC-002)
- Live vote-count updates to creator ≤ 1 second (SC-003)
- Home feed initial load ≤ 2 seconds (SC-005)
- Notification delivery ≤ 5 seconds (SC-007)
- Registration completion ≤ 60 seconds (SC-001)

**Constraints**:

- Zero PII stored at any point (FR-002, SC-010)
- PDF uploads: PDF-only MIME validation, max 10 MB per file
- No test files or test projects anywhere in the repository (Principle XV)
- Zero custom CSS anywhere in the Angular project (Principle I)
- Zero raw SQL in .NET projects (Principle XIII)
- All linters must pass with zero warnings before any merge (Principle XII)

**Scale/Scope**: Initial target — ~1,000 concurrent users; 6 lazy-loaded Angular feature modules;
8 .NET projects; ~20 API endpoint groups; 2 SignalR hubs (`PollHub`, `NotificationHub`).

## Constitution Check

_GATE: Must pass before Phase 0 research. Re-check after Phase 1 design._

| #    | Principle                                  | Compliance Plan                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                              | Status  |
| ---- | ------------------------------------------ | ---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | ------- |
| I    | Angular Material Only                      | All UI components are Angular Material. Zero `[style]` bindings, component stylesheets, or utility-class libraries. Layout via Material CDK layout primitives. ESLint rule configured to catch inline style violations.                                                                                                                                                                                                                                                                                                      | ✅ PASS |
| II   | Reactive-First (NgRX Signal Store)         | Auth state, feed state, poll session state, survey state, and notification state all managed in NgRX Signal Stores. RxJS Observables for all async flows. No raw Promise chains in component code. Local component state only for transient UI flags (e.g., drawer open/closed).                                                                                                                                                                                                                                             | ✅ PASS |
| III  | Real-Time First (SignalR)                  | `PollHub`: live question broadcasts and vote-count updates. `NotificationHub`: real-time invitation delivery. Frontend uses `@microsoft/signalr`. BFF hosts both hubs and relays events from the backend API. No HTTP polling as a substitute for any push event.                                                                                                                                                                                                                                                            | ✅ PASS |
| IV   | Performance & Bundle Discipline            | All six feature modules (`auth`, `feed`, `polls`, `surveys`, `notifications`, `profile`) lazy-loaded. Only app shell (root layout, auth guard, core services, global error handler, `traceparent` interceptor) eager-loaded. Bundle audited at each merge using Angular build analyzer.                                                                                                                                                                                                                                      | ✅ PASS |
| V    | Accessibility & Responsive Design          | Angular Material a11y used throughout (focus management, ARIA roles, keyboard nav). WCAG 2.1 AA compliance. Responsive layouts verified at 320px / 768px / 1440px before each feature story is marked complete.                                                                                                                                                                                                                                                                                                              | ✅ PASS |
| VI   | BFF Architecture                           | Frontend connects to BFF only. BFF validates Supabase JWT on every request. BFF hosts SignalR hubs. BFF calls backend API via internal HTTPS (gRPC preferred for strongly-typed contracts). API is network-isolated and never reachable from the public internet. No business logic duplicated between BFF and backend.                                                                                                                                                                                                      | ✅ PASS |
| VII  | IaC & Environment Parity (Aspire + Podman) | All services declared in `BSDCPolls.AppHost`: BFF, API, MigrationWorker, Supabase PostgreSQL container, Supabase GoTrue container, SigNoz container. No Docker dependency. `BSDCPolls.MigrationWorker` runs EF Core migrations at startup before API becomes available.                                                                                                                                                                                                                                                      | ✅ PASS |
| VIII | Layered .NET Architecture                  | Eight projects with enforced upward-reference prohibition via `.csproj` references: `AppHost`, `MigrationWorker`, `BFF` (→ `BFF.Business`, `Contracts`), `BFF.Business` (→ `Contracts`), `Api` (→ `Api.Business`, `Contracts`), `Api.Business` (→ `Api.Data`, `Contracts`), `Api.Data` (→ `Contracts` for shared value types only), `Contracts` (zero internal dependencies).                                                                                                                                                | ✅ PASS |
| IX   | Code Quality & Maintainability First       | `<TreatWarningsAsErrors>true</TreatWarningsAsErrors>` and `<Nullable>enable</Nullable>` in all .NET projects. Angular `strict: true` and `strictTemplates: true`. All public/internal .NET APIs have XML doc comments. All exported Angular services/components/stores have JSDoc. No commented-out code. No `// TODO` merged to main.                                                                                                                                                                                       | ✅ PASS |
| X    | Contract-Driven Validation                 | All request DTOs in `BSDCPolls.Contracts` have co-located `AbstractValidator<T>`. NSwag generates TypeScript types and API client from BFF OpenAPI spec. Generated files committed to repo. No hand-written TypeScript duplicating C# types. Angular validators derived from NSwag metadata; complex cross-field rules implemented as custom Angular validators with `// Mirrors: BSDCPolls.Contracts.<Validator>.<Rule>` comments. AutoMapper PROHIBITED.                                                                   | ✅ PASS |
| XI   | Observability (SigNoz + OpenTelemetry)     | Global Angular `ErrorHandler` reports unhandled errors to BFF logging endpoint (error message, stack trace, route, component context). HTTP interceptor injects `traceparent` on all outgoing requests. All .NET services export OTLP traces/metrics/logs to SigNoz. `X-Trace-Id` header + trace ID in error response body. W3C TraceContext propagation on BFF → API calls. SigNoz provisioned as Podman container in Aspire AppHost.                                                                                       | ✅ PASS |
| XII  | Code Style & Linting                       | CSharpier: `.csharpierrc.json` at solution root; `dotnet csharpier --check .` in CI. StyleCop.Analyzers: applied via `Directory.Build.props`; single `stylecop.json` + solution-level `GlobalSuppressions.cs`. Prettier: single `.prettierrc` at repo root; `prettier --check .` in CI. Angular ESLint: `ng lint --max-warnings 0` in CI. All linters are build-fail-on-violation.                                                                                                                                           | ✅ PASS |
| XIII | EF Core Conventions                        | All entities extend `AuditableEntity` (Id, Uid, IsActive, CreatedOn, CreatedById, UpdatedOn, UpdatedById). Private setters. Static `Create` factory methods. Named instance methods for mutations. No raw SQL. N+1 prevented via `Include`/`ThenInclude` with inline comments. `SaveChangesInterceptor` populates audit fields and writes `AuditLog` rows. All relationships configured via Fluent API in `OnModelCreating` with explicit FK properties. `UseLazyLoadingProxies()` enabled; navigation properties `virtual`. | ✅ PASS |
| XIV  | Interface-Driven Design                    | All service and data-access classes in `*.Business` and `Api.Data` implement `I`-prefixed interfaces co-located in the same folder. All DI registrations use interface types. Constructor parameters declare interface types. Controllers and EF Core `DbContext` subclass are exempt.                                                                                                                                                                                                                                       | ✅ PASS |
| XV   | No Test Projects                           | Zero test files (`.spec.ts`, `*Tests.csproj`), zero test directories (`tests/`, `__tests__/`), zero test library references anywhere in the solution. Any AI-generated test code MUST NOT be committed.                                                                                                                                                                                                                                                                                                                      | ✅ PASS |

**Gate result: ALL 15 PRINCIPLES PASS — proceed to Phase 0**

## Project Structure

### Documentation (this feature)

```text
specs/001-polls-surveys-platform/
├── plan.md              # This file
├── research.md          # Phase 0 output
├── data-model.md        # Phase 1 output
├── quickstart.md        # Phase 1 output
├── contracts/
│   ├── api-endpoints.md      # REST API contracts
│   ├── signalr-hubs.md       # SignalR hub contracts
│   └── dto-schemas.md        # Request/response DTO shapes
└── tasks.md             # Phase 2 output (/speckit-tasks — not created here)
```

### Source Code (repository root)

```text
BSDCPolls/                          # repository root
├── .specify/
├── specs/
├── BSDCPolls.sln
│
├── # ── Angular SPA ──────────────────────────────────────────────────────
├── bsdcpolls-frontend/
│   ├── src/
│   │   └── app/
│   │       ├── core/
│   │       │   ├── auth/            # Supabase auth service, auth guard, token interceptor
│   │       │   ├── error/           # Global ErrorHandler + BFF error reporting service
│   │       │   ├── signalr/         # SignalR connection factory and base connection service
│   │       │   └── observability/   # traceparent HTTP interceptor
│   │       ├── shared/              # Shared Angular Material wrapper components + pipes
│   │       ├── features/
│   │       │   ├── auth/            # Login + Register (lazy-loaded)
│   │       │   ├── feed/            # Home feed with 3 tabs: Polls | Surveys | Results (lazy-loaded)
│   │       │   ├── polls/           # Poll creation, live session view, creator dashboard (lazy-loaded)
│   │       │   ├── surveys/         # Survey builder, respondent view, results view (lazy-loaded)
│   │       │   ├── notifications/   # Notification bell component + notification panel (lazy-loaded)
│   │       │   └── profile/         # Profile view + privacy settings (lazy-loaded)
│   │       ├── store/               # NgRX Signal Stores (auth, feed, poll-session, survey, notifications)
│   │       └── generated/           # NSwag-generated TypeScript API clients and types (committed)
│   ├── angular.json
│   ├── tsconfig.json
│   ├── eslint.config.js
│   └── nswag.json                   # NSwag generation configuration
│
├── # ── .NET Solution ────────────────────────────────────────────────────
├── src/
│   ├── BSDCPolls.AppHost/           # Aspire AppHost — declares ALL services and containers
│   ├── BSDCPolls.MigrationWorker/   # Aspire worker — runs EF Core migrations at startup
│   ├── BSDCPolls.BFF/               # ASP.NET Core BFF — controllers, SignalR hubs, JWT validation
│   ├── BSDCPolls.BFF.Business/      # BFF business logic — auth forwarding, service orchestration
│   ├── BSDCPolls.Api/               # ASP.NET Core backend API — controllers (internal-only)
│   ├── BSDCPolls.Api.Business/      # Domain logic — poll/survey/user/invitation services
│   ├── BSDCPolls.Api.Data/          # EF Core DbContext, entities, migrations, repositories
│   └── BSDCPolls.Contracts/         # Shared DTOs, FluentValidation validators, SignalR payloads
│
├── # ── Solution-wide Config ─────────────────────────────────────────────
├── Directory.Build.props            # StyleCop.Analyzers, TreatWarningsAsErrors, Nullable enable
├── .csharpierrc.json                # CSharpier formatting config
├── stylecop.json                    # StyleCop rules
├── GlobalSuppressions.cs            # Solution-level SuppressMessage (reviewed per suppression)
└── .prettierrc                      # Prettier config for TS/HTML/JSON
```

**Structure Decision**: Option 2 (web application with Angular frontend + .NET backend), adapted to
the constitution-mandated BFF split and 8-project .NET solution layout. The Angular project lives at
the repository root under `bsdcpolls-frontend/` alongside the `src/` directory containing the .NET
solution projects. The `BSDCPolls.sln` file is at the repository root.

## Complexity Tracking

> No constitution violations requiring justification. All principles pass without exception.
