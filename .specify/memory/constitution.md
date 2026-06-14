<!--
SYNC IMPACT REPORT
==================
Version change: 1.9.0 → 2.0.0

Reason for MAJOR bump: Backward-incompatible redefinition of Principle I. The absolute
"Zero custom CSS" prohibition has been replaced with a controlled SCSS policy that permits
SCSS strictly for Angular Material theming, responsive layout overrides, and component
encapsulation — while adding an unconditional `::ng-deep` ban. This changes the governing
rule; code that was previously prohibited (any SCSS) is now permitted under defined conditions,
and code that was previously permitted implicitly (`::ng-deep` was not called out) is now
explicitly banned. Principle V is also materially expanded with a Google Material Design 3
mandate, stronger WCAG requirements, and a comprehensive responsive testing protocol.

Modified principles:
  - I.  "Angular Material Only" → "Angular Material + Controlled SCSS"
        Old: Zero custom CSS of any kind, PROHIBITED entirely.
        New: SCSS is permitted for theming, responsive overrides, and component layout.
             `::ng-deep` is absolutely prohibited. Inline styles remain prohibited.
  - V.  "Accessibility & Responsive Design" (expanded)
        New: Explicit Google Material Design 3 mandate, WCAG 2.1 AA minimum
             (AAA where reasonably achievable), screen reader testing mandate,
             breakpoint testing extended to 5 named breakpoints.

Added sections: None.

Removed constraints:
  - "CSS: Zero custom CSS, zero inline styles" in Technical Constraints updated to reflect
    the new SCSS-permitted-with-restrictions policy.

Templates reviewed:
  - .specify/templates/plan-template.md     ⚠ pending — Constitution Check section
                                              references Principle I by old name.
                                              Manual update recommended.
  - .specify/templates/spec-template.md     ✅ No changes needed.
  - .specify/templates/tasks-template.md    ✅ No changes needed.

Follow-up TODOs: None.
-->

# BSDCPolls Constitution

## Core Principles

### I. Angular Material + Controlled SCSS (NON-NEGOTIABLE)

All UI MUST be built on Angular Material components and its theming system. Angular Material
is the primary and mandatory UI framework; no other UI component library may be used without a
constitution amendment.

**SCSS is permitted** under the following conditions only:
- Angular Material theming: customising the Material Design 3 colour scheme, typography scale,
  density, and design tokens via `@use '@angular/material' as mat` and `mat.define-theme(...)`.
- Component-scoped SCSS for layout properties (margin, padding, gap, flex/grid) that cannot be
  expressed with Material layout primitives or Angular CDK. Every such SCSS block MUST carry
  an inline comment explaining why a Material primitive is insufficient.
- Responsive overrides at Angular Material's named breakpoints (`xs`, `sm`, `md`, `lg`, `xl`)
  using `@media` queries. Breakpoints MUST align with Material Design 3 adaptive layout
  guidelines.

**Absolutely PROHIBITED — no exceptions:**
- `::ng-deep` in any form, in any file, for any reason. Shadow-DOM piercing defeats Angular's
  view encapsulation and is permanently banned across the entire codebase.
- Inline styles via `[style]` bindings or the `style` attribute in templates.
- Global CSS overrides that bypass Angular Material's theming API.
- Utility-class CSS libraries (Tailwind, Bootstrap, UnoCSS, etc.).
- Importing or applying third-party CSS/SCSS files outside of Angular Material and CDK.

**Rationale**: Restricting SCSS to theming and layout-only overrides preserves the benefits
of a design-system-driven UI (consistency, accessibility, dark-mode support) while allowing
the necessary ergonomic control over spacing and responsive behaviour. The absolute `::ng-deep`
ban is non-negotiable: once shadow-DOM piercing enters a codebase it proliferates uncontrolled,
destroys encapsulation, and creates unmaintainable specificity wars.

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

### V. Accessibility, Responsive Design & Material Design 3 (NON-NEGOTIABLE)

**Accessibility — maximum focus**

