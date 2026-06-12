# Tasks: BSDCPolls — Real-Time Polls & Surveys Platform

**Input**: Design documents from `specs/001-polls-surveys-platform/`

**Prerequisites**: plan.md ✅ spec.md ✅ research.md ✅ data-model.md ✅ contracts/ ✅

**Tests**: NONE — Principle XV unconditionally prohibits all test files, test projects, and
test libraries for both frontend and backend.

**Organization**: Tasks grouped by user story to enable independent implementation and testing.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no shared dependencies on incomplete tasks)
- **[Story]**: Which user story this task belongs to (US1–US8)
- Exact file paths in every description

---

## Phase 1: Solution Scaffolding & Tooling

**Purpose**: Create the .NET solution structure, Angular project, and all configuration files.
No application code — only project files, manifests, and tooling configuration.

- [ ] T001 Create `.gitignore` at repo root covering .NET (bin/, obj/), Angular (node_modules/, dist/), IDE artifacts (.DS_Store, .idea/, .vscode/), secrets (.env*), and generated API client (BSDCPolls.Web/src/app/generated/) ← commit generated files but include in .gitignore for local-only overrides
- [ ] T002 Create `BSDCPolls.sln` at repo root referencing all 8 .NET projects under `src/`
- [ ] T003 Create `Directory.Build.props` at repo root: `<TreatWarningsAsErrors>true</TreatWarningsAsErrors>`, `<Nullable>enable</Nullable>`, StyleCop.Analyzers PackageReference, single `stylecop.json` path, solution-wide GlobalSuppressions.cs path
- [ ] T004 [P] Create `.csharpierrc.json` at repo root (empty object `{}` — CSharpier opinionated defaults)
- [ ] T005 [P] Create `stylecop.json` at repo root enabling SA documentation rules for public and internal members; disabling file header rules
- [ ] T006 [P] Create `GlobalSuppressions.cs` at repo root as empty C# file with solution assembly attribute scaffold
- [ ] T007 Create `src/BSDCPolls.Contracts/BSDCPolls.Contracts.csproj`: Class library, net9.0, references FluentValidation and System.Text.Json only; zero intra-solution project references
- [ ] T008 Create `src/BSDCPolls.Api.Data/BSDCPolls.Api.Data.csproj`: Class library, net9.0; references BSDCPolls.Contracts, EF Core 9 (Microsoft.EntityFrameworkCore, Npgsql.EntityFrameworkCore.PostgreSQL, Microsoft.EntityFrameworkCore.Proxies)
- [ ] T009 [P] Create `src/BSDCPolls.Api.Business/BSDCPolls.Api.Business.csproj`: Class library, net9.0; references BSDCPolls.Api.Data, BSDCPolls.Contracts
- [ ] T010 [P] Create `src/BSDCPolls.Api/BSDCPolls.Api.csproj`: ASP.NET Core Web API, net9.0; references BSDCPolls.Api.Business, BSDCPolls.Contracts; adds FluentValidation.AspNetCore, Serilog.AspNetCore, OpenTelemetry packages
- [ ] T011 [P] Create `src/BSDCPolls.BFF.Business/BSDCPolls.BFF.Business.csproj`: Class library, net9.0; references BSDCPolls.Contracts only
- [ ] T012 [P] Create `src/BSDCPolls.BFF/BSDCPolls.BFF.csproj`: ASP.NET Core Web API, net9.0; references BSDCPolls.BFF.Business, BSDCPolls.Contracts; adds Microsoft.AspNetCore.SignalR, Microsoft.AspNetCore.Authentication.JwtBearer, FluentValidation.AspNetCore, Serilog.AspNetCore, OpenTelemetry packages, NSwag.AspNetCore
- [ ] T013 [P] Create `src/BSDCPolls.AppHost/BSDCPolls.AppHost.csproj`: Aspire AppHost, net9.0; references Aspire.Hosting packages; references BSDCPolls.BFF and BSDCPolls.Api service projects via ProjectResource
- [ ] T014 [P] Create `src/BSDCPolls.MigrationWorker/BSDCPolls.MigrationWorker.csproj`: Worker service, net9.0; references BSDCPolls.Api.Data, BSDCPolls.Contracts; adds EF Core Design package
- [ ] T015 Create `BSDCPolls.Web/` Angular project: run `ng new BSDCPolls.Web --routing --style=scss --skip-tests --package-manager=npm` then remove all Karma/Jasmine entries from `package.json` and `angular.json`; install: `@angular/material`, `@ngrx/signals`, `@microsoft/signalr`, `@supabase/supabase-js`
- [ ] T016 [P] Configure `BSDCPolls.Web/tsconfig.json`: set `"strict": true`; add `angularCompilerOptions` with `strictTemplates: true`; set `noImplicitAny`, `strictNullChecks`, `strictFunctionTypes`
- [ ] T017 [P] Create `.prettierrc` at repo root: `{ "singleQuote": true, "printWidth": 120, "trailingComma": "all", "htmlWhitespaceSensitivity": "ignore" }`
- [ ] T018 [P] Create `BSDCPolls.Web/eslint.config.js` using `@angular-eslint/eslint-plugin` and `@angular-eslint/eslint-plugin-template`; set `max-warnings: 0`; add rule to flag `[style]` bindings
- [ ] T019 [P] Create `BSDCPolls.Web/nswag.json` pointing at BFF project's `openapi.json` output; configure output to `src/app/generated/api.ts`; add `"generate-api"` script to `BSDCPolls.Web/package.json`

**Checkpoint**: All project files exist; `dotnet restore` and `npm install` succeed; no application code yet

---

## Phase 2: Foundational Infrastructure (Blocking Prerequisites)

**Purpose**: Core infrastructure that ALL user stories depend on. No story can begin until this
phase is complete. Covers: EF Core base entities, DbContext, Aspire AppHost wiring, BFF JWT
middleware, Angular core services, and observability.

**⚠️ CRITICAL**: Complete every task in this phase before beginning any user story phase.

### .NET Contracts Layer

- [ ] T020 Create `src/BSDCPolls.Contracts/Enums/` directory and define all enums used across the solution in `src/BSDCPolls.Contracts/Enums/Enums.cs`: `PollStatus`, `SurveyStatus`, `SurveyAnswerType`, `InvitePermission`, `InvitationStatus` (see data-model.md for values)
- [ ] T021 [P] Create JSONB value object records in `src/BSDCPolls.Contracts/Documents/SurveyDocuments.cs`: `SurveyQuestionTreeDocument`, `SurveyQuestionNode`, `SurveyChoiceOption`, `SurveyConditionalBranch`, `SurveyAnswersDocument`, `SurveyAnswerEntry` — all as `sealed record` types with XML doc comments
- [ ] T022 [P] Create error response DTO in `src/BSDCPolls.Contracts/Responses/ApiErrorResponse.cs`: `ApiErrorResponse` record with `TraceId`, `Status`, `Message`, `Errors` (list of `ApiFieldError` with `Field?` and `Message`)

### EF Core Data Layer

