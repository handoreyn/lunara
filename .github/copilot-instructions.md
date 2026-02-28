# GitHub Copilot Instructions for Lunara

These instructions guide AI code generation in this repository. Follow them strictly.

---

## Architecture

Lunara is a **modular monolith** built on:

- **.NET 10** — target framework for all projects.
- **Clean Architecture** — strict dependency direction: Domain ← Application ← Infrastructure.
- **Option B modular architecture** — each module owns its own Domain, Application, and Infrastructure projects.

### Project layout

```
src/
  Platform/
    Lunara.Api                   # ASP.NET Core minimal API entrypoint
    Lunara.Worker                # Background worker entrypoint
    Lunara.SharedKernel          # Shared domain primitives (no dependencies on Application/Infrastructure)
    Lunara.BuildingBlocks        # Cross-cutting utilities (guards, extensions)
    Lunara.Infrastructure.Host   # Composition root — wires all module DI registrations
  Modules/
    {Module}/
      Lunara.{Module}.Domain         # Entities, ValueObjects, DomainEvents, Repository interfaces
      Lunara.{Module}.Application    # UseCases, Ports (interfaces), DTOs
      Lunara.{Module}.Infrastructure # Persistence, Messaging, DI registrations
tests/
  Lunara.UnitTests     # xUnit unit tests
  Lunara.ArchTests     # xUnit + NetArchTest architecture enforcement tests
```

---

## Dependency Rules (enforced by arch tests)

| From | May depend on | Must NOT depend on |
|------|--------------|-------------------|
| `*.Domain` | `SharedKernel` | `*.Application`, `*.Infrastructure` |
| `*.Application` | `*.Domain`, `SharedKernel`, `BuildingBlocks` | `*.Infrastructure` |
| `*.Infrastructure` | `*.Application`, `*.Domain` | — |
| `Lunara.Api` | `Infrastructure.Host`, `SharedKernel` | module `*.Infrastructure` directly |
| `Lunara.Worker` | `Infrastructure.Host`, `SharedKernel` | module `*.Infrastructure` directly |

---

## Forbidden Libraries

**Never add** the following packages or assemblies to any project in this repository:

| Library | Reason |
|---------|--------|
| `AutoMapper` | Implicit mapping hides intent; use explicit mapping methods |
| `MediatR` | Implicit dispatch hides call graphs; use explicit use-case interfaces |
| `MassTransit` | Not adopted; use the custom messaging abstraction in `BuildingBlocks` |

Violations are detected at build time by `Lunara.ArchTests`.

---

## Code Style

- **Namespace style**: file-scoped (`namespace Foo.Bar;`) — enforced by `.editorconfig`.
- **Nullable**: `enable` — all nullable reference types must be annotated or suppressed with justification.
- **Implicit usings**: `enable` — only add explicit `using` when required.
- **No `var`**: use explicit types unless the type is _immediately obvious_ from the right-hand side (e.g. `new` expressions).
- **Naming**: PascalCase for types, methods, properties; camelCase for fields and locals; `_camelCase` for private fields.
- **Test naming**: `MethodOrScenario_Condition_ExpectedResult` (underscores allowed in test names).
- All public and internal API members must have XML doc comments.

---

## Commits

Use **Conventional Commits** — see [docs/commit-standard.md](docs/commit-standard.md).

**Every logical change is a separate atomic commit.** Do not bundle unrelated changes.

---

## Branching

See [docs/git-flow.md](docs/git-flow.md).

- Feature work targets `develop`.
- Only `develop` merges into `master` (via PR).
- Hotfixes branch from `master` and merge into both `master` and `develop`.

---

## Testing

- New features must include unit tests in `Lunara.UnitTests`.
- New dependency rules must be added to `Lunara.ArchTests.CleanArchitectureTests`.
- `dotnet test` must pass before any commit.

---

## What NOT to generate

- Do not generate `AutoMapper` profiles, `IMapper` injections, or mapping configuration.
- Do not generate `IMediator` send/publish calls or `IRequest`/`INotification` handlers.
- Do not generate `IBus`, `IPublishEndpoint`, or any MassTransit consumer/saga.
- Do not add `[ApiController]` base classes where minimal API endpoints suffice.
- Do not introduce framework-level abstractions not already present in the codebase.