Accessibility is a first-class requirement in this project, not an afterthought. All UI MUST
meet or exceed **WCAG 2.1 Level AA** as a minimum; WCAG 2.1 Level AAA MUST be achieved
wherever it is reasonably implementable without architectural compromise.

Mandatory accessibility requirements:
- All interactive elements MUST be fully operable via keyboard alone (Tab, Enter, Space, Arrow
  keys) with a visible focus indicator at all times.
- Every non-decorative image and icon MUST have a meaningful `alt` attribute or `aria-label`.
- All form fields MUST have associated `<label>` elements or `aria-labelledby` references; error
  messages MUST be linked with `aria-describedby`.
- Colour contrast ratios MUST meet WCAG AA (4.5:1 for normal text, 3:1 for large text and UI
  components). Do not rely on colour alone to convey information.
- Angular Material's built-in ARIA roles, focus management, and keyboard navigation MUST NOT be
  overridden, bypassed, or suppressed.
- Live regions (`aria-live`, `aria-atomic`) MUST be used for real-time vote count updates so
  screen readers announce changes without requiring focus movement.
- Screen reader compatibility MUST be manually verified with at least one screen reader
  (VoiceOver on macOS or NVDA on Windows) before a feature is marked complete.

**Google Material Design 3 compliance**

All UI decisions (layout, typography, colour, spacing, interaction patterns, motion) MUST
follow **Google's official Material Design 3 guidelines** (`m3.material.io`). Angular Material
implements M3; use its components and theming API as the implementation vehicle for M3
compliance. Deviations from M3 patterns MUST be explicitly justified.

Specific M3 obligations:
- Use M3 colour roles (`primary`, `on-primary`, `surface`, `on-surface`, etc.) exclusively —
  hard-coded hex/rgb colour values in SCSS are PROHIBITED.
- Use M3 typography scale tokens (`display-large`, `headline-medium`, `body-small`, etc.) via
  Angular Material's typography system — hard-coded `font-size` or `font-weight` values in
  SCSS are PROHIBITED.
- Use M3 elevation and shape tokens for surfaces, cards, and dialogs — hard-coded
  `box-shadow` or `border-radius` values in SCSS are PROHIBITED.
- Motion and transitions MUST use M3 motion duration and easing tokens where Angular Material
  exposes them.

**Responsive design — all screen sizes**

All screens MUST be fully functional and visually correct at every standard breakpoint:

| Breakpoint | Range | Verification required |
|---|---|---|
| `xs` (phone portrait) | 320 px – 599 px | ✅ |
| `sm` (phone landscape / small tablet) | 600 px – 959 px | ✅ |
| `md` (tablet) | 960 px – 1279 px | ✅ |
| `lg` (desktop) | 1280 px – 1919 px | ✅ |
| `xl` (large desktop) | 1920 px + | ✅ |

Responsive layouts MUST be implemented using Angular Material's responsive primitives (CDK
`BreakpointObserver`, `mat-grid-list`, Flex Layout, or CSS Grid within component SCSS).
Breakpoint verification MUST be performed before a feature is marked complete.

**Rationale**: BSDCPolls is used across devices ranging from budget phones to large monitors.
Material Design 3 provides a complete, Google-authored design system that solves colour, type,
spacing, motion, and accessibility together — adherence eliminates local design decisions and
ensures the product is coherent, accessible, and on-brand. The WCAG AAA aspiration signals
that this project treats accessibility as a genuine engineering concern, not a checkbox.

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

### XI. Observability & Structured Logging (NON-NEGOTIABLE)

**Structured logging in .NET (ILogger<T> + Serilog)**
All application code in `BSDCPolls.BFF`, `BSDCPolls.BFF.Business`, `BSDCPolls.Api`,
`BSDCPolls.Api.Business`, and `BSDCPolls.Api.Data` MUST use `ILogger<T>` exclusively.
Direct Serilog static calls (`Log.Information(...)`, `Log.Error(...)`) in application code
are PROHIBITED; Serilog is configured only at host startup as the `ILogger` provider and
sink. All log messages MUST use structured message templates with named placeholders — string
interpolation in log calls is PROHIBITED:

```csharp
// CORRECT — structured, queryable in SigNoz
_logger.LogError(ex, "Vote rejected for poll {PollId} by user {UserId}", pollId, userId);

// PROHIBITED — loses structure, not queryable
_logger.LogError($"Vote rejected for poll {pollId} by user {userId}");
```

Log levels MUST be used consistently: `Error` for exceptions and unexpected failures;
`Warning` for degraded-but-recoverable states; `Information` for significant domain events
(poll created, vote cast); `Debug` for detailed flow tracing useful in development.

**Exception handling — no swallowing, no bleeding**
Exceptions MUST NOT be swallowed. Empty `catch` blocks and `catch (Exception)` blocks
that do not log are PROHIBITED. Every caught exception MUST be logged at `Error` level with
the full exception object so the stack trace is captured:

```csharp
catch (Exception ex)
{
    _logger.LogError(ex, "Failed to process vote for poll {PollId}", pollId);
    throw; // or wrap and rethrow — never silently discard
}
```

API error responses sent to the Angular frontend MUST NOT include stack traces, exception
type names, or internal service names. Error responses MUST return: an appropriate HTTP
status code, a user-readable message, and the OpenTelemetry **trace ID** of the request.
The trace ID is the only link the user carries; support uses it to pull the full trace from
SigNoz.

**OpenTelemetry — three pillars**
All .NET services (BFF, API, MigrationWorker) MUST export traces, metrics, and logs via
OTLP to SigNoz. W3C TraceContext (`traceparent`) MUST be propagated on all inter-service
calls (BFF → API, API → Supabase). Every API error response MUST include the active trace
ID in a response header (e.g., `X-Trace-Id`) and in the error response body.

**Angular observability**
The Angular app MUST implement a global `ErrorHandler` that captures all unhandled errors
and reports them to a dedicated logging endpoint on the BFF, including: error message,
stack trace (safe — server-side only, not rendered to the user), current route, and
component context. An Angular HTTP interceptor MUST inject the `traceparent` header on all
outgoing requests so distributed traces span frontend → BFF → API.

**SigNoz orchestration**
SigNoz MUST be provisioned as a self-hosted Podman container registered in the Aspire
AppHost. The Aspire dashboard MUST surface a link to the SigNoz UI. No managed or external
observability SaaS is used in local development or CI.

**Rationale**: Abundant, structured logging is the primary debugging tool for a real-time
application where race conditions and async failures are hard to reproduce. Stack traces
MUST reach the log and MUST NOT reach the browser — the trace ID is the safe bridge between
a user-reported error and the full diagnostic context. OpenTelemetry's vendor-neutral OTLP
protocol and SigNoz's self-hosted model keep observability data in the project's control and
reproducible in the local dev environment.

### XII. Code Style & Linting Enforcement (NON-NEGOTIABLE)

**Linter failures are build failures.** A PR that introduces linter violations MUST NOT be
merged. All linters described below are configured to fail their respective build steps with
a non-zero exit code; CI MUST run all linters and fail the pipeline on any violation.

**C# formatting — CSharpier**
CSharpier is the pre-approved opinionated C# code formatter. All C# files MUST be
CSharpier-formatted. `dotnet csharpier --check .` MUST pass in CI; unformatted files are a
pipeline failure. CSharpier configuration lives at the solution root (`.csharpierrc.json`)
and applies to all projects — per-project overrides are PROHIBITED.

**C# style rules — StyleCop.Analyzers**
StyleCop.Analyzers MUST be referenced in `Directory.Build.props` at the solution root so
it applies uniformly to every .NET project without per-project opt-in. The rule set follows
the Microsoft C# Coding Conventions and Google C# Style Guide where they do not conflict with
ASP.NET Core conventions. A single `stylecop.json` at the solution root is the sole
StyleCop configuration file.