- [ ] T023 Create `src/BSDCPolls.Api.Data/Entities/AuditableEntity.cs`: abstract base class with `int Id`, `Guid Uid`, `bool IsActive`, `DateTime CreatedOn`, `int CreatedById`, `virtual ApplicationUser CreatedBy`, `DateTime UpdatedOn`, `int UpdatedById`, `virtual ApplicationUser UpdatedBy` — all with private setters; XML doc comments on all properties
- [ ] T024 Create `src/BSDCPolls.Api.Data/Entities/ApplicationUser.cs`: extends AuditableEntity; properties `Username` (string, private set), `SupabaseUserId` (string, private set); Data Annotations: `[Required]`, `[MaxLength(60)]` on Username, `[MaxLength(200)]` on SupabaseUserId; static `Create(string username, string supabaseUserId, int systemUserId)` factory method; `UpdateUsername(string newUsername, int updatedById)` instance method; `Deactivate(int updatedById)` instance method; protected EF constructor; XML docs
- [ ] T025 [P] Create `src/BSDCPolls.Api.Data/Entities/UsernameHistory.cs`: extends AuditableEntity; `int UserId`, `virtual ApplicationUser User`, `string Username` (`[Required][MaxLength(60)]`), `DateTime RetiredAt`; static `Create` factory; XML docs
- [ ] T026 [P] Create `src/BSDCPolls.Api.Data/Entities/UserPrivacySettings.cs`: extends AuditableEntity; `int UserId`, `virtual ApplicationUser User`, `bool ShowPublicContent`, `InvitePermission InvitePermission`; static `CreateDefault(int userId, int createdById)` factory; `Update(bool showPublicContent, InvitePermission permission, int updatedById)` instance method; XML docs
- [X] T027 [P] Create `src/BSDCPolls.Api.Data/Entities/InviteAllowlistEntry.cs`: extends AuditableEntity; `int OwnerId`, `virtual ApplicationUser Owner`, `int AllowedUserId`, `virtual ApplicationUser AllowedUser`; static `Create` factory; `Deactivate(int updatedById)` instance method; XML docs
- [X] T028 Create `src/BSDCPolls.Api.Data/Entities/AuditLog.cs`: does NOT extend AuditableEntity; `long Id` (bigint PK), `string EntityType`, `int EntityId`, `Guid EntityUid`, `string Action`, `string ChangedProperties` (JSONB as string), `string OldValues` (JSONB), `string NewValues` (JSONB), `DateTime ChangedOn`, `int? ChangedById`; [Required] on string properties; `[MaxLength(100)]` on EntityType, `[MaxLength(6)]` on Action; XML docs
- [X] T029 Create `src/BSDCPolls.Api.Data/Infrastructure/ICurrentUserContext.cs` interface and `src/BSDCPolls.Api.Data/Infrastructure/CurrentUserContext.cs` implementation: `int? UserId` property read from `IHttpContextAccessor`; registered as `AddScoped`; XML docs
- [X] T030 Create `src/BSDCPolls.Api.Data/Infrastructure/AuditInterceptor.cs` implementing `SaveChangesInterceptor`: on every INSERT sets `CreatedOn`, `UpdatedOn`, `CreatedById`, `UpdatedById` from `ICurrentUserContext` (falls back to system user Id=1 when no authenticated user); on every UPDATE sets `UpdatedOn`, `UpdatedById`; writes one `AuditLog` row per changed entity to `AuditLog` DbSet; XML docs; implement `IAuditInterceptor` interface co-located in same folder
- [X] T031 Create `src/BSDCPolls.Api.Data/BsdcPollsDbContext.cs`: extends `DbContext`; `DbSet` properties for all entities (ApplicationUser, UsernameHistory, UserPrivacySettings, InviteAllowlistEntry, AuditLog); configure `UseLazyLoadingProxies()`; `OnModelCreating` with Fluent API for all relationships and unique indexes from data-model.md (ApplicationUser unique Username + SupabaseUserId; UserPrivacySettings unique UserId; InviteAllowlistEntry unique OwnerId+AllowedUserId); seed system sentinel user (Id=1); XML docs
- [X] T032 Create `src/BSDCPolls.MigrationWorker/Worker.cs`: background service that calls `dbContext.Database.MigrateAsync()` and then `StopAsync()` — runs migrations at startup and exits; register `BsdcPollsDbContext` in `Program.cs` with Aspire-injected connection string; XML docs
- [X] T033 Create initial EF Core migration: `cd src/BSDCPolls.Api.Data && dotnet ef migrations add InitialCreate --startup-project ../BSDCPolls.MigrationWorker` — creates `Migrations/` folder with the full initial schema including all foundational tables, unique indexes, sentinel user seed, and AuditLog table

### Aspire AppHost Wiring

- [X] T034 Create `src/BSDCPolls.AppHost/Program.cs`: declare PostgreSQL container (`supabase/postgres:15.1.0.117`), GoTrue auth container (`supabase/gotrue:v2.143.0`) with required env vars (`GOTRUE_DB_DATABASE_URL`, `GOTRUE_JWT_SECRET`, `GOTRUE_MAILER_AUTOCONFIRM=true`, `GOTRUE_API_EXTERNAL_URL`), SigNoz container (`signoz/signoz-otel-collector`) with OTLP ports (4317/4318), MigrationWorker resource (runs before API), API resource, BFF resource with Angular static file serving; wire service references via `WithReference`; log Aspire dashboard link to SigNoz UI

### BFF Core Infrastructure

- [X] T035 Create `src/BSDCPolls.BFF/Program.cs`: configure `AddAuthentication().AddJwtBearer` with GoTrue authority and `"authenticated"` audience from Aspire config; configure FluentValidation DI registration for all Contracts validators; configure NSwag/Swagger document generation; configure CORS for Angular dev server; register Serilog with structured logging; register OpenTelemetry with OTLP exporter pointing at Aspire-injected SigNoz endpoint; add `IHttpContextAccessor`; add `AddSignalR()`; map hubs (placeholder URLs for now); XML docs on extension methods
- [X] T036 Create `src/BSDCPolls.BFF/Middleware/TraceIdResponseMiddleware.cs`: middleware that adds `X-Trace-Id` response header from the current OpenTelemetry `Activity.Current.TraceId`; register in `Program.cs`; XML docs; implement `ITraceIdResponseMiddleware` interface
- [X] T037 Create `src/BSDCPolls.BFF/Middleware/ExceptionHandlingMiddleware.cs`: catches unhandled exceptions; logs at `Error` level via `ILogger<ExceptionHandlingMiddleware>` with full exception object; returns `ApiErrorResponse` JSON with appropriate HTTP status, user-readable message, and trace ID; NEVER includes stack trace or exception type in response; register in `Program.cs`; implement `IExceptionHandlingMiddleware` interface; XML docs

### Backend API Core Infrastructure

- [X] T038 Create `src/BSDCPolls.Api/Program.cs`: configure `AddAuthentication().AddJwtBearer` (same GoTrue authority); register `BsdcPollsDbContext` with Npgsql connection string from Aspire; register `AuditInterceptor` and `ICurrentUserContext`; configure Serilog; configure OpenTelemetry with OTLP; add FluentValidation for defense-in-depth; add `IHttpContextAccessor`; XML docs on extension methods
- [X] T039 [P] Create `src/BSDCPolls.Api/Middleware/ExceptionHandlingMiddleware.cs` (same pattern as BFF version): catches, logs, returns ApiErrorResponse with trace ID; implement `IApiExceptionHandlingMiddleware` interface; XML docs

### Angular Core Infrastructure

- [X] T040 Create `BSDCPolls.Web/src/app/core/error/global-error-handler.ts`: implements Angular `ErrorHandler`; captures unhandled errors and posts to BFF `/api/client-errors` endpoint with `{ message, stack, route, component }`; uses `HttpClient` injected via constructor; JSDoc
- [X] T041 [P] Create `BSDCPolls.Web/src/app/core/observability/trace-parent.interceptor.ts`: implements `HttpInterceptorFn`; generates a W3C `traceparent` header value (format: `00-{traceId}-{spanId}-01`) and injects it on every outgoing HTTP request; JSDoc
- [ ] T042 [P] Create `BSDCPolls.Web/src/app/core/observability/error-reporting.service.ts`: `IErrorReportingService` equivalent interface pattern; `HttpClient`-based service posting error payloads to BFF; JSDoc
- [X] T043 [P] Create `BSDCPolls.Web/src/app/core/signalr/hub-connection.factory.ts`: factory function `createHubConnection(url: string, tokenFactory: () => string): HubConnection` wrapping `HubConnectionBuilder` with `accessTokenFactory` and `withAutomaticReconnect`; JSDoc
- [X] T044 [P] Create `BSDCPolls.Web/src/app/app.config.ts`: `ApplicationConfig` with `provideRouter`, `provideHttpClient(withInterceptors([traceparentInterceptor]))`, `provideAnimations`, `{ provide: ErrorHandler, useClass: GlobalErrorHandler }`, Angular Material theme bootstrap; JSDoc
- [X] T045 [P] Create `BSDCPolls.Web/src/app/app.component.ts`: root shell component using Angular Material `<mat-sidenav-container>` with app bar (`<mat-toolbar>`), notification bell placeholder, user avatar placeholder, and router outlet; no custom CSS; JSDoc

