# Lunara — Local Infrastructure

Docker Compose stack for local development. Runs PostgreSQL, Kafka (+ Zookeeper), and Kafka UI.

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
# Start all services in the background
docker compose -f infra/docker-compose.yml up -d

# Start a single service (e.g. postgres only)
docker compose -f infra/docker-compose.yml up -d postgres

# Stop and remove containers (volumes are preserved)
docker compose -f infra/docker-compose.yml down

# Stop and remove containers + all volumes (⚠ deletes data)
docker compose -f infra/docker-compose.yml down -v

# View logs
docker compose -f infra/docker-compose.yml logs -f

# Check health
docker compose -f infra/docker-compose.yml ps
```

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
| Connection string | `Host=localhost;Port=5432;Database=lunara;Username=lunara;Password=lunara` |

### Kafka

| Property | Value |
|----------|-------|
| Bootstrap servers (host) | `localhost:9092` |
| Bootstrap servers (container-to-container) | `kafka:29092` |

### Kafka UI

Browse topics, consumer groups, and messages at **http://localhost:8080**.

---

## Viewing Kafka topics & messages

### Via Kafka UI (easiest)

1. Open [http://localhost:8080](http://localhost:8080).
2. Select the **local** cluster.
3. Navigate to **Topics** to browse or produce/consume messages.

### Via console tools (Confluent CLI inside the `kafka` container)

```bash
# List topics
docker exec lunara-kafka kafka-topics --bootstrap-server localhost:9092 --list

# Create a topic manually
docker exec lunara-kafka kafka-topics \
  --bootstrap-server localhost:9092 \
  --create --topic lunara.social.swiped --partitions 1 --replication-factor 1

# Consume all messages from a topic
docker exec -it lunara-kafka kafka-console-consumer \
  --bootstrap-server localhost:9092 \
  --topic lunara.social.swiped \
  --from-beginning

# Describe a topic
docker exec lunara-kafka kafka-topics \
  --bootstrap-server localhost:9092 \
  --describe --topic lunara.social.swiped
```

---

## Application appsettings (Development)

Update `appsettings.Development.json` in `Lunara.Api` and `Lunara.Worker` to match:

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

---

## Troubleshooting

### Port conflicts

If port 5432 or 9092 is already in use, stop the conflicting process or change the host-side port mapping in `docker-compose.yml`.

### Kafka not ready

The Kafka healthcheck has a 30 s start period. Wait for `docker compose ps` to show `healthy` before connecting.

### Resetting state

```bash
docker compose -f infra/docker-compose.yml down -v   # ⚠ destroys all data
docker compose -f infra/docker-compose.yml up -d
```
