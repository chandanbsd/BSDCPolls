var builder = DistributedApplication.CreateBuilder(args);

// ── PostgreSQL (Supabase) ─────────────────────────────────────────────────────
var postgres = builder
    .AddPostgres("bsdcpolls-postgres")
    .WithEnvironment("POSTGRES_DB", "bsdcpolls")
    .WithPgAdmin();

var db = postgres.AddDatabase("BsdcPollsDb", "bsdcpolls");

// ── Supabase GoTrue (auth) ────────────────────────────────────────────────────
// Self-hosted GoTrue. MAILER_AUTOCONFIRM=true disables the email confirmation
// flow; users register with synthetic internal emails only.
var goTrue = builder
    .AddContainer("gotrue", "supabase/gotrue", "v2.173.0")
    .WithEnvironment("GOTRUE_DB_DRIVER", "postgres")
    .WithEnvironment("GOTRUE_DB_DATABASE_URL", "postgres://postgres:postgres@bsdcpolls-postgres:5432/bsdcpolls")
    .WithEnvironment("GOTRUE_SITE_URL", "http://localhost:4200")
    .WithEnvironment("GOTRUE_JWT_SECRET", "super-secret-jwt-token-for-dev-only")
    .WithEnvironment("GOTRUE_JWT_EXP", "3600")
    .WithEnvironment("GOTRUE_API_HOST", "0.0.0.0")
    .WithEnvironment("PORT", "9999")
    .WithEnvironment("MAILER_AUTOCONFIRM", "true")
    .WithEnvironment("GOTRUE_LOG_LEVEL", "debug")
    .WithEnvironment("API_EXTERNAL_URL", "http://localhost:9999")
    .WithHttpEndpoint(targetPort: 9999, name: "gotrue");

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
    .WithEnvironment("Otlp__Endpoint", sigNozOtlpEndpoint)
    .WaitForCompletion(migrationWorker);

// ── BFF (internet-facing) ─────────────────────────────────────────────────────
builder
    .AddProject<Projects.BSDCPolls_BFF>("bsdcpolls-bff")
    .WithReference(api)
    .WithEnvironment("GoTrue__Url", goTrue.GetEndpoint("gotrue"))
    .WithEnvironment("InternalApi__Url", api.GetEndpoint("http"))
    .WithEnvironment("Otlp__Endpoint", sigNozOtlpEndpoint)
    .WaitForCompletion(migrationWorker);

builder.Build().Run();