**Checkpoint**: `dotnet build` zero warnings; `ng build` zero TypeScript errors; Aspire stack starts all containers; BFF returns 401 on unauthenticated requests

---

## Phase 3: User Story 1 — Account Registration & Login (Priority: P1) 🎯 MVP

**Goal**: Users can register with auto-generated username, log in, and access authenticated routes.

**Independent Test**: Scenario 1 in quickstart.md — register, verify generated username stored,
log in, verify JWT returned. No PII in any table.

### Contracts & Validation

- [ ] T046 Create `src/BSDCPolls.Contracts/Requests/Auth/RegisterRequest.cs`: `sealed record` with `string Password`; `[Required][MinLength(12)]` annotation; co-located `RegisterRequestValidator.cs` with FluentValidation password strength rules (uppercase, lowercase, digit, special char); XML docs on both
- [ ] T047 [P] Create `src/BSDCPolls.Contracts/Requests/Auth/LoginRequest.cs`: `sealed record` with `string Username` and `string Password`; co-located `LoginRequestValidator.cs`; XML docs
- [ ] T048 [P] Create `src/BSDCPolls.Contracts/Responses/Auth/RegisterResponse.cs`: `sealed record` with `string Username`, `Guid UserUid`; XML docs
- [ ] T049 [P] Create `src/BSDCPolls.Contracts/Responses/Auth/LoginResponse.cs`: `sealed record` with `string AccessToken`, `DateTime ExpiresAt`, `Guid UserUid`, `string Username`; XML docs

### Api.Data — Auth Repositories

- [ ] T050 Create `src/BSDCPolls.Api.Data/Repositories/IUserRepository.cs` interface and `UserRepository.cs` implementation: `GetBySupabaseIdAsync`, `GetByUsernameAsync`, `GetByUidAsync`, `UsernameExistsAsync`, `CreateAsync`, `UpdateAsync` methods; registered as `AddScoped<IUserRepository, UserRepository>` in Api DI extension; XML docs
- [ ] T051 [P] Create `src/BSDCPolls.Api.Data/Repositories/IUsernameHistoryRepository.cs` and `UsernameHistoryRepository.cs`: `AddAsync`, `IsUsernameRecentlyUsedAsync(string username, int userId, int days)` methods; XML docs

### Api.Business — Username Generation & Auth Service

- [ ] T052 Create `src/BSDCPolls.Api.Business/UsernameGeneration/IUsernameGenerator.cs` interface and `UsernameGeneratorService.cs` implementation: `GenerateAsync()` method; reads embedded adjective word list from `Resources/adjectives.txt` (~1,000 adjectives) and noun word list from `Resources/nouns.txt` (~1,000 nouns); generates `adj-adj-noun` pattern; checks profanity deny-list from `Resources/profanity.txt`; retries up to 10 times; registered as `AddSingleton<IUsernameGenerator, UsernameGeneratorService>`; XML docs
- [ ] T053 [P] Create `src/BSDCPolls.Api.Business/UsernameGeneration/Resources/adjectives.txt`: 1,000+ common English adjectives (safe, no profanity) — one word per line; embedded resource in .csproj
- [ ] T054 [P] Create `src/BSDCPolls.Api.Business/UsernameGeneration/Resources/nouns.txt`: 1,000+ common English nouns (safe, no profanity) — one word per line; embedded resource
- [ ] T055 [P] Create `src/BSDCPolls.Api.Business/UsernameGeneration/Resources/profanity.txt`: deny-list of profane/offensive English words — one word per line; embedded resource
- [ ] T056 Create `src/BSDCPolls.Api.Business/Auth/IAuthService.cs` interface and `AuthService.cs` implementation: `RegisterAsync(RegisterRequest req)` — calls IUsernameGenerator, creates ApplicationUser, calls GoTrue registration via HTTP client; `LoginAsync(LoginRequest req)` — resolves username to GoTrue email, calls GoTrue token endpoint; `ChangeUsernameAsync(int userId, int systemUserId)` — generates new username, updates ApplicationUser, creates UsernameHistory entry; registered as `AddScoped<IAuthService, AuthService>`; uses `IUserRepository`, `IUsernameHistoryRepository`, `IUsernameGenerator`; XML docs

### Api — Auth Controller

- [ ] T057 Create `src/BSDCPolls.Api/Controllers/AuthController.cs`: `[ApiController][Route("api/internal/auth")]` (internal endpoint, not BFF-facing); `POST register` → calls `IAuthService.RegisterAsync`; `POST login` → calls `IAuthService.LoginAsync`; `POST change-username` → calls `IAuthService.ChangeUsernameAsync` with `[Authorize]`; returns `ApiErrorResponse` on failures with trace ID from `Activity.Current`; logs operations at `Information` level; XML docs

### BFF.Business — Auth Forwarding

- [ ] T058 Create `src/BSDCPolls.BFF.Business/Auth/IBffAuthService.cs` interface and `BffAuthService.cs` implementation: `RegisterAsync(RegisterRequest req)` forwards to backend API `/api/internal/auth/register`; `LoginAsync(LoginRequest req)` forwards to backend API `/api/internal/auth/login`; uses `HttpClient` injected via `IHttpClientFactory` with named client pointing at the backend API internal URL from Aspire; registered as `AddScoped<IBffAuthService, BffAuthService>`; XML docs

### BFF — Auth Controller

- [ ] T059 Create `src/BSDCPolls.BFF/Controllers/AuthController.cs`: `[ApiController][Route("api/auth")]`; `POST /register` → `[AllowAnonymous]` → calls `IBffAuthService.RegisterAsync`; `POST /login` → `[AllowAnonymous]` → calls `IBffAuthService.LoginAsync`; returns `401` with `ApiErrorResponse` on auth failure; logs at `Information` level for successful registration/login, `Warning` for failures; XML docs
- [ ] T060 Create `src/BSDCPolls.BFF/Controllers/UsersController.cs`: `[ApiController][Route("api/users")][Authorize]`; `GET /me` → returns `UserProfileResponse` from current user claim; `POST /me/username/change` → calls backend API via `IBffAuthService`; rate-limiting check (max 3 per 24h) via response header inspection; XML docs

### Angular — Auth Feature

- [ ] T061 Create `BSDCPolls.Web/src/app/core/auth/auth.service.ts`: wraps `@supabase/supabase-js` `createClient`; `signUp(password: string)` calls BFF `POST /api/auth/register` then signs in via Supabase; `signIn(username: string, password: string)` calls BFF `POST /api/auth/login`; `signOut()`; `currentToken()` returns JWT string; JSDoc
- [ ] T062 [P] Create `BSDCPolls.Web/src/app/store/auth.store.ts`: NgRX Signal Store `AuthStore` with state `{ userUid, username, accessToken, isAuthenticated, isLoading, error }`; `login`, `register`, `logout` methods calling `AuthService`; persists token to sessionStorage; JSDoc
- [ ] T063 [P] Create `BSDCPolls.Web/src/app/core/auth/auth.guard.ts`: Angular functional route guard using `AuthStore.isAuthenticated`; redirects unauthenticated users to `/auth/login`; JSDoc
- [ ] T064 [P] Create `BSDCPolls.Web/src/app/core/auth/token.interceptor.ts`: `HttpInterceptorFn`; reads token from `AuthStore.accessToken`; injects `Authorization: Bearer <token>` on all requests to BFF origin; JSDoc
- [ ] T065 Create `BSDCPolls.Web/src/app/features/auth/auth.routes.ts`: lazy-loaded route configuration for `/auth` path; children: `login` → `LoginComponent`, `register` → `RegisterComponent`
- [ ] T066 [P] Create `BSDCPolls.Web/src/app/features/auth/login/login.component.ts` and `login.component.html`: Angular Material form (`<mat-card>`, `<mat-form-field>`, `<mat-input>`) with reactive form for username and password; calls `AuthStore.login()`; shows `<mat-error>` on failure; shows spinner during loading; ZERO custom CSS; JSDoc
- [ ] T067 [P] Create `BSDCPolls.Web/src/app/features/auth/register/register.component.ts` and `register.component.html`: Angular Material form for password only; calls `AuthStore.register()`; on success shows the generated username in a `<mat-dialog>` with instructions to save it; ZERO custom CSS; JSDoc
- [ ] T068 Update `BSDCPolls.Web/src/app/app.routes.ts`: add auth lazy-loaded route, add default redirect to `/feed`; protect non-auth routes with `authGuard`
- [ ] T069 Run NSwag generation: `cd BSDCPolls.Web && npm run generate-api` to generate initial `src/app/generated/api.ts` from BFF OpenAPI spec; commit generated file

