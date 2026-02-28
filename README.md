# Lunara

A modular monolith built with .NET 10, Clean Architecture, and modular architecture (Option B — each module has its own Domain/Application/Infrastructure).

## Architecture

- **Platform**: `Lunara.Api`, `Lunara.Worker`, `Lunara.SharedKernel`, `Lunara.BuildingBlocks`, `Lunara.Infrastructure.Host`
- **Modules**: Identity, Profiles, Discovery, Social, Messaging, Media, Notifications, Moderation — each with `.Domain`, `.Application`, `.Infrastructure` projects

## Getting Started

```bash
dotnet restore
dotnet build
dotnet test
```

See [docs/local-development.md](docs/local-development.md) for the full local setup guide (infrastructure, migrations, running the API and Worker).

## Branching Strategy

See [docs/git-flow.md](docs/git-flow.md).

## Commit Convention

See [docs/commit-standard.md](docs/commit-standard.md).
