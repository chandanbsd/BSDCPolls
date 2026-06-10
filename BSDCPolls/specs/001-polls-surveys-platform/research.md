# Research: BSDCPolls — Real-Time Polls & Surveys Platform

**Date**: 2026-06-10
**Phase**: 0 — Resolves all technical unknowns before Phase 1 design

---

## R-001: Supabase Auth Self-Hosted JWT Validation in BFF

**Decision**: Use the self-hosted Supabase GoTrue service as the auth provider. The BFF validates
incoming JWTs by fetching the JWKS (JSON Web Key Set) from the GoTrue instance at startup and
caching it. Token validation uses the `Microsoft.AspNetCore.Authentication.JwtBearer` package
pointed at the GoTrue issuer URL (the Aspire-registered GoTrue service base URL). The BFF never
calls external Supabase cloud endpoints.

**How the BFF validates**: `AddAuthentication().AddJwtBearer(options => { options.Authority =
"<GoTrue internal URL>"; options.Audience = "authenticated"; })`. The Angular frontend receives
a JWT from GoTrue on login (via the Supabase JS client) and sends it as `Authorization: Bearer
<token>` on every BFF request. The BFF validates the token signature and `exp` claim before
processing any authenticated route.

**Supabase JS client on Angular**: The `@supabase/supabase-js` package is used in the Angular
`AuthService` to call GoTrue for sign-up and sign-in. The generated access token is stored in
the NgRX Signal Store (auth slice) and injected by the token interceptor into all outgoing
BFF requests.

**Note on PII**: GoTrue requires an email by default. To satisfy FR-002 (zero PII), the BFF's
registration endpoint generates a synthetic email of the form `<generated-uid>@internal.bsdcpolls`
which is never shown to the user and treated as a non-PII system identifier. The user's
experience is: password only. Supabase's email confirmation flow MUST be disabled in GoTrue
configuration (confirmed via `MAILER_AUTOCONFIRM=true` environment variable in AppHost).

**Rationale**: Self-hosted GoTrue gives us full JWT issuance/validation within the Aspire
environment with no external network dependency. JwtBearer with a JWKS endpoint is the standard
ASP.NET Core pattern and avoids any Supabase SDK dependency in the .NET backend.

**Alternatives considered**:
- Using ASP.NET Core Identity as the auth system — rejected because Supabase Auth (GoTrue) is
  explicitly mandated by the constitution.
- Storing the JWT secret as a static key — rejected because JWKS rotation is a security best
  practice and JwtBearer supports it natively.

---

## R-002: Privacy-Preserving Username Generation

**Decision**: Implement username generation in `BSDCPolls.Api.Business` as an `IUsernameGenerator`
service backed by embedded word lists (adjective list ~1,500 words, noun list ~1,500 words).
Generated usernames follow the pattern `adjective-adjective-noun` (e.g., "swift-amber-moon").

**Profanity filtering**: The profanity filter uses a curated deny-list of English profane words
checked against each generated word component. If any component matches the deny-list, the
combination is discarded and a new one generated. The deny-list is embedded as a resource file
in `BSDCPolls.Api.Business`.

**Uniqueness guarantee**: After generating a candidate username, the service checks for uniqueness
against the `ApplicationUser` table. If a collision occurs (rare but possible), the service
retries generation up to 10 times before returning an error. The uniqueness check uses an
EF Core `AnyAsync` query on the unique index column.

**Username change requests**: When a user requests a change, the same `IUsernameGenerator` service
produces a new username. The old username is stored in `UsernameHistory` to prevent reuse within
90 days (preventing impersonation via recycled usernames).

**Rationale**: Embedded word lists avoid external API dependencies. The deny-list approach is
industry-standard (used by Bitwarden, Ente, and similar privacy-focused applications). The
`adjective-adjective-noun` pattern provides approximately 1,500 × 1,500 × 1,500 = 3.375 billion
combinations, making collisions effectively impossible at early scale.