**Checkpoint**: User can register (username auto-generated displayed), log in with username+password, receive JWT. No PII in database. Quickstart Scenario 1 passes.

---

## Phase 4: User Stories 2 & 3 — Live Polls (Priority: P2)

**Goal**: Creator creates and activates a live poll; pushes questions with choices in real-time;
participants see questions appear instantly and submit votes; creator sees live vote counts.

**Independent Test**: Quickstart Scenario 2 — two users, real-time question push, vote count
update within 1 second, duplicate submission rejected.

### Contracts

- [ ] T070 Create `src/BSDCPolls.Contracts/Requests/Polls/` directory with: `CreatePollRequest.cs` + `CreatePollRequestValidator.cs`, `AddPollQuestionRequest.cs` + `AddPollQuestionRequestValidator.cs`, `SubmitPollVoteRequest.cs` + `SubmitPollVoteRequestValidator.cs`, `ChangePollStatusRequest.cs` + `ChangePollStatusRequestValidator.cs` — all as `sealed record` types with FluentValidation validators; see dto-schemas.md for rules; XML docs
- [ ] T071 [P] Create `src/BSDCPolls.Contracts/Responses/Polls/` with: `PollDetailResponse.cs`, `PollFeedItem.cs`, `PollFeedResponse.cs`, `PollQuestionResponse.cs`, `PollSubmissionResponse.cs`, `PollResultsResponse.cs` — all as `sealed record` types; XML docs
- [ ] T072 [P] Create `src/BSDCPolls.Contracts/SignalR/PollHubPayloads.cs`: `PollQuestionPushedPayload`, `PollVoteCountUpdatedPayload`, `PollStatusChangedPayload` as `sealed record` types; XML docs

### Api.Data — Poll Entities & Repositories

- [ ] T073 Create `src/BSDCPolls.Api.Data/Entities/Poll.cs`: extends AuditableEntity; `string Title` `[Required][MaxLength(200)]`; `bool IsPublic`; `PollStatus Status`; `virtual ICollection<PollQuestion> Questions`; static `Create(string title, bool isPublic, int createdById)` factory; `Activate(int updatedById)`, `Close(int updatedById)` instance methods with status transition validation; XML docs
- [ ] T074 [P] Create `src/BSDCPolls.Api.Data/Entities/PollQuestion.cs`: extends AuditableEntity; `int PollId`, `virtual Poll Poll`; `string Text` `[Required][MaxLength(500)]`; `int OrderIndex`; `DateTime? PushedAt`; `virtual ICollection<PollAnswerOption> Options`; `virtual ICollection<PollSubmission> Submissions`; static `Create` factory; `Push(DateTime pushedAt, int updatedById)` instance method; XML docs
- [ ] T075 [P] Create `src/BSDCPolls.Api.Data/Entities/PollAnswerOption.cs`: extends AuditableEntity; `int PollQuestionId`, `virtual PollQuestion PollQuestion`; `string Text` `[Required][MaxLength(200)]`; `int OrderIndex`; static `Create` factory; XML docs
- [ ] T076 [P] Create `src/BSDCPolls.Api.Data/Entities/PollSubmission.cs`: extends AuditableEntity; `int PollQuestionId`, `virtual PollQuestion PollQuestion`; `int SelectedOptionId`, `virtual PollAnswerOption SelectedOption`; `int RespondentId`, `virtual ApplicationUser Respondent`; static `Create` factory; XML docs
- [ ] T077 Update `src/BSDCPolls.Api.Data/BsdcPollsDbContext.cs`: add `DbSet` for Poll, PollQuestion, PollAnswerOption, PollSubmission; add `OnModelCreating` Fluent API for all poll relationships (cascades, unique constraints, composite indexes from data-model.md)
- [ ] T078 Create EF Core migration for poll tables: `dotnet ef migrations add AddPollEntities --startup-project ../BSDCPolls.MigrationWorker`
- [ ] T079 Create `src/BSDCPolls.Api.Data/Repositories/IPollRepository.cs` and `PollRepository.cs`: `GetByUidAsync(Guid uid, int? requestingUserId)` with eager-load of Questions+Options; `GetFeedAsync(int userId, bool showPublic, int page, int pageSize)` — polls that are public OR have an invitation for userId; `GetResultsAsync(Guid pollUid, int creatorId)` with vote counts; `CreateAsync`, `UpdateAsync`; XML docs
- [ ] T080 [P] Create `src/BSDCPolls.Api.Data/Repositories/IPollSubmissionRepository.cs` and `PollSubmissionRepository.cs`: `HasUserSubmittedAsync(int questionId, int userId)`, `GetVoteCountsAsync(int questionId)`, `CreateAsync`; XML docs

### Api.Business — Poll Domain Service

- [ ] T081 Create `src/BSDCPolls.Api.Business/Polls/IPollService.cs` interface and `PollService.cs` implementation: `CreateAsync(CreatePollRequest, int creatorId)`, `GetByUidAsync(Guid uid, int requestingUserId)`, `GetFeedAsync(int userId, bool showPublic, ...)`, `AddQuestionAsync(Guid pollUid, AddPollQuestionRequest, int creatorId, bool pushImmediately)`, `PushQuestionAsync(Guid pollUid, Guid questionUid, int creatorId)`, `ChangeStatusAsync(Guid pollUid, PollStatus newStatus, int creatorId)`, `SubmitVoteAsync(Guid pollUid, SubmitPollVoteRequest, int respondentId)`, `GetResultsAsync(Guid pollUid, int requestingUserId)`; validates creator ownership; validates poll/question state before mutations; logs all domain events at `Information` level; XML docs

### Api — Poll Controller

- [ ] T082 Create `src/BSDCPolls.Api/Controllers/PollsController.cs`: `[ApiController][Route("api/internal/polls")][Authorize]`; maps all poll endpoints from api-endpoints.md (GET feed, POST create, GET detail, PATCH status, POST question, POST question push, POST submission, GET results); calls `IPollService`; returns `ApiErrorResponse` with trace ID on errors; logs at appropriate levels; XML docs

### BFF SignalR Hub — PollHub

- [ ] T083 Create `src/BSDCPolls.BFF/Hubs/IPollSessionTracker.cs` interface and `src/BSDCPolls.BFF/Hubs/PollSessionTracker.cs` implementation: thread-safe in-memory dictionary mapping `pollUid → creatorConnectionId`; `RegisterCreator`, `UnregisterCreator`, `GetCreatorConnectionId` methods; registered as `AddSingleton<IPollSessionTracker, PollSessionTracker>`; XML docs
- [ ] T084 Create `src/BSDCPolls.BFF/Hubs/PollHub.cs`: extends `Hub`; `[Authorize]`; `OnConnectedAsync` — validates `pollUid` query param, checks user authorization via backend API, adds to group `pollUid.ToString()`, if creator registers in `IPollSessionTracker`; `OnDisconnectedAsync` — removes from `IPollSessionTracker` if creator; `SubmitVote(Guid questionUid, Guid selectedOptionUid)` hub method — calls backend API `/api/internal/polls/{uid}/submissions`, on success broadcasts `VoteCountUpdated` only to creator connection ID via `IPollSessionTracker`; broadcasts `PollStatusChanged` to group when poll closes; throws typed `HubException` with string constants (`VOTE_DUPLICATE`, `VOTE_NOT_AUTHORIZED`, etc.) for client error handling; XML docs
- [ ] T085 Create `src/BSDCPolls.BFF.Business/Polls/IBffPollService.cs` and `BffPollService.cs`: forwards all poll REST requests to backend API; `PushQuestionAsync` — persists via API then calls `IHubContext<PollHub>.Clients.OthersInGroup(pollUid).SendAsync("QuestionPushed", payload)`; all other methods are thin HTTP forwarding; registered as `AddScoped<IBffPollService, BffPollService>`; XML docs
- [ ] T086 Create `src/BSDCPolls.BFF/Controllers/PollsController.cs`: `[ApiController][Route("api/polls")][Authorize]`; maps all public poll endpoints from api-endpoints.md; delegates to `IBffPollService`; returns `ApiErrorResponse` on failure; XML docs
- [ ] T087 Map `PollHub` in `src/BSDCPolls.BFF/Program.cs`: `app.MapHub<PollHub>("/hubs/poll")`

