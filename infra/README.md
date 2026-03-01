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
docker compose -f infra/docker-compose.yml up -d postgres zookeeper kafka kafka-ui loki promtail grafana

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

### Grafana

Log viewer and dashboard at **http://localhost:3000**.

| Property | Value |
|----------|-------|
| URL | `http://localhost:3000` |
| Username | `admin` |
| Password | `admin` |

The **Loki** datasource is provisioned automatically on first start — no manual setup needed.

### Loki

Log aggregation backend. Not accessed directly; queried through Grafana. Port `3100` is exposed for debugging only.

### Promtail

Sidecar log shipper. Reads all Lunara container logs via the Docker socket and forwards them to Loki with `container`, `service`, and `logstream` labels.

---

## Viewing logs in Grafana

### Opening Grafana

1. Navigate to [http://localhost:3000](http://localhost:3000).
2. Log in with **admin / admin**.
3. Go to **Explore** (compass icon in the left sidebar).
4. Select the **Loki** datasource (auto-selected as default).

### LogQL query examples

```logql
# All logs from the API container
{service="api"}

# All logs from the Worker container
{service="worker"}

# All Lunara container logs combined
{container=~"lunara-.*"}

# Filter for warnings and errors across all services
{container=~"lunara-.*"} |= "warn" or {container=~"lunara-.*"} |= "error"

# Database readiness failures only
{service="api"} |= "Database readiness check"

# Outbox publisher activity
{service="worker"} |= "Outbox publisher"

# Errors with structured fields (JSON log parsing)
{service="api"} | json | level =~ "(warn|error|crit)"
```

### Saving a dashboard

1. In **Explore**, run a query.
2. Click **Add to dashboard** to persist the panel.
3. Dashboards are stored in the `grafana_data` volume and survive container restarts.

---

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
|------|----------|
| `5432` | PostgreSQL |
| `9092` | Kafka (host listener) |
| `5000` | Lunara.Api |
| `8080` | Kafka UI |
| `3000` | Grafana |
| `3100` | Loki |

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