**Alternatives considered**:
- Using UUID slugs (e.g., "abc-123") — rejected because they are not memorable or friendly.
- Using a third-party profanity library — rejected because an embedded deny-list gives full
  control and avoids an external dependency that could change its list without notice.

---

## R-003: AuditableEntity Bootstrapping for ApplicationUser

**Decision**: Seed a system sentinel user in the first migration (`Id = 1`, `Username = "system"`,
`IsActive = false`, `SupabaseUserId = "SYSTEM"`, `CreatedById = 1` (self-referential),
`UpdatedById = 1`). All `ApplicationUser` registrations set `CreatedById = 1` (the system user)
at row creation. The `SaveChangesInterceptor` that normally populates `CreatedById` from
`ICurrentUserContext` treats the registration flow specially: when no authenticated user exists
in context (i.e., during registration itself), it falls back to the system sentinel ID.

**Rationale**: The `AuditableEntity` `CreatedById` column is non-nullable and references
`ApplicationUser.Id`. Without a sentinel, the first real user's INSERT would fail the FK
constraint because no ApplicationUser exists yet. Seeding a system user in the initial migration
breaks the circular dependency cleanly.

**Alternatives considered**:
- Nullable `CreatedById` on `ApplicationUser` only — rejected because the constitution says
  "no entity may omit any of these properties," implying non-nullable.
- Self-referential on INSERT (set CreatedById = own Id) — rejected because the Id is
  not known until after the INSERT completes; this would require an additional UPDATE.

---

## R-004: JSONB Storage for Survey Question Trees with EF Core

**Decision**: The `Survey` entity stores its question tree as a JSONB column
(`QuestionTree jsonb NOT NULL`). EF Core maps this via a value converter that serializes/
deserializes a `SurveyQuestionTreeDocument` C# record using `System.Text.Json`. The column is
declared in `OnModelCreating` with `.HasColumnType("jsonb")`.