### Angular — Polls Feature

- [ ] T088 Create `BSDCPolls.Web/src/app/store/poll-session.store.ts`: NgRX Signal Store with state `{ poll, questions, activeQuestion, isCreator, isConnected, error }`; `loadPoll(uid)`, `setActiveQuestion(q)`, `addQuestion(q)`, `updateVoteCounts(payload)`, `closePoll()` methods; JSDoc
- [ ] T089 Create `BSDCPolls.Web/src/app/features/polls/poll.routes.ts`: lazy-loaded routes — `/polls/new` → `CreatePollComponent`, `/polls/:uid` → `PollSessionComponent`
- [ ] T090 [P] Create `BSDCPolls.Web/src/app/features/polls/create-poll/create-poll.component.ts` and `.html`: Angular Material form (`<mat-card>`, `<mat-form-field>`, `<mat-slide-toggle>` for public toggle); calls BFF `POST /api/polls`; on success navigates to `/polls/:uid`; ZERO custom CSS; JSDoc
- [ ] T091 Create `BSDCPolls.Web/src/app/features/polls/services/poll-hub.service.ts`: wraps `@microsoft/signalr` `HubConnection` to `/hubs/poll?pollUid=`; `connect(pollUid: string)`, `disconnect()`, `submitVote(questionUid, optionUid)`; subscribes to `QuestionPushed`, `VoteCountUpdated`, `PollStatusChanged` events and updates `PollSessionStore`; reconnect logic calls `GET /api/polls/:uid` on reconnect to re-sync state; JSDoc
- [ ] T092 Create `BSDCPolls.Web/src/app/features/polls/session/poll-session.component.ts` and `.html`: loads poll via BFF API on init; connects to `PollHubService`; shows creator view (push-question button, live bar-chart dashboard using Angular Material `<mat-progress-bar>`) OR participant view (active question popup using `<mat-dialog>` or `<mat-card>`); activate/close poll controls for creator; ZERO custom CSS; JSDoc
- [ ] T093 [P] Create `BSDCPolls.Web/src/app/features/polls/session/add-question-dialog.component.ts` and `.html`: `<mat-dialog>` with dynamic form for question text + 2–10 answer option inputs (`<mat-chip-grid>` or repeated `<mat-form-field>`); "Push Now" toggle; calls BFF `POST /api/polls/:uid/questions`; ZERO custom CSS; JSDoc
- [ ] T094 Update `BSDCPolls.Web/src/app/app.routes.ts` to include polls lazy-loaded route module

**Checkpoint**: Quickstart Scenario 2 passes end-to-end. `ng lint --max-warnings 0` passes. `dotnet csharpier --check .` passes.

---

## Phase 5: User Stories 4 & 5 — Structured Surveys (Priority: P3)

**Goal**: Creator builds conditional surveys with multiple answer types and PDF upload. Respondents
complete surveys navigating conditional branches. Results visible to creator.

**Independent Test**: Quickstart Scenario 3 — survey with conditional branch, PDF upload accepted, non-PDF rejected, response stored with correct branch navigation.

### Contracts

- [ ] T095 Create `src/BSDCPolls.Contracts/Requests/Surveys/` with: `CreateSurveyRequest.cs` + `CreateSurveyRequestValidator.cs` (includes `SurveyQuestionTreeDocumentValidator` + `SurveyQuestionNodeValidator` with recursive child validation), `UpdateSurveyQuestionsRequest.cs` + validator, `SaveSurveyResponseRequest.cs` + `SaveSurveyResponseRequestValidator.cs`, `ChangeSurveyStatusRequest.cs` + validator; all as `sealed record`; see dto-schemas.md for all FluentValidation rules including cross-field constraints; XML docs
- [ ] T096 [P] Create `src/BSDCPolls.Contracts/Responses/Surveys/` with: `SurveyDetailResponse.cs`, `SurveyFeedItem.cs`, `SurveyFeedResponse.cs`, `SurveyResponseStatusResponse.cs`, `SurveyDocumentResponse.cs`, `SurveyResultsResponse.cs`, `SurveyQuestionSummary.cs` — all `sealed record`; XML docs

### Api.Data — Survey Entities & Repositories

