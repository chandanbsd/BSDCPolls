<!--
SYNC IMPACT REPORT
==================
Version change: 1.4.0 → 1.4.1

Reason for PATCH bump: Principle X clarified to make explicit that C# FluentValidation is
the single authoritative source of truth for ALL validation rules — including frontend Angular
form validators. Angular form validators must mirror FluentValidation rules exactly; the C#
side is the definition, the TypeScript side is the enforcement replica. NSwag section
expanded to describe the validation metadata bridge and the manual-sync requirement for
complex cross-field rules.

Modified principles:
  - X. Contract-Driven Validation & TypeScript Generation — clarified validation single-source-
    of-truth intent; expanded NSwag section; added Angular form sync rules.

Added sections: None

Removed sections: None

Templates reviewed:
  - .specify/templates/plan-template.md     ✅ No changes needed.
  - .specify/templates/spec-template.md     ✅ No changes needed.
  - .specify/templates/tasks-template.md    ✅ No changes needed.

Follow-up TODOs: None.
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
communication only. The BFF is responsible for: hosting the SignalR hub that the frontend
connects to, enforcing Supabase auth token validation at the edge, and forwarding domain
requests to the backend API. All BFF → backend API calls MUST use internal HTTPS or gRPC;
raw SignalR proxying from BFF to backend is PROHIBITED. No business logic MUST be duplicated
between BFF and backend — the BFF acts as a thin translation and security layer.

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

### VIII. Layered .NET Project Architecture (NON-NEGOTIABLE)

All .NET code MUST follow a strict layered project structure. No project may reference a project
in a layer above its own. The mandated solution layout is:

*BFF tier*
- `BSDCPolls.BFF` — ASP.NET Core Web API; entry point, controllers, SignalR hub; references
  `BSDCPolls.BFF.Business` and `BSDCPolls.Contracts` only.
- `BSDCPolls.BFF.Business` — class library; all BFF business logic, auth forwarding, service
  orchestration; references `BSDCPolls.Contracts` only.

*Backend API tier*
- `BSDCPolls.Api` — ASP.NET Core Web API; entry point and controllers; references
  `BSDCPolls.Api.Business` and `BSDCPolls.Contracts` only.
- `BSDCPolls.Api.Business` — class library; all domain logic; references `BSDCPolls.Api.Data`
  and `BSDCPolls.Contracts`.
- `BSDCPolls.Api.Data` — class library; EF Core `DbContext`, entity classes, migrations,
  repository implementations; references `BSDCPolls.Contracts` for shared value types only.

*Shared*
- `BSDCPolls.Contracts` — class library; all request/response DTOs, gRPC message types, and
  SignalR event payload types that cross the BFF ↔ API boundary. Has zero dependencies on other
  solution projects. Neither BFF nor API may define ad-hoc inter-service types outside this
  library.

*Aspire orchestration*
- `BSDCPolls.AppHost` — .NET Aspire AppHost; declares all services, containers, and references.
- `BSDCPolls.MigrationWorker` — Aspire worker service; runs EF Core Migrations at startup
  before the API becomes available (see Technical Constraints).

**Rationale**: Enforcing project-level layer boundaries via .csproj references makes illegal
dependencies a compile error, not a code-review finding. The Contracts library eliminates
implicit coupling between services — every shared type has a single authoritative definition.

### IX. Code Quality & Maintainability First (NON-NEGOTIABLE — OVERARCHING PRINCIPLE)

This is the highest-priority principle in this constitution and the explicit, intentional
charter of the project. BSDCPolls is a **software engineering excellence project first**;
a usable product second. When any decision creates tension between code quality and delivery
speed, **code quality MUST win, without exception**.

The following rules are non-negotiable across all .NET and Angular code:

**Zero-warning builds**
- All .NET projects MUST set `<TreatWarningsAsErrors>true</TreatWarningsAsErrors>` and
  `<Nullable>enable</Nullable>`. No warnings are acceptable in any build configuration.
- The Angular project MUST compile with `"strict": true` in `tsconfig.json` and
  `strictTemplates: true` in `angularCompilerOptions`.

**No unjustified suppressions**
- `#pragma warning disable`, `[SuppressMessage]`, `// @ts-ignore`, and
  `// eslint-disable[-next-line]` directives MUST NOT be committed without an inline
  comment at the same location explaining precisely why the suppression is safe.
