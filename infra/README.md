# Lunara — Local Infrastructure

Docker Compose stack for local development. Runs PostgreSQL, Kafka (+ Zookeeper), Kafka UI, Lunara.Api, and Lunara.Worker.

## Prerequisites

| Tool | Minimum version |
|------|----------------|
| Docker Desktop (macOS) | 4.x |
| Docker Compose | v2 (bundled with Docker Desktop) |

---

## First-time setup

```bash
cp infra/.env.example infra/.env   # create local secrets file (git-ignored)
```

Edit `infra/.env` if you want non-default credentials.

---

## Commands

All commands should be run from the **repository root**.

```bash
# Build images (or rebuild after code changes) and start all services
docker compose -f infra/docker-compose.yml up -d --build

# Start only infrastructure (skip api + worker — useful during active development)
docker compose -f infra/docker-compose.yml up -d postgres zookeeper kafka kafka-ui

# Start a single service
docker compose -f infra/docker-compose.yml up -d postgres

# Stop and remove containers (volumes are preserved)
docker compose -f infra/docker-compose.yml down

# Stop and remove containers + all volumes (⚠ deletes data)
docker compose -f infra/docker-compose.yml down -v

# View logs for all services
docker compose -f infra/docker-compose.yml logs -f

# View logs for a specific service
docker compose -f infra/docker-compose.yml logs -f api

# Check status and health
docker compose -f infra/docker-compose.yml ps
```

> **Note** — `api` and `worker` wait for `service_healthy` on `postgres` and `kafka` before starting.
> Kafka has a 30 s start period, so first-run startup typically takes ~45 s.

---

## Services

### PostgreSQL

| Property | Value |
|----------|-------|
| Host | `localhost` |
| Port | `5432` |
| User | `lunara` (from `.env`) |
| Password | `lunara` (from `.env`) |
| Database | `lunara` (from `.env`) |

### Kafka

| Property | Value |
|----------|-------|
| Bootstrap servers (host) | `localhost:9092` |
| Bootstrap servers (container-to-container) | `kafka:29092` |

### Kafka UI

Browse topics, consumer groups, and messages at **http://localhost:8080**.

### Lunara.Api

REST API exposed at **http://localhost:5000**.

### Lunara.Worker

Background worker — no exposed port. Polls the transactional outbox and publishes events to Kafka.

---

## Calling the API

```bash
# Health / smoke-test — record a swipe
curl -s -X POST http://localhost:5000/swipes \
  -H "Content-Type: application/json" \
  -H "X-User-Id: 00000000-0000-0000-0000-000000000001" \
  -d '{"targetUserId": "00000000-0000-0000-0000-000000000002", "action": "Like"}' \
  | jq .

# Record a dislike
curl -s -X POST http://localhost:5000/swipes \
  -H "Content-Type: application/json" \
  -H "X-User-Id: 00000000-0000-0000-0000-000000000001" \
  -d '{"targetUserId": "00000000-0000-0000-0000-000000000003", "action": "Pass"}' \
  | jq .
```

> Replace `X-User-Id` with any valid UUID. A match is created when both users like each other, which triggers a `lunara.social.match-created.v1` outbox event.

---

## Viewing Kafka topics & messages

### Via Kafka UI (easiest)

1. Open [http://localhost:8080](http://localhost:8080).
2. Select the **local** cluster.
3. Navigate to **Topics** to browse or produce/consume messages.
4. Look for `lunara.social.match-created.v1` after a mutual like is recorded.

### Via console tools (Confluent CLI inside the `kafka` container)

```bash
# List topics
docker exec lunara-kafka kafka-topics --bootstrap-server localhost:9092 --list

# Create a topic manually
docker exec lunara-kafka kafka-topics \
  --bootstrap-server localhost:9092 \
  --create --topic lunara.social.match-created.v1 --partitions 1 --replication-factor 1

# Consume all messages from a topic (Ctrl+C to exit)
docker exec -it lunara-kafka kafka-console-consumer \
  --bootstrap-server localhost:9092 \
  --topic lunara.social.match-created.v1 \
  --from-beginning

# Describe a topic
docker exec lunara-kafka kafka-topics \
  --bootstrap-server localhost:9092 \
  --describe --topic lunara.social.match-created.v1
```

---

## Running the API/Worker outside Docker (development mode)

When running `dotnet run` locally instead of via Docker, use these `appsettings.Development.json` values (pointing at the infra containers):

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

Start only the infrastructure services so there are no port conflicts:

```bash
docker compose -f infra/docker-compose.yml up -d postgres zookeeper kafka kafka-ui
```

---

## Troubleshooting

### Port conflicts

| Port | Service |
|------|---------|
| `5432` | PostgreSQL |
| `9092` | Kafka (host listener) |
| `5000` | Lunara.Api |
| `8080` | Kafka UI |

Stop the conflicting process or change the host-side port mapping in `docker-compose.yml`.

### Kafka not ready

The Kafka healthcheck has a 30 s start period. Wait for `docker compose ps` to show `healthy` before connecting:

```bash
docker compose -f infra/docker-compose.yml ps
```

### api / worker exit immediately

Check logs for startup errors — most commonly a missing EF Core migration:

```bash
docker compose -f infra/docker-compose.yml logs api
docker compose -f infra/docker-compose.yml logs worker
```

Run pending migrations from the host:

```bash
dotnet ef database update \
  --project src/Platform/Lunara.Infrastructure.Host \
  --startup-project src/Platform/Lunara.Api
```

### Resetting state

```bash
docker compose -f infra/docker-compose.yml down -v   # ⚠ destroys all data
docker compose -f infra/docker-compose.yml up -d --build
```