- [ ] T097 Create `src/BSDCPolls.Api.Data/Entities/Survey.cs`: extends AuditableEntity; `string Title` `[Required][MaxLength(200)]`; `bool IsPublic`; `SurveyStatus Status`; `SurveyQuestionTreeDocument QuestionTree` (mapped as JSONB via value converter); `virtual ICollection<SurveyResponse> Responses`; static `Create` factory; `Publish(int updatedById)`, `Close(int updatedById)`, `UpdateQuestionTree(SurveyQuestionTreeDocument tree, int updatedById)` instance methods; XML docs
- [ ] T098 [P] Create `src/BSDCPolls.Api.Data/Entities/SurveyResponse.cs`: extends AuditableEntity; `int SurveyId`, `virtual Survey Survey`; `int RespondentId`, `virtual ApplicationUser Respondent`; `SurveyAnswersDocument AnswersJson` (JSONB value converter); `bool IsComplete`; `DateTime? SubmittedAt`; `virtual ICollection<SurveyDocument> Documents`; static `Create` factory; `SaveProgress(SurveyAnswersDocument answers, int updatedById)` and `Submit(SurveyAnswersDocument answers, int updatedById)` instance methods; XML docs
- [ ] T099 [P] Create `src/BSDCPolls.Api.Data/Entities/SurveyDocument.cs`: extends AuditableEntity; `int SurveyResponseId`, `virtual SurveyResponse SurveyResponse`; `Guid QuestionUid`; `string FileName` `[Required][MaxLength(255)]`; `long FileSizeBytes`; `byte[] FileData` `[Required]`; static `Create` factory; XML docs
- [ ] T100 Create `src/BSDCPolls.Api.Data/ValueConverters/SurveyQuestionTreeConverter.cs` and `SurveyAnswersConverter.cs`: EF Core `ValueConverter<T, string>` using `System.Text.Json` to serialize/deserialize `SurveyQuestionTreeDocument` and `SurveyAnswersDocument` as JSONB; register in `OnModelCreating` with `.HasColumnType("jsonb")`; XML docs
- [ ] T101 Update `src/BSDCPolls.Api.Data/BsdcPollsDbContext.cs`: add `DbSet` for Survey, SurveyResponse, SurveyDocument; add Fluent API configuration (JSONB columns via `HasColumnType("jsonb")`, unique `(SurveyId, RespondentId)` constraint, cascade deletes)
- [ ] T102 Create EF Core migration for survey tables: `dotnet ef migrations add AddSurveyEntities --startup-project ../BSDCPolls.MigrationWorker`
- [ ] T103 Create `src/BSDCPolls.Api.Data/Repositories/ISurveyRepository.cs` and `SurveyRepository.cs`: `GetByUidAsync`, `GetFeedAsync`, `GetResultsAsync` (aggregates answers from AnswersJson JSONB deserialized in C#), `CreateAsync`, `UpdateAsync`; XML docs
- [ ] T104 [P] Create `src/BSDCPolls.Api.Data/Repositories/ISurveyResponseRepository.cs` and `SurveyResponseRepository.cs`: `GetByRespondentAsync(Guid surveyUid, int respondentId)`, `HasCompletedAsync`, `CreateAsync`, `UpdateAsync`; XML docs
- [ ] T105 [P] Create `src/BSDCPolls.Api.Data/Repositories/ISurveyDocumentRepository.cs` and `SurveyDocumentRepository.cs`: `CreateAsync(SurveyDocument doc)`, `GetByUidAsync(Guid uid)`; XML docs

### Api.Business — Survey Domain Service

- [ ] T106 Create `src/BSDCPolls.Api.Business/Surveys/ISurveyService.cs` and `SurveyService.cs`: `CreateAsync`, `GetByUidAsync`, `GetFeedAsync`, `ChangeStatusAsync`, `UpdateQuestionsAsync`, `SaveResponseAsync(Guid surveyUid, SaveSurveyResponseRequest, int respondentId)`, `UploadDocumentAsync(Guid surveyUid, Guid responseUid, Stream pdfStream, string fileName, long fileSize, int respondentId)` — validates PDF content-type and 10 MB limit, creates SurveyDocument, returns `SurveyDocumentResponse`; `GetResultsAsync`; validates access permissions; logs domain events at `Information`; XML docs

### Api — Survey Controller

- [ ] T107 Create `src/BSDCPolls.Api/Controllers/SurveysController.cs`: `[ApiController][Route("api/internal/surveys")][Authorize]`; maps all survey endpoints from api-endpoints.md; `POST .../responses/{uid}/documents` accepts `IFormFile` and validates `application/pdf` content-type and size before calling `ISurveyService.UploadDocumentAsync`; returns `ApiErrorResponse` with trace ID on errors; XML docs

### BFF Survey Service & Controller

- [ ] T108 Create `src/BSDCPolls.BFF.Business/Surveys/IBffSurveyService.cs` and `BffSurveyService.cs`: thin HTTP forwarding to backend API for all survey endpoints; for document upload, uses `HttpClient` multipart/form-data forwarding; registered as `AddScoped<IBffSurveyService, BffSurveyService>`; XML docs
- [ ] T109 Create `src/BSDCPolls.BFF/Controllers/SurveysController.cs`: `[ApiController][Route("api/surveys")][Authorize]`; maps public survey endpoints; document upload endpoint validates PDF content-type and 10 MB limit at BFF edge BEFORE forwarding to API; returns `ApiErrorResponse` on failure; XML docs

### Angular — Surveys Feature

- [ ] T110 Create `BSDCPolls.Web/src/app/store/survey.store.ts`: NgRX Signal Store with state `{ survey, currentQuestionPath, answers, responseUid, isSubmitting, error }`; `loadSurvey`, `setAnswer(questionUid, value)`, `navigateToNextQuestion(questionUid, selectedChoiceUid)` (computes next visible question based on conditional branches), `saveProgress`, `submit` methods; JSDoc
- [ ] T111 Create `BSDCPolls.Web/src/app/features/surveys/survey.routes.ts`: lazy routes — `/surveys/new` → `SurveyBuilderComponent`, `/surveys/:uid` → `SurveyRespondentComponent`, `/surveys/:uid/results` → `SurveyResultsComponent`
- [ ] T112 Create `BSDCPolls.Web/src/app/features/surveys/builder/survey-builder.component.ts` and `.html`: Angular Material stepper (`<mat-stepper>`) for survey title/visibility → question tree editor; question type selector (`<mat-select>`); conditional branch configuration (`<mat-expansion-panel>`); `<mat-chip-grid>` for multiple choice options; calls BFF `POST /api/surveys`; ZERO custom CSS; JSDoc
- [ ] T113 [P] Create `BSDCPolls.Web/src/app/features/surveys/respondent/survey-respondent.component.ts` and `.html`: renders current visible question based on `SurveyStore.currentQuestionPath`; conditionally shows/hides questions based on prior answers; renders answer type appropriately (`<mat-radio-group>` for MC, `<mat-input>` for text, `<input type="file" accept=".pdf">` for PDF); calls PDF upload endpoint then saves reference; `<mat-button>` to advance/submit; ZERO custom CSS; JSDoc
- [ ] T114 [P] Create `BSDCPolls.Web/src/app/features/surveys/results/survey-results.component.ts` and `.html`: displays aggregated results per question; `<mat-table>` for MC tallies; text answers list; document count badge; ZERO custom CSS; JSDoc
- [ ] T115 Update `BSDCPolls.Web/src/app/app.routes.ts` to include surveys lazy-loaded route

**Checkpoint**: Quickstart Scenario 3 passes. Non-PDF upload rejected. Conditional branch renders correctly. `ng lint --max-warnings 0` passes. `dotnet csharpier --check .` passes.

---

## Phase 6: User Stories 6 & 7 — Home Feed & Notifications (Priority: P4)

**Goal**: Home page with three tabs showing public+invited polls/surveys and results. Notification
bell with real-time invitation delivery. Invite controls on polls and surveys.

**Independent Test**: Quickstart Scenarios 4 & 5 — feed respects public/invite-only; notifications arrive within 5 seconds; invite permission blocking works.

### Contracts

- [ ] T116 Create `src/BSDCPolls.Contracts/Requests/Invitations/CreateInvitationRequest.cs` + `CreateInvitationRequestValidator.cs`: `TargetUsername` string; `[Required][MaxLength(60)]`; XML docs
- [ ] T117 [P] Create `src/BSDCPolls.Contracts/Responses/Notifications/`: `InvitationResponse.cs`, `NotificationListResponse.cs`, `NotificationItem.cs`, `NotificationReadResponse.cs`, `InvitationReceivedPayload.cs` (SignalR payload) — all `sealed record`; XML docs

### Api.Data — Invitation & Notification Entities

- [ ] T118 Create `src/BSDCPolls.Api.Data/Entities/Invitation.cs`: extends AuditableEntity; `int InviterId`, `virtual ApplicationUser Inviter`; `int InviteeId`, `virtual ApplicationUser Invitee`; `int? PollId`, `virtual Poll? Poll`; `int? SurveyId`, `virtual Survey? Survey`; `InvitationStatus Status`; static `Create(int inviterId, int inviteeId, int? pollId, int? surveyId)` factory with validation that exactly one of pollId/surveyId is non-null; `MarkViewed(int updatedById)` instance method; XML docs
- [ ] T119 [P] Create `src/BSDCPolls.Api.Data/Entities/Notification.cs`: extends AuditableEntity; `int RecipientId`, `virtual ApplicationUser Recipient`; `int InvitationId`, `virtual Invitation Invitation`; `bool IsRead`; `DateTime? ReadAt`; static `Create` factory; `MarkRead(int updatedById)` instance method; XML docs
- [ ] T120 Update `src/BSDCPolls.Api.Data/BsdcPollsDbContext.cs`: add `DbSet` for Invitation and Notification; Fluent API config (unique index on notifications for recipient+invitation pair; composite index on invitations for inviteeId+status; nullable FK for PollId/SurveyId)
- [ ] T121 Create EF Core migration for invitation/notification tables: `dotnet ef migrations add AddInvitationNotificationEntities --startup-project ../BSDCPolls.MigrationWorker`
- [ ] T122 Create `src/BSDCPolls.Api.Data/Repositories/IInvitationRepository.cs` and `InvitationRepository.cs`: `CreateAsync`, `GetByUidAsync`, `IsDuplicateAsync(int inviteeId, int? pollId, int? surveyId)`, `GetForPollAsync(Guid pollUid, int inviteeId)`, `GetForSurveyAsync(Guid surveyUid, int inviteeId)`; XML docs
- [ ] T123 [P] Create `src/BSDCPolls.Api.Data/Repositories/INotificationRepository.cs` and `NotificationRepository.cs`: `CreateAsync`, `GetByRecipientAsync(int recipientId, bool unreadOnly, int page, int pageSize)`, `GetUnreadCountAsync(int recipientId)`, `MarkReadAsync(Guid notificationUid, int recipientId)`, `MarkAllReadAsync(int recipientId)`; XML docs

### Api.Business — Invitation & Notification Services

- [ ] T124 Create `src/BSDCPolls.Api.Business/Invitations/IInvitationService.cs` and `InvitationService.cs`: `CreatePollInvitationAsync(Guid pollUid, CreateInvitationRequest, int inviterId)` — validates target user exists, checks `UserPrivacySettings.InvitePermission`, checks allowlist if needed, creates `Invitation` + `Notification`, calls BFF internal notification push endpoint via `IHttpClientFactory`; `CreateSurveyInvitationAsync` same pattern for surveys; validates: target user exists, not already invited, poll/survey exists and caller is creator; logs at `Information`; XML docs
- [ ] T125 [P] Create `src/BSDCPolls.Api.Business/Notifications/INotificationService.cs` and `NotificationService.cs`: `GetNotificationsAsync(int userId, bool unreadOnly, int page, int pageSize)`, `MarkReadAsync(Guid notificationUid, int userId)`, `MarkAllReadAsync(int userId)`, `GetUnreadCountAsync(int userId)`; XML docs

### Api — Invitation & Notification Controllers

- [ ] T126 Create `src/BSDCPolls.Api/Controllers/InvitationsController.cs`: `[ApiController][Route("api/internal")][Authorize]`; `POST /polls/{uid}/invitations` → `IInvitationService.CreatePollInvitationAsync`; `POST /surveys/{uid}/invitations` → `IInvitationService.CreateSurveyInvitationAsync`; returns `ApiErrorResponse` on failure; XML docs
- [ ] T127 [P] Create `src/BSDCPolls.Api/Controllers/NotificationsController.cs`: `GET /api/internal/notifications`, `PATCH .../read`, `PATCH .../read-all`; delegates to `INotificationService`; XML docs

### BFF — NotificationHub & Internal Push

- [ ] T128 Create `src/BSDCPolls.BFF/Hubs/NotificationHub.cs`: extends `Hub`; `[Authorize]`; `OnConnectedAsync` — adds connection to personal group `userId.ToString()`; `OnDisconnectedAsync` — nothing extra needed (SignalR handles group cleanup); no client-invocable methods; XML docs; implement `INotificationHub` interface (empty interface for DI type safety with `IHubContext<NotificationHub>`)
- [ ] T129 Create `src/BSDCPolls.BFF/Controllers/Internal/NotificationPushController.cs`: `[ApiController][Route("internal/notifications")]` (not authenticated by JWT — secured by internal network only; add `[RequireHost("*")]` or IP filter); `POST /push` accepts `{ targetUserId: int, payload: InvitationReceivedPayload }` and calls `IHubContext<NotificationHub>.Clients.Group(targetUserId.ToString()).SendAsync("InvitationReceived", payload)`; XML docs
- [ ] T130 Map `NotificationHub` in `src/BSDCPolls.BFF/Program.cs`: `app.MapHub<NotificationHub>("/hubs/notifications")`
- [ ] T131 Create `src/BSDCPolls.BFF.Business/Invitations/IBffInvitationService.cs` and `BffInvitationService.cs`: forwards `POST /api/polls/:uid/invitations` and `POST /api/surveys/:uid/invitations` to backend API; XML docs
- [ ] T132 [P] Create `src/BSDCPolls.BFF.Business/Notifications/IBffNotificationService.cs` and `BffNotificationService.cs`: forwards GET/PATCH notification endpoints to backend API; XML docs
- [ ] T133 Create `src/BSDCPolls.BFF/Controllers/InvitationsController.cs` and `NotificationsController.cs`: public-facing BFF endpoints from api-endpoints.md; delegate to `IBffInvitationService` and `IBffNotificationService`; XML docs

### Angular — Feed & Notifications Features

- [ ] T134 Create `BSDCPolls.Web/src/app/store/feed.store.ts`: NgRX Signal Store with state `{ polls, surveys, results, activeTab, isLoading, error }`; `loadPolls`, `loadSurveys`, `loadResults` methods calling BFF; JSDoc
- [ ] T135 [P] Create `BSDCPolls.Web/src/app/store/notifications.store.ts`: NgRX Signal Store with state `{ items, unreadCount, isLoading }`; `loadNotifications`, `markRead(uid)`, `markAllRead`, `addNotification(item)` (called from SignalR event); JSDoc
- [ ] T136 Create `BSDCPolls.Web/src/app/features/feed/feed.routes.ts`: lazy route `/feed` → `FeedComponent`
- [ ] T137 Create `BSDCPolls.Web/src/app/features/feed/feed.component.ts` and `.html`: Angular Material `<mat-tab-group>` with three tabs: "Polls" (list of `PollFeedItem` cards), "Surveys" (list of `SurveyFeedItem` cards), "Results" (list of owned polls/surveys with result links); each card uses `<mat-card>` with title, creator, status badge; click navigates to poll/survey URL; ZERO custom CSS; JSDoc
- [ ] T138 Create `BSDCPolls.Web/src/app/features/notifications/notification-bell.component.ts` and `.html`: `<button mat-icon-button>` with `<mat-icon>notifications</mat-icon>` and `<mat-badge>` showing unread count from `NotificationsStore`; opens notification panel in a `<mat-menu>`; each item shows inviter username and poll/survey title with a link; "Mark all read" button; JSDoc
- [ ] T139 Create `BSDCPolls.Web/src/app/features/notifications/notification-hub.service.ts`: wraps `@microsoft/signalr` connection to `/hubs/notifications`; subscribes to `InvitationReceived` event and calls `NotificationsStore.addNotification`; reconnects and calls `GET /api/notifications?unreadOnly=true` to re-sync; JSDoc
- [ ] T140 Update `BSDCPolls.Web/src/app/app.component.ts`: integrate `NotificationBellComponent` in the `<mat-toolbar>`; initialize `NotificationHubService` connection on app init (via `APP_INITIALIZER` or `ngOnInit`); update `BSDCPolls.Web/src/app/app.routes.ts` to include feed and notifications lazy routes
- [ ] T141 Add invite controls to poll and survey components: add `<mat-icon-button>` "Invite" button in `poll-session.component` and survey builder/results; invite dialog component (`InviteUserDialogComponent`) with `<mat-form-field>` for username input; calls `POST /api/polls/:uid/invitations` or surveys equivalent; shows error if user not found or not accepting invites; ZERO custom CSS; JSDoc

**Checkpoint**: Quickstart Scenarios 4 & 5 pass. Notification delivered within 5 seconds. Feed respects public/invite-only settings. `ng lint --max-warnings 0` passes.

---

## Phase 7: User Story 8 — User Profile & Privacy Controls (Priority: P5)

**Goal**: Profile view with username display, username change, and full privacy settings
(public content toggle, invite permission, allowlist management).

**Independent Test**: Quickstart Scenario 5 (privacy section) — all privacy toggles work, allowlist entry validates username existence, invite blocking enforced end-to-end.

### Contracts

- [ ] T142 Create `src/BSDCPolls.Contracts/Requests/Privacy/UpdatePrivacySettingsRequest.cs` + `UpdatePrivacySettingsRequestValidator.cs` and `AddAllowlistEntryRequest.cs` + `AddAllowlistEntryRequestValidator.cs`; `src/BSDCPolls.Contracts/Responses/Users/PrivacySettingsResponse.cs`, `AllowlistEntryResponse.cs`, `UsernameChangeResponse.cs`, `UserProfileResponse.cs` — all `sealed record`; XML docs

### Api.Business — Privacy Service

- [ ] T143 Create `src/BSDCPolls.Api.Business/Privacy/IPrivacyService.cs` and `PrivacyService.cs`: `GetSettingsAsync(int userId)`, `UpdateSettingsAsync(int userId, UpdatePrivacySettingsRequest)`, `AddAllowlistEntryAsync(int ownerId, string targetUsername)` — validates target username exists before adding; `RemoveAllowlistEntryAsync(int ownerId, Guid allowedUserUid)`; uses `IUserRepository` for username→user lookup; registered as `AddScoped<IPrivacyService, PrivacyService>`; XML docs

### Api.Data — Privacy Repositories Update

- [ ] T144 Create `src/BSDCPolls.Api.Data/Repositories/IPrivacySettingsRepository.cs` and `PrivacySettingsRepository.cs`: `GetByUserIdAsync`, `UpdateAsync`, `GetAllowlistAsync`, `AddAllowlistEntryAsync`, `RemoveAllowlistEntryAsync`; XML docs
- [ ] T145 Update `src/BSDCPolls.Api.Data/BsdcPollsDbContext.cs`: verify `UserPrivacySettings` and `InviteAllowlistEntry` are properly configured with unique indexes and FK relationships if not already done in Phase 2

### Api — Privacy & User Controllers

- [ ] T146 Create `src/BSDCPolls.Api/Controllers/PrivacyController.cs` and update `UsersController.cs`: expose privacy endpoints per api-endpoints.md (`GET/PUT /api/internal/users/me/privacy`, `POST/DELETE /api/internal/users/me/privacy/allowlist`); delegates to `IPrivacyService`; XML docs

### BFF — Privacy Forwarding

- [ ] T147 Create `src/BSDCPolls.BFF.Business/Privacy/IBffPrivacyService.cs` and `BffPrivacyService.cs`: forwards all privacy and user profile endpoints to backend API; XML docs
- [ ] T148 Update `src/BSDCPolls.BFF/Controllers/UsersController.cs`: add all privacy endpoints from api-endpoints.md delegating to `IBffPrivacyService`; XML docs

### Angular — Profile Feature

- [ ] T149 Create `BSDCPolls.Web/src/app/features/profile/profile.routes.ts`: lazy route `/profile` → `ProfileComponent`
- [ ] T150 Create `BSDCPolls.Web/src/app/features/profile/profile.component.ts` and `.html`: Angular Material `<mat-card>` sections: (1) username display + "Change Username" button that calls `POST /api/users/me/username/change` and shows new username in `<mat-dialog>`; (2) `<mat-slide-toggle>` for "Show Public Content"; (3) `<mat-radio-group>` for invite permission (Everyone/Nobody/Selected Users); (4) allowlist management — `<mat-form-field>` to enter username + validation call to BFF, `<mat-chip-grid>` displaying current allowlist entries with remove button; all changes persist immediately via API calls; ZERO custom CSS; JSDoc
- [ ] T151 Update `BSDCPolls.Web/src/app/app.component.ts`: clicking user avatar/initials in `<mat-toolbar>` navigates to `/profile`; update `app.routes.ts` to include profile lazy route

**Checkpoint**: Quickstart Scenario 5 (profile section) passes. All privacy controls work. `ng lint --max-warnings 0` passes.

---

## Phase 8: Polish & Cross-Cutting Concerns

**Purpose**: Observability verification, NSwag regeneration, code quality gates, and final end-to-end validation.

- [ ] T152 Add BFF client error logging endpoint: create `src/BSDCPolls.BFF/Controllers/ClientErrorsController.cs` `[AllowAnonymous]` `POST /api/client-errors` that receives `{ message, stack, route }` and logs at `Error` level via `ILogger` with structured fields; this is the endpoint used by Angular's `GlobalErrorHandler`; XML docs
- [ ] T153 [P] Add XML documentation comments to all remaining public/internal .NET APIs that were missed — run `dotnet build 2>&1 | grep "CS1591"` to find missing docs and add them; run `dotnet build` to confirm zero warnings
- [ ] T154 [P] Add JSDoc comments to all exported Angular services, components, and store features that are missing them; run `ng lint --max-warnings 0` to confirm zero linting warnings
- [ ] T155 [P] Run CSharpier formatting: `dotnet csharpier .` from repo root to format all C# files; confirm `dotnet csharpier --check .` exits with code 0
- [ ] T156 [P] Run Prettier formatting: `cd BSDCPolls.Web && npx prettier --write .`; confirm `npx prettier --check .` exits with code 0
- [ ] T157 Regenerate NSwag TypeScript: `cd BSDCPolls.Web && npm run generate-api`; commit updated `src/app/generated/api.ts`; verify Angular compiles with zero TypeScript errors after regeneration
- [ ] T158 [P] Verify bundle discipline: run `ng build --stats-json` and inspect `stats.json` for unexpected bundle size regressions; confirm all 6 feature modules appear as lazy-loaded chunks
- [ ] T159 [P] Verify no test files exist anywhere: `find . -name "*.spec.ts" -o -name "*Tests.csproj" -o -name "*Test.csproj"` must return empty
- [ ] T160 [P] Verify Principle XV compliance: `grep -r "xunit\|jest\|jasmine\|karma\|NUnit\|MSTest\|@testing-library\|playwright" --include="*.csproj" --include="*.json" .` must return no matches
- [ ] T161 Run full Aspire stack and execute all 8 Quickstart scenarios from `quickstart.md` — document any failures for resolution
- [ ] T162 [P] Update `specs/001-polls-surveys-platform/checklists/requirements.md`: verify all 12 checklist items remain passing post-implementation; add implementation completion note with date

---

## Dependencies & Execution Order

### Phase Dependencies

- **Phase 1 (Setup)**: No dependencies — start immediately
- **Phase 2 (Foundational)**: Depends on Phase 1 completion — **BLOCKS all user story phases**
- **Phase 3 (US1 — Auth)**: Depends on Phase 2 completion
- **Phase 4 (US2+3 — Polls)**: Depends on Phase 2 + Phase 3 (ApplicationUser must exist)
- **Phase 5 (US4+5 — Surveys)**: Depends on Phase 2 + Phase 3; can start in parallel with Phase 4
- **Phase 6 (US6+7 — Feed+Notifications)**: Depends on Phase 3, 4, 5 (needs poll+survey entities for feed)
- **Phase 7 (US8 — Profile)**: Depends on Phase 3 (UserPrivacySettings created on registration); can start in parallel with Phase 4+5
- **Phase 8 (Polish)**: Depends on all story phases complete

### Within Each Phase

- Tasks marked `[P]` touch different files and can run concurrently with other `[P]` tasks in the same phase
- Entity → Repository → Service → Controller order is mandatory within each layer
- Angular Store → Service → Component → Route order is mandatory within each story

### Parallel Opportunities

**Phase 2 foundational tasks that can run in parallel**:
- T020–T022 (contracts layer) || T023–T028 (entities) || T034 (AppHost)

**Phase 3 tasks that can run in parallel**:
- T046–T049 (contracts) can all run in parallel
- T050–T051 (repositories) can run in parallel
- T052–T055 (word list resources) can all run in parallel

**Phase 4 tasks that can run in parallel**:
- T070–T072 (contracts) can all run in parallel
- T073–T076 (entities) can all run in parallel

---

## Implementation Strategy

### MVP First (User Story 1 Only)

1. Complete Phase 1: Setup
2. Complete Phase 2: Foundational (**critical — blocks everything**)
3. Complete Phase 3: US1 Registration & Login
4. **STOP and VALIDATE**: No PII in DB; JWT returned; Angular auth flow works
5. Run Quickstart Scenario 1

### Incremental Delivery

1. Phase 1 + 2 → Foundation ready
2. Phase 3 → Auth MVP (can register and log in)
3. Phase 4 → Live Polls (primary feature complete)
4. Phase 5 → Surveys added (second major pillar)
5. Phase 6 → Social layer (feed, notifications, invitations)
6. Phase 7 → Privacy controls
7. Phase 8 → Production quality gates

---

## Notes

- `[P]` = different files, no dependencies on incomplete tasks in same phase
- `[Story]` maps to user story for traceability to spec.md
- NO test tasks anywhere — Principle XV is absolute
- After every entity task, the EF Core DbContext must be updated and a migration created
- After every BFF contract change, run `npm run generate-api` to keep TypeScript types in sync
- `dotnet csharpier --check .` and `ng lint --max-warnings 0` must pass at every checkpoint