Suppressing a StyleCop rule is PROHIBITED unless:
1. The suppression appears in the shared solution-level `GlobalSuppressions.cs` (not in
   individual `.cs` files, except where the suppression is genuinely call-site-specific).
2. The `[SuppressMessage]` attribute carries a non-empty `Justification` parameter explaining
   precisely why the rule does not apply and confirming the same exception applies uniformly
   across the codebase.
3. The suppression is reviewed and approved during code review.

Per-project deviations from the shared StyleCop configuration are PROHIBITED. If a rule is
inappropriate for all projects, it is disabled at the solution root with justification — never
selectively per project.

**TypeScript/Angular formatting — Prettier**
Prettier is the pre-approved TypeScript, HTML, JSON, and SCSS formatter. A single
`.prettierrc` at the repository root governs all frontend files. `prettier --check .` MUST
pass in CI. Prettier overrides in nested directories or per-file pragma comments
(`// prettier-ignore`) require a code comment explaining the exception and are subject to
review.

**TypeScript/Angular linting — Angular ESLint**
`@angular-eslint/eslint-plugin` and `@angular-eslint/eslint-plugin-template` are the
pre-approved Angular linting packages. `ng lint` MUST pass with zero errors and zero
warnings (warnings are treated as errors via `--max-warnings 0`). A single
`eslint.config.js` or `.eslintrc.json` at the Angular project root governs all files.
`// eslint-disable` comments require an inline justification and are reviewed; blanket file-
level disable comments (`/* eslint-disable */`) are PROHIBITED.

**Rationale**: Consistent, automatically-enforced formatting eliminates all style debates
from code review, freeing it for substantive correctness and design feedback. Uniform
StyleCop rules across projects mean every C# developer reads and writes the same style
regardless of which service they are in. Build-failure-on-lint-violation makes the
standard non-negotiable rather than aspirational.

### XIII. Entity Framework Core Conventions (NON-NEGOTIABLE)

**No raw SQL**
`FromSqlRaw`, `FromSqlInterpolated`, `ExecuteSqlRaw`, `ExecuteSqlInterpolated`, and
`Database.ExecuteSql*` are PROHIBITED. All data access MUST go through EF Core LINQ
queries. LINQ method syntax (`Where`, `Select`, `Include`, `OrderBy`, etc.) is mandatory.
LINQ query syntax (`from x in … select`) is permitted only when method syntax would produce
genuinely unreadable expression trees, and MUST carry an inline comment explaining the
exception.

**ModelBuilder configuration**
All entity relationships MUST be explicitly configured in `OnModelCreating` using the Fluent
API — relying on EF Core's naming conventions alone for relationships is PROHIBITED. Every
relationship MUST declare bidirectional navigation properties on both sides and MUST use
`HasForeignKey` to name the typed FK property explicitly:

```csharp
entity.HasMany(p => p.Votes)
      .WithOne(v => v.Poll)
      .HasForeignKey(v => v.PollId)
      .OnDelete(DeleteBehavior.Cascade);
```

Typed FK properties (e.g., `public int PollId { get; private set; }`) MUST exist on the
dependent entity alongside the navigation property. Shadow FK properties are PROHIBITED.

**Entity encapsulation pattern**
All entity property setters MUST be `private set` or `init`. Public property setters are
PROHIBITED. The public parameterless constructor is PROHIBITED; use a `protected` EF Core
constructor (for proxy support) plus a `private` full constructor called by the factory.

Entity creation MUST use a `public static` factory method that validates inputs and returns
the new entity. The factory method is responsible for calling `DbSet.Add`:

```csharp
// In the service layer:
var poll = Poll.Create("My Poll", createdByUserId, dbContext);
await dbContext.SaveChangesAsync();
```

All post-creation state mutations MUST go through named instance methods — direct property
assignment from outside the entity class is PROHIBITED:

```csharp
// CORRECT
poll.UpdateTitle("New Title", updatedByUserId);

// PROHIBITED
poll.Title = "New Title";
```

