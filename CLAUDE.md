<!-- SPECKIT START -->
For additional context about technologies to be used, project structure,
shell commands, and other important information, read the current plan
at `specs/001-polls-surveys-platform/plan.md`.

Key artifacts for the active feature:
- Spec: `specs/001-polls-surveys-platform/spec.md`
- Research decisions: `specs/001-polls-surveys-platform/research.md`
- Data model: `specs/001-polls-surveys-platform/data-model.md`
- REST contracts: `specs/001-polls-surveys-platform/contracts/api-endpoints.md`
- SignalR contracts: `specs/001-polls-surveys-platform/contracts/signalr-hubs.md`
- DTO validation: `specs/001-polls-surveys-platform/contracts/dto-schemas.md`
- Validation guide: `specs/001-polls-surveys-platform/quickstart.md`
- Constitution: `.specify/memory/constitution.md`
<!-- SPECKIT END -->

## AI Coding Skills — Auto-Activation

Before writing **any Angular code** (TypeScript, components, services, stores, templates),
automatically load the Angular skill:
- Skill: `angular` (installed from `https://github.com/angular/skills`)

Before writing **any .NET / C# code** (BFF, API, Contracts, Data layer, Aspire, migrations),
automatically load the .NET skill:
- Skill: `dotnet` (installed from `https://github.com/dotnet/skills`)

These skills activate at the start of the relevant implementation task — no manual prompt
needed. The skills provide language-specific implementation guidance; the project
constitution (`.specify/memory/constitution.md`) provides architectural governance.
Both apply simultaneously.
