var builder = DistributedApplication.CreateBuilder(args);

// ── PostgreSQL (Supabase) ─────────────────────────────────────────────────────
// Pinned dev password so GoTrue (a non-.NET container) can share the same creds.
// Set via: dotnet user-secrets set "Parameters:postgres-password" "<pass>" --project src/BSDCPolls.AppHost
var pgPassword = builder.AddParameter("postgres-password", secret: true);

var postgres = builder
    .AddPostgres("bsdcpolls-postgres", password: pgPassword)
    .WithEnvironment("POSTGRES_DB", "bsdcpolls")

    // Mounts postgres-init/ so PostgreSQL runs 01-create-auth-schema.sql on first startup,
    // pre-creating the auth schema that GoTrue requires before it can run its own migrations.
    .WithBindMount("postgres-init", "/docker-entrypoint-initdb.d")
    .WithPgAdmin();

var db = postgres.AddDatabase("BsdcPollsDb", "bsdcpolls");

// GoTrue DB URL constructed from the same Aspire parameter — password resolved at runtime.
var goTrueDbUrl = ReferenceExpression.Create(
    $"postgres://postgres:{pgPassword.Resource}@bsdcpolls-postgres:5432/bsdcpolls");

// ── Supabase GoTrue (auth) ────────────────────────────────────────────────────
// Self-hosted GoTrue. MAILER_AUTOCONFIRM=true disables the email confirmation
// flow; users register with synthetic internal emails only.
var goTrue = builder
    .AddContainer("gotrue", "supabase/gotrue", "v2.151.0")
    .WithEnvironment("GOTRUE_DB_DRIVER", "postgres")
    .WithEnvironment("GOTRUE_DB_DATABASE_URL", goTrueDbUrl)
    .WithEnvironment("GOTRUE_SITE_URL", "http://localhost:4200")
    .WithEnvironment("GOTRUE_JWT_SECRET", "super-secret-jwt-token-for-dev-only")
    .WithEnvironment("GOTRUE_JWT_EXP", "3600")
    .WithEnvironment("GOTRUE_API_HOST", "0.0.0.0")
    .WithEnvironment("PORT", "9999")
    .WithEnvironment("MAILER_AUTOCONFIRM", "true")
    .WithEnvironment("GOTRUE_LOG_LEVEL", "debug")
    .WithEnvironment("API_EXTERNAL_URL", "http://localhost:9999")
    .WithHttpEndpoint(targetPort: 9999, name: "gotrue")
    .WaitFor(postgres);

// ── SigNoz observability ──────────────────────────────────────────────────────
// SigNoz all-in-one container. OTLP gRPC on 4317, OTLP HTTP on 4318.
var sigNoz = builder
    .AddContainer("signoz", "signoz/signoz", "latest")
    .WithVolume("signoz-data", "/var/lib/signoz")
    .WithHttpEndpoint(targetPort: 3301, name: "signoz-ui")
    .WithEndpoint(targetPort: 4317, name: "otlp-grpc", scheme: "http")
    .WithEndpoint(targetPort: 4318, name: "otlp-http", scheme: "http");

var sigNozOtlpEndpoint = sigNoz.GetEndpoint("otlp-grpc");

// ── Migration Worker ──────────────────────────────────────────────────────────
var migrationWorker = builder
    .AddProject<Projects.BSDCPolls_MigrationWorker>("migration-worker")
    .WithReference(db)
    .WaitFor(postgres);

// ── Internal API ─────────────────────────────────────────────────────────────
var api = builder
    .AddProject<Projects.BSDCPolls_Api>("bsdcpolls-api")
    .WithReference(db)
    .WithEnvironment("GoTrue__Url", goTrue.GetEndpoint("gotrue"))
    .WithEnvironment("Otlp__Endpoint", sigNozOtlpEndpoint)
    .WaitForCompletion(migrationWorker);

// ── BFF (internet-facing) ─────────────────────────────────────────────────────
builder
    .AddProject<Projects.BSDCPolls_BFF>("bsdcpolls-bff")
    .WithReference(api)
    .WithEnvironment("InternalApi__Url", api.GetEndpoint("http"))
    .WithEnvironment("Otlp__Endpoint", sigNozOtlpEndpoint)
    .WaitForCompletion(migrationWorker);

builder.Build().Run();