**Universal AuditableEntity base class**
Every entity in `BSDCPolls.Api.Data` MUST inherit from `AuditableEntity`. No entity may
omit any of these properties:

- `int Id` — integer primary key (auto-increment, internal use and FK references only)
- `Guid Uid` — unique GUID generated in C# at creation (`Guid.NewGuid()`); the only ID
  exposed to the frontend and external APIs (prevents integer enumeration)
- `bool IsActive` — soft-delete flag; defaults to `true`; hard deletes are PROHIBITED
- `DateTime CreatedOn` — UTC timestamp set by `SaveChangesInterceptor` on INSERT
- `int CreatedById` — FK to `ApplicationUser.Id`; set once on creation, never updated
- `ApplicationUser CreatedBy` — navigation property
- `DateTime UpdatedOn` — UTC timestamp set by `SaveChangesInterceptor` on every UPDATE
- `int UpdatedById` — FK to `ApplicationUser.Id`; updated on every change
- `ApplicationUser UpdatedBy` — navigation property

`CreatedOn`/`UpdatedOn`/`CreatedById`/`UpdatedById` MUST be populated automatically by a
`SaveChangesInterceptor` that reads from `ICurrentUserContext` — manual assignment in
service code is PROHIBITED.

**Loading strategy**
Lazy loading MUST be enabled globally via `UseLazyLoadingProxies()`. All entity navigation
properties MUST be declared `virtual` to support proxy generation.

Eager loading (`Include`/`ThenInclude`) MUST be used when the query is in a service method
that is known to access related data. The choice MUST be documented with an inline comment:

```csharp
// Eager-load votes: this endpoint renders the full poll result breakdown.
var poll = await dbContext.Polls
    .Include(p => p.Votes)
    .FirstOrDefaultAsync(p => p.Uid == uid);
```

Using lazy loading in a loop or in any code that iterates a collection is PROHIBITED (N+1
prevention). All such call sites MUST use eager loading with explicit `Include`.

**PostgreSQL audit history (temporal table equivalent)**
SQL Server's temporal tables do not exist in PostgreSQL. The mandated equivalent is an EF
Core `SaveChangesInterceptor` that writes a row to an `AuditLog` table on every INSERT,
UPDATE, and DELETE. Each `AuditLog` row stores: `EntityType`, `EntityId` (int), `EntityUid`
(GUID), `Action` (Create/Update/Delete), `ChangedProperties` (JSONB), `OldValues` (JSONB),
`NewValues` (JSONB), `ChangedOn` (UTC), `ChangedById`. This provides a full change history
queryable from the application layer. The `AuditLog` table is append-only; rows MUST NOT be
updated or deleted by application code.

**Rationale**: The no-raw-SQL rule keeps all data access in strongly-typed, refactorable
LINQ. Private setters + factory + instance methods enforce the invariant that an entity is
always in a valid state — no external code can leave it half-constructed. The dual-key
pattern (int Id + Guid Uid) keeps joins fast internally while exposing non-guessable
identifiers publicly. The universal `AuditableEntity` ensures every table is audit-ready
from commit one; the interceptor-based audit log gives PostgreSQL full temporal history
without requiring a schema extension.

### XIV. Interface-Driven Design (NON-NEGOTIABLE)

Every non-trivial class in `BSDCPolls.BFF.Business`, `BSDCPolls.Api.Business`, and
`BSDCPolls.Api.Data` MUST implement a corresponding interface. The **sole exception** is
ASP.NET Core controllers — controllers are framework infrastructure and do not require an
interface.

**Naming and co-location**
Interfaces follow the standard .NET `I`-prefix convention: `IPollService` → `PollService`,
`IVoteRepository` → `VoteRepository`. The interface file MUST be co-located with its
implementation in the same project and namespace (e.g., `Services/IPollService.cs` alongside
`Services/PollService.cs`). Interfaces MUST NOT be placed in a separate `Interfaces/`
folder — proximity to the implementation is deliberate and aids discoverability.

**DI registration and injection**
All DI registrations MUST reference the interface, never the concrete type:

