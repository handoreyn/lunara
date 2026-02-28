# Local Development Guide

## Prerequisites

| Tool | Version |
|------|---------|
| .NET SDK | 10.x |
| Docker Desktop (macOS) | 4.x |
| EF Core CLI | `dotnet tool install --global dotnet-ef` |

---

## 1. Start infrastructure

```bash
cp infra/.env.example infra/.env   # first time only
docker compose -f infra/docker-compose.yml up -d
```

Services started:

| Service | URL / address |
|---------|--------------|
| PostgreSQL | `localhost:5432` |
| Kafka | `localhost:9092` |
| Kafka UI | http://localhost:8080 |

Stop everything (data preserved):
```bash
docker compose -f infra/docker-compose.yml down
```

---

## 2. Configure appsettings

Both `Lunara.Api` and `Lunara.Worker` read configuration from `appsettings.Development.json`.
Ensure the files contain:

```json
{
  "Database": {
    "ConnectionString": "Host=localhost;Port=5432;Database=lunara;Username=lunara;Password=lunara"
  },
  "Kafka": {
    "BootstrapServers": "localhost:9092",
    "TopicPrefix": "lunara."
  }
}
```

These match the defaults in `infra/.env.example`. No changes needed if you used the default `.env`.

---

## 3. Run EF Core migrations

`Lunara.Infrastructure.Host` owns `LunaraDbContext`. `Lunara.Api` is the startup project
(it hosts `IConfiguration` and the DI container).

```bash
# Create a new migration (replace <MigrationName> with a descriptive name)
dotnet ef migrations add <MigrationName> \
  --project src/Platform/Lunara.Infrastructure.Host \
  --startup-project src/Platform/Lunara.Api

# Apply migrations to the local database
dotnet ef database update \
  --project src/Platform/Lunara.Infrastructure.Host \
  --startup-project src/Platform/Lunara.Api
```

> The `ASPNETCORE_ENVIRONMENT=Development` environment variable is picked up automatically
> by the design-time factory in `LunaraDbContextFactory`, so no extra flags are needed.

---

## 4. Run the API

```bash
dotnet run --project src/Platform/Lunara.Api
```

Default endpoints:

| Endpoint | Description |
|----------|-------------|
| `GET /health` | Liveness probe |
| `GET /ready` | Readiness probe |
| `POST /v1/social/swipe` | Record a swipe (Social module) |

---

## 5. Run the Worker

```bash
dotnet run --project src/Platform/Lunara.Worker
```

The Worker hosts `OutboxPublisherHostedService`, which polls the outbox table every 2 s and
publishes pending messages to Kafka.

---

## 6. Run tests

```bash
dotnet test
```

| Project | Tests |
|---------|-------|
| `Lunara.UnitTests` | Domain + use-case unit tests |
| `Lunara.ArchTests` | Clean Architecture + forbidden-library rules |