**Survey question tree structure** (C# record stored as JSONB):

```csharp
public sealed record SurveyQuestionTreeDocument(
    IReadOnlyList<SurveyQuestionNode> Questions
);

public sealed record SurveyQuestionNode(
    Guid Uid,
    string Text,
    SurveyAnswerType AnswerType,     // MultipleChoice | ShortText | LongText | DocumentUpload
    bool IsRequired,
    IReadOnlyList<SurveyChoiceOption>? Choices,  // null unless AnswerType = MultipleChoice
    IReadOnlyList<SurveyConditionalBranch>? Branches  // child questions with conditions
);

public sealed record SurveyChoiceOption(Guid Uid, string Text);

public sealed record SurveyConditionalBranch(
    Guid ParentChoiceUid,   // which choice on the parent must be selected
    IReadOnlyList<SurveyQuestionNode> Questions  // sub-questions to show
);
```

**Survey response storage**: `SurveyResponse.AnswersJson` (JSONB) stores the respondent's answers
as a list of `{ questionUid, answerType, value }` entries. PDF answers store a reference to the
`SurveyDocument` row UID rather than the bytes themselves.

**Querying**: Direct JSONB querying via EF Core's Npgsql JSONB operators is available but not
required in Phase 1. The primary access pattern is: load the full `Survey.QuestionTree` document,
deserialize in C#, and work with the tree in memory. If aggregate analytics require JSONB querying,
that is a future enhancement.

**Rationale**: JSONB in PostgreSQL is the constitution-mandated storage model for survey question
trees ("flexible document records within the relational database"). Using a value converter keeps
the EF Core entity model clean while respecting the no-raw-SQL constraint — the serialization/
deserialization is explicit C# code, not a raw SQL call.

**Alternatives considered**:
- A separate `SurveyQuestion` entity table with a self-referential `ParentQuestionId` FK —
  rejected because the spec explicitly states survey structure is stored as a document. The
  spec says "all the survey options as NoSQL documents within Postgres."
- EF Core owned entities with JSONB — rejected because owned entities have rigid mapping
  requirements that conflict with arbitrary nesting depth.

---

## R-005: PDF File Storage Strategy

**Decision**: Store PDF file bytes as a `bytea` column in a `SurveyDocument` entity in PostgreSQL.
Maximum file size enforced at 10 MB at the BFF controller level before the bytes reach the database.

**Serving PDFs back to authorized users**: A dedicated BFF endpoint streams the `FileData` column
bytes to the client with `Content-Type: application/pdf` and `Content-Disposition: attachment`.
The endpoint enforces authorization: only the survey creator and the respondent who uploaded
the file may download it.

**Constitution alignment**: The constitution mandates "Self-hosted Supabase (PostgreSQL and Auth)"
as the infrastructure primitives. Adding Supabase Storage would require additional containers
(Storage API service, MinIO/S3 container). Storing in PostgreSQL keeps the container count
minimal and the data co-located with all other application data, simplifying backup and
restore.

**Rationale**: At ≤ 10 MB per file, PostgreSQL `bytea` performs acceptably. The survey feature is
not expected to be a high-throughput file store in the initial scope. Keeping files in the same
PostgreSQL instance as all other data simplifies local development, backup, and Aspire environment
parity with zero additional containers.

**Alternatives considered**:
- Supabase Storage (self-hosted MinIO) — rejected for Phase 1 because it adds container
  complexity without a compelling need at the stated 10 MB cap. Can be revisited as a
  future enhancement.
- Local filesystem in the API container — rejected because it breaks the stateless-container
  principle and does not survive container restarts; no environment parity guarantee.

---

## R-006: SignalR Group Strategy for Live Polls

**Decision**: Use SignalR groups keyed by `pollUid.ToString()`. On connection to `PollHub`, the
BFF automatically adds the authenticated user to the group for the poll UID passed in the
connection query string (`?pollUid=<guid>`). On disconnect, SignalR automatically removes the
connection from all groups. The hub authorizes group membership by checking whether the user is
the creator, an invitee, or the poll is public.

**Question broadcasting**: When the creator calls `PushQuestion`, the BFF hub calls the backend
API (BFF → API) to persist the question, then broadcasts `QuestionPushed` to the entire group
(`Clients.OthersInGroup(pollUid)`). The creator receives a confirmation response on their own
connection, not via the group broadcast.

**Vote-count fan-out**: When a participant submits a vote via `SubmitVote`, the hub calls the
API to persist the vote, then pushes a `VoteCountUpdated` event to the creator's specific
connection ID (stored in the hub's in-memory `PollSessionTracker`) rather than the entire group.
This prevents broadcasting vote-count data to participants (information hiding).

**Connection tracking**: `PollSessionTracker` is a scoped in-memory service (`IPollSessionTracker`)
on the BFF that maps `pollUid → creatorConnectionId`. It is populated when the creator connects
and cleared when they disconnect. This is BFF-tier state only; no persistence required.

**Rationale**: Group-based broadcasting is the canonical SignalR pattern for multi-participant
real-time events. Keying groups by poll GUID ties directly to the domain model. Tracking creator
connection IDs server-side avoids sending vote totals to all participants.

**Alternatives considered**:
- A separate presence service (Redis backplane) for distributed SignalR — rejected because
  initial scope targets ~1,000 concurrent users, which a single BFF instance handles comfortably.
  Horizontal scaling with a Redis backplane is a documented future path.

---

## R-007: NotificationHub and Real-Time Invitation Delivery

**Decision**: A separate `NotificationHub` (connection URL: `/hubs/notifications`) handles
real-time invitation delivery. Each authenticated user connects to this hub and is added to a
personal group keyed by `userId.ToString()`. When an invitation is created in the backend API,
the backend calls a BFF notification endpoint (internal HTTP POST) with the target user's ID and
the invitation payload. The BFF then pushes `InvitationReceived` to the target user's personal
group.

**Why a separate hub from PollHub**: The notification stream is global and persistent (always
connected for any logged-in user), whereas PollHub connections are poll-session-specific and
transient. Mixing them would complicate connection lifecycle management.

**Fallback for offline users**: If the target user is not currently connected to the
`NotificationHub`, the invitation is still persisted in the `Invitation` and `Notification`
tables. On the user's next app load, the Angular app calls `GET /api/notifications` to fetch
unread notifications and hydrates the store. The SignalR push is therefore an enhancement,
not the sole delivery mechanism.

**Rationale**: The 5-second delivery SLA (SC-007) is met by the push path. The REST fallback
ensures no notifications are lost for offline users.

---

## R-008: NSwag TypeScript Generation Build Pipeline

**Decision**: The NSwag generation pipeline works as follows:

1. **BFF project generates `openapi.json`** at build time using NSwag's document generation
   via a .NET CLI tool (`dotnet nswag aspnetcore2openapi`) configured in `nswag.json` at the
   `BSDCPolls.BFF` project root. This produces `openapi.json` committed alongside the BFF project
   and re-generated whenever the BFF is rebuilt in CI.

2. **Angular project reads `openapi.json` and generates TypeScript** using the NSwag CLI
   (`npx nswag run`) with a `nswag.json` at the `BSDCPolls.Web` root that points at the
   committed `openapi.json`. Generated files are output to `src/app/generated/` and committed
   to the repository.

3. **CI pipeline step order**: `dotnet build → dotnet nswag (generate openapi.json) → npx nswag
   run (generate TS) → ng build`. A diff on the generated files in CI acts as a type-regression
   detector.

**Developer workflow**: After changing any BFF contract, run `npm run generate-api` (defined in
`package.json` as the `dotnet nswag` + `npx nswag run` sequence). Commit the updated generated
files with the contract change PR.

**Rationale**: Committing the generated files means any breaking contract change is visible as
a diff in the PR, not a silent runtime failure. The two-step process (BFF generates JSON; Angular
reads JSON) decouples the TypeScript generation from a running BFF instance, enabling CI to run
the full pipeline without a live server.

---

## R-009: SigNoz Provisioning in Aspire AppHost

**Decision**: SigNoz is provisioned using the official SigNoz Docker/Podman image as a
`ContainerResource` in the Aspire AppHost. The OTLP endpoint (`4317` for gRPC, `4318` for HTTP)
is wired as a service reference. All .NET services (`BFF`, `Api`, `MigrationWorker`) register
OpenTelemetry with the OTLP exporter pointed at the Aspire-resolved SigNoz endpoint.

**SigNoz UI**: The Aspire dashboard link to the SigNoz UI (`http://localhost:3301`) is registered
as a well-known service endpoint so developers can navigate to it from the Aspire dashboard.

**Rationale**: Self-hosted SigNoz in Podman via Aspire ensures the observability stack is fully
local with no external SaaS dependency, and the OTLP endpoint is resolved via Aspire's service
discovery just like any other service.

---

## Summary of Decisions

| ID | Topic | Decision |
|----|-------|----------|
| R-001 | Supabase Auth JWT validation | `JwtBearer` with JWKS from self-hosted GoTrue; synthetic email for registration |
| R-002 | Username generation | Embedded word lists (adj-adj-noun); deny-list profanity filter; `IUsernameGenerator` |
| R-003 | AuditableEntity bootstrapping | System sentinel user seeded in migration (Id=1); registration interceptor falls back to sentinel |
| R-004 | JSONB for survey trees | EF Core value converter with `System.Text.Json`; `SurveyQuestionTreeDocument` record |
| R-005 | PDF storage | PostgreSQL `bytea` column on `SurveyDocument` entity; 10 MB cap at BFF controller |
| R-006 | SignalR poll groups | Group per `pollUid`; creator connection tracked in `IPollSessionTracker`; vote counts to creator only |
| R-007 | Notification hub | Separate `NotificationHub`; personal group per `userId`; REST fallback for offline users |
| R-008 | NSwag pipeline | BFF generates `openapi.json`; Angular reads committed JSON; `npm run generate-api` script |
| R-009 | SigNoz in Aspire | `ContainerResource` in AppHost; OTLP on 4317/4318; linked from Aspire dashboard |