```csharp
// CORRECT
services.AddScoped<IPollService, PollService>();

// PROHIBITED — bypasses the abstraction
services.AddScoped<PollService>();
```

Constructor parameters MUST declare the interface type, never the concrete type. Direct
instantiation of service classes (`new PollService(...)`) outside of tests is PROHIBITED.

**What requires an interface**
- All business/domain service classes in `*.Business` projects
- All data-access classes in `BSDCPolls.Api.Data` (query services, interceptors, context
  wrappers, audit writers)
- All infrastructure helpers injected via DI (e.g., `ICurrentUserContext`,
  `IDateTimeProvider`, `ISignalRNotifier`)
- All BFF coordination services

**What does NOT require an interface**
- ASP.NET Core controllers (explicit exception — framework convention)
- EF Core `DbContext` subclass (EF Core's own DI handles registration)
- Static utility classes with no DI dependencies
- Entity classes (domain objects, not services)

**Rationale**: Interface-driven design makes every dependency a seam — swappable and
independently evolvable. It enforces the Dependency Inversion Principle at the architectural
level: higher-layer code (Business) depends on abstractions, not concretions in lower layers
(Data).

### XV. No Test Projects (NON-NEGOTIABLE)

**No unit tests, integration tests, e2e tests, or test files of any kind MUST be written
or committed to this repository — for either the frontend or the backend.**

This is an explicit, unconditional architectural decision. Quality is achieved through:
- Self-documenting code with mnemonic, intention-revealing names (Principle IX)
- Strong type systems (TypeScript strict mode, C# nullable reference types)
- Compile-time safety (interface contracts, FluentValidation, EF Core model validation)
- Automated linting and formatting (Principle XII)
- Architectural pattern enforcement (layer boundaries, entity factory pattern, no raw SQL)

No test project files (`.spec.ts`, `*Tests.csproj`, `*Test.csproj`, `tests/` directories,
`__tests__/` directories) MUST be created, and no test libraries MUST be added as
dependencies. Any AI-generated test code MUST NOT be committed. Pull requests containing
test files MUST be rejected.

**Rationale**: The user has made a deliberate, informed decision that the return on
investment for AI-generated unit tests in this project does not justify the maintenance
overhead they create. Code quality is the responsibility of the design, type system,
linting, and code review — not of a test suite that mirrors the implementation.

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
- **SCSS policy**: SCSS is permitted for Angular Material theming, responsive layout overrides,
  and component-scoped layout properties only (Principle I). `::ng-deep` is ABSOLUTELY
  PROHIBITED in any form. Inline styles (`[style]` bindings) are PROHIBITED. Utility-class
  CSS libraries (Tailwind, Bootstrap, etc.) are PROHIBITED. Hard-coded colour, font-size,
  font-weight, box-shadow, or border-radius values in SCSS are PROHIBITED — use M3 design
  tokens exclusively.
- **Design system**: Google Material Design 3 (`m3.material.io`). All layout, typography,
  colour, spacing, and interaction decisions MUST follow M3 guidelines. Angular Material is
  the implementation vehicle.
- **Accessibility**: WCAG 2.1 AA minimum, AAA aspirational. Screen reader verification
  required before feature sign-off. Live regions for real-time updates mandatory.
- **No testing libraries**: Angular Testing Library, Jest, Jasmine, Karma, Playwright,
  xUnit, NUnit, MSTest, and all other test frameworks are PROHIBITED — do not install,
  reference, or configure any testing library (Principle XV).
- **Validation**: FluentValidation is pre-approved for `BSDCPolls.Contracts` (validator
  definitions) and for `BSDCPolls.BFF` / `BSDCPolls.Api` (DI registration only). No other
  validation library may be used for cross-boundary payload validation.
- **TypeScript generation**: NSwag CLI is the pre-approved tool for generating TypeScript
  types and API clients from the BFF OpenAPI schema. The generation MUST run as part of the
  build pipeline. Hand-written TypeScript that duplicates C# contract types is PROHIBITED.
- **Prohibited libraries**: AutoMapper, Mapster, TinyMapper, and all convention-based
  object-mapping libraries are PROHIBITED. Explicit, hand-written mapping code is mandatory.
- **Observability backend**: Self-hosted SigNoz. Provisioned as a Podman container via
  Aspire. Receives OTLP (traces, metrics, logs) from all .NET services. No external
  observability SaaS in local or CI environments.
- **Logging provider**: Serilog (configured at host startup only as the `ILogger` sink).
  Application code uses `ILogger<T>` exclusively — never Serilog static methods.
- **Telemetry SDK**: OpenTelemetry .NET SDK (`OpenTelemetry.*` packages). Traces, metrics,
  and logs MUST be exported via OTLP. W3C TraceContext propagation is mandatory on all
  inter-service calls.
- **Angular error reporting**: A global Angular `ErrorHandler` implementation MUST exist
  that reports unhandled errors to the BFF logging endpoint. An HTTP interceptor MUST
  inject `traceparent` on all outgoing requests.
- **C# formatter**: CSharpier. Single `.csharpierrc.json` at solution root. `dotnet
  csharpier --check .` MUST pass in CI.
- **C# style linter**: StyleCop.Analyzers via `Directory.Build.props` (solution-wide).
  Single `stylecop.json` + solution-level `GlobalSuppressions.cs`. No per-project rule
  deviations without solution-wide justification.
- **TypeScript/HTML/SCSS formatter**: Prettier. Single `.prettierrc` at repository root.
  `prettier --check .` MUST pass in CI.
- **TypeScript/Angular linter**: Angular ESLint (`@angular-eslint/*`). `ng lint
  --max-warnings 0` MUST pass in CI. No blanket file-level ESLint disables.
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

**AI coding skills — automatic activation**
The following Claude Code AI skills are installed and activate **automatically** at the
start of any implementation task. No manual invocation is required or expected.

- **Angular skill** (`https://github.com/angular/skills`) — auto-loaded before writing
  any Angular TypeScript, component, service, store, or template code.
- **.NET skill** (`https://github.com/dotnet/skills`) — auto-loaded before writing any
  C# code across BFF, API, Contracts, Data, or Aspire projects.

Auto-activation is configured in `CLAUDE.md`. These skills are the implementation-level
authority for language patterns; this constitution is the architectural and governance
authority. Both apply simultaneously — the skills govern *how* to write code; the
constitution governs *what rules* the code must follow.

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

All PRs MUST verify compliance with Principles I (Angular Material + Controlled SCSS),
II (Reactive-First), V (Accessibility, Responsive Design & Material Design 3), VI (BFF
Architecture), VII (IaC & Environment Parity), VIII (Layered .NET Architecture), IX (Code
Quality & Maintainability First), X (Contract-Driven Validation & TypeScript Generation),
XI (Observability & Structured Logging), XII (Code Style & Linting Enforcement), XIII (EF
Core Conventions), XIV (Interface-Driven Design), and XV (No Test Projects) in the
Constitution Check section of `plan.md`.
Principle IX applies to every line of every PR. Any new Contract DTO touching the frontend
boundary MUST have a co-located FluentValidation validator (Principle X). Any new service
endpoint MUST include structured logging for the happy path and all error branches (Principle
XI). All linters MUST pass — failures are merge blockers (Principle XII). Every new entity
MUST extend `AuditableEntity`, use private setters, and have a static Create factory method;
no raw SQL; N+1 must be explicitly prevented with eager loading (Principle XIII). Every
Angular PR MUST verify: no `::ng-deep`, no inline styles, no hard-coded colour/size values
in SCSS, all breakpoints render correctly, WCAG AA contrast ratios pass (Principle I + V).
PRs containing any test files MUST be rejected (Principle XV).
Complexity exceptions MUST be justified in the plan's Complexity Tracking table.
Use `CLAUDE.md` for runtime agent guidance.

**Version**: 2.0.0 | **Ratified**: 2026-06-10 | **Last Amended**: 2026-06-13