- The null-forgiving operator (`!`) MUST NOT be used without an inline comment proving
  null is impossible at that point.
- Each PR review MUST check that the suppression count has not increased without approval.

**Mandatory documentation**
- All `public` and `internal` .NET APIs MUST have XML documentation comments.
- All exported Angular services, components, and store features MUST have JSDoc comments.

**No committed noise**
- Commented-out code MUST NOT appear in any committed changeset.
- `// TODO` comments MUST NOT be merged to main — they become tracked issues before merge.

**Quality gates over velocity**
- A feature delivered slowly with excellent code is ACCEPTABLE and PREFERRED.
- A feature delivered quickly with quality shortcuts MUST be reverted or refactored before
  it may be considered complete. Speed is not a justification for lowering standards.
- No "we'll fix it later" exemptions — later never comes, and debt compounds.

**Rationale**: Technical debt incurred at the foundation of a project compounds faster than
anywhere else. Establishing and mechanically enforcing zero-tolerance quality standards from
commit one is orders of magnitude cheaper than retrofitting them later. This principle is
the user's deliberate, informed choice — it is not subject to trade-off negotiation.

### X. Contract-Driven Validation & TypeScript Generation (NON-NEGOTIABLE)

**C# FluentValidation is the single source of truth for all validation rules.**
Every validation rule that a user must satisfy before submitting a form EXISTS FIRST in a
C# FluentValidation `AbstractValidator<T>` in `BSDCPolls.Contracts`. The Angular reactive
form validators are a replica of those rules — they enforce the same constraints client-side
for UX, but the C# side is the definition. If the two ever diverge, the C# rule is correct
and the Angular rule MUST be updated to match.

