<!-- SPECKIT START -->
For additional context about technologies to be used, project structure,
shell commands, and other important information, read the current plan
at `specs/002-material-responsive-ui/plan.md`.

Key artifacts for the active feature:
- Spec: `specs/002-material-responsive-ui/spec.md`
- Research decisions: `specs/002-material-responsive-ui/research.md`
- Data model: `specs/002-material-responsive-ui/data-model.md`
- Navigation layout contract: `specs/002-material-responsive-ui/contracts/navigation-layout.md`
- Design tokens contract: `specs/002-material-responsive-ui/contracts/design-tokens.md`
- Accessibility contract: `specs/002-material-responsive-ui/contracts/accessibility.md`
- Validation guide: `specs/002-material-responsive-ui/quickstart.md`
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