**FluentValidation validators in Contracts**
Every request payload type in `BSDCPolls.Contracts` that originates from the frontend MUST
have a co-located `AbstractValidator<T>` in the same project. Validators MUST be exhaustive
— they encode every constraint a user must satisfy, including: required fields, max/min
lengths, value ranges, format rules, conditional rules, and cross-field rules (e.g., "field A
and field B may each be null, but both MUST NOT be null simultaneously"). Thin validators
that only check `RuleFor(x => x.Field).NotEmpty()` on every field are PROHIBITED; the
validator is the formal specification of valid input.

Validator DI registration occurs in `BSDCPolls.BFF` (for frontend-facing validation) and
`BSDCPolls.Api` (for defense-in-depth). Validator class definitions live exclusively in
`BSDCPolls.Contracts`.

Note: EF Core entity classes in `BSDCPolls.Api.Data` use Data Annotations for schema
constraints (Principle VIII). FluentValidation governs cross-boundary payload validation
in Contracts. The two are complementary, not competing.

**NSwag TypeScript generation and Angular form sync**
All TypeScript types, API client code, and — where the OpenAPI schema supports it —
validation metadata MUST be auto-generated from the BFF's OpenAPI specification using NSwag.
Hand-written TypeScript that duplicates C# contract types is PROHIBITED. The NSwag generation
step MUST run as part of the build pipeline; generated files MUST be committed to the
repository so type regressions appear as diffs in PRs.

For simple rules expressible in OpenAPI (required, minLength, maxLength, pattern, minimum,
maximum), NSwag-generated types carry the metadata and Angular form validators MUST be
derived from it — no hand-coded duplication.

For complex rules that OpenAPI cannot express (cross-field constraints, conditional
nullability, business-rule interdependencies), Angular MUST implement a custom reactive form
validator that replicates the C# FluentValidation rule exactly. Each such Angular validator
MUST carry a code comment of the form:
```
// Mirrors: BSDCPolls.Contracts.<ValidatorClass>.<RuleName>
```
This comment is the traceability link that code review uses to verify the frontend rule
matches its C# authoritative source.

**AutoMapper ban**
AutoMapper and all similar convention-based object-mapping libraries (Mapster, TinyMapper,
etc.) are PROHIBITED across the entire solution. All object-to-object mapping MUST be
explicit, hand-written code — preferably static factory methods (`DTO.From(entity)`) or
extension methods. Explicit mapping fails at compile time when a property is renamed;
implicit mapping fails silently at runtime.

**Rationale**: Forms are a trust boundary. If a user can submit a payload that the backend
rejects, the UX is broken. If the backend accepts a payload the frontend thought was valid,
security is broken. Deriving Angular form validators from the same FluentValidation rules
that the backend enforces eliminates this entire class of frontend/backend disagreement. The
C# contract is the ground truth; NSwag and the traceability comments are the enforcement
mechanism that keeps Angular in lockstep.

## Technical Constraints

- **Frontend**: Angular (latest stable LTS), Angular Material, RxJS, NgRX Signal Store, Angular
  Signals, `@microsoft/signalr`. No other UI or state libraries without a constitution amendment.
- **Real-time transport**: SignalR. Frontend uses `@microsoft/signalr`; BFF and backend use
  ASP.NET Core SignalR hubs.
- **Backend services** (both latest stable):
  - *BFF*: ASP.NET Core Web API, C# — internet-facing, hosts SignalR hub, validates Supabase
    JWT, calls backend API via internal HTTPS or gRPC.
  - *Backend API*: ASP.NET Core Web API, C#, Entity Framework Core — internal only, owns domain
    logic and database access; exposes gRPC and/or HTTPS endpoints for BFF consumption only.
- **BFF → API transport**: Internal HTTPS or gRPC. Raw SignalR proxying to the backend is
  PROHIBITED. gRPC is preferred for strongly-typed service contracts; HTTPS is acceptable for
  simpler pass-through endpoints.
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
- **Validation**: FluentValidation is pre-approved for `BSDCPolls.Contracts` (validator
  definitions) and for `BSDCPolls.BFF` / `BSDCPolls.Api` (DI registration only). No other
  validation library may be used for cross-boundary payload validation.
- **TypeScript generation**: NSwag CLI is the pre-approved tool for generating TypeScript
  types and API clients from the BFF OpenAPI schema. The generation MUST run as part of the
  build pipeline. Hand-written TypeScript that duplicates C# contract types is PROHIBITED.
- **Prohibited libraries**: AutoMapper, Mapster, TinyMapper, and all convention-based
  object-mapping libraries are PROHIBITED. Explicit, hand-written mapping code is mandatory.
- **Entity validation**: Every EF Core entity class MUST declare all validation and schema
  constraints via Data Annotations within the same `.cs` file (e.g., `[Required]`,
  `[MaxLength]`, `[Range]`). Separate validation classes and standalone FluentValidation
  profiles applied to entity types are PROHIBITED. Fluent API in `OnModelCreating` is permitted
  only for schema details that cannot be expressed as attributes (e.g., composite primary keys,
  complex indexes, owned-entity configuration); it MUST NOT duplicate annotation-expressible
  constraints.
- **Schema migrations**: All database schema changes MUST be applied via EF Core Migrations.
  Manual SQL schema changes are PROHIBITED in all environments including local development and
  CI. Migrations MUST be executed by `BSDCPolls.MigrationWorker` — a dedicated Aspire-registered
  worker service — at startup, prior to the API accepting traffic. The API project MUST NOT call
  `Database.Migrate()` itself at runtime.

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

**Principle IX (Code Quality & Maintainability First) supersedes all other principles when
they conflict.** If following any other principle would require a quality shortcut, the
shortcut is not permitted — the other principle must be satisfied in a quality-compliant way
or the feature must be deferred until it can be.

Amendments require:
1. A documented rationale explaining why the existing principle is insufficient.
2. A semantic version bump (MAJOR for principle removal/redefinition, MINOR for additions,
   PATCH for clarifications).
3. Propagation of the change to all affected templates and this Sync Impact Report.
4. Review and explicit acceptance before the next feature spec is opened.

All PRs MUST verify compliance with Principles I (Angular Material Only), II (Reactive-First),
VI (BFF Architecture), VII (IaC & Environment Parity), VIII (Layered .NET Architecture),
IX (Code Quality & Maintainability First), and X (Contract-Driven Validation & TypeScript
Generation) in the Constitution Check section of `plan.md`. Principle IX applies to every
line of every PR. Any new Contract DTO touching the frontend boundary MUST have a co-located
FluentValidation validator (Principle X). Complexity exceptions MUST be justified in the
plan's Complexity Tracking table. Use `CLAUDE.md` for runtime agent guidance.

**Version**: 1.4.1 | **Ratified**: 2026-06-10 | **Last Amended**: 2026-06-10
