# Local Observability

This folder contains the local Grafana observability stack for `FCG.Payments`.

## Services

- `grafana` on `http://localhost:3000`
- `prometheus` on `http://localhost:9090`
- `loki` on `http://localhost:3100`
- `tempo` on `http://localhost:3200`
- `otel-collector` on `http://localhost:4317` (gRPC), `http://localhost:4318` (HTTP), and `http://localhost:9464/metrics`

## Default Credentials

- Grafana user: `admin`
- Grafana password: `admin`
- Seq user: `admin`
- Seq password: `YourPassword123`

## Start The Stack

```bash
docker compose up -d --build
```

To restart only the application after telemetry changes:

```bash
docker compose up -d --build fcg-payments
```

To stop everything:

```bash
docker compose down
```

To remove persisted observability data too:

```bash
docker compose down -v
```

## What Is Provisioned Automatically

- Grafana datasources for Prometheus, Loki, and Tempo
- Grafana dashboards:
  - `FCG Payments - API Overview`
  - `FCG Payments - Payments Overview`
  - `FCG Payments - Kafka and Trace Overview`

## Validate The Stack

1. Open `http://localhost:3000` and confirm the dashboards are visible.
2. Open `http://localhost:5054/health` and confirm it returns `Healthy`.
3. Call an authenticated endpoint without a token to generate HTTP telemetry:

```bash
curl -i http://localhost:5054/api/v1/payments/history
```

4. Publish a `user-created` event followed by an `order-placed` event to validate Kafka + tracing:

```bash
USER_ID=$(uuidgen)
GAME_ID=$(uuidgen)
CORRELATION_ID=$(uuidgen)

printf '{"UserId":"%s","Name":"Telemetry User","Email":"telemetry.user@example.com","CorrelationId":"%s","CreatedAt":"2026-03-06T22:00:00Z"}\n' "$USER_ID" "$CORRELATION_ID" \
  | docker exec -i kafka kafka-console-producer --bootstrap-server kafka:29092 --topic user-created

printf '{"UserEmail":"telemetry.user@example.com","CorrelationId":"%s","UserId":"%s","GameId":"%s","Amount":10.0,"CreatedAt":"2026-03-06T22:00:05Z"}\n' "$CORRELATION_ID" "$USER_ID" "$GAME_ID" \
  | docker exec -i kafka kafka-console-producer --bootstrap-server kafka:29092 --topic order-placed
```

5. Confirm the following:
- Prometheus exposes `payments_processed_total`
- Prometheus exposes `application_command_executions_commands_total`
- Loki contains logs with `service_name="FCG/FCG.Payments"`
- Tempo search returns traces for `FCG.Payments`
- Grafana dashboards render data without manual datasource setup

## Useful Verification Commands

Metrics exported by the collector:

```bash
curl -s http://localhost:9464/metrics
```

Current stack status:

```bash
docker compose ps
```

Application and collector logs:

```bash
docker compose logs --tail=100 fcg-payments otel-collector
```

Recent traces from Tempo:

```bash
curl -sG "http://localhost:3200/api/search" --data-urlencode 'limit=10'
```

Recent logs from Loki:

```bash
START=$(date -u -d '15 minutes ago' +%s%N)
END=$(date -u +%s%N)
curl -sG "http://localhost:3100/loki/api/v1/query_range" \
  --data-urlencode 'query={service_name="FCG/FCG.Payments"}' \
  --data-urlencode "start=$START" \
  --data-urlencode "end=$END"
```

## Troubleshooting

- If `fcg-payments` is stuck in `Created`, run `docker compose up -d fcg-payments` after Kafka and SQL Server are healthy.
- If Loki fails to start, validate the file at `observability/loki-config.yaml` and restart only Loki with `docker compose up -d loki`.
- If Grafana shows empty panels, confirm data first in Prometheus, Loki, and Tempo before debugging dashboards.
- If Kafka consumers log `Unknown topic or partition`, publish a message once so Kafka auto-creates the topic, then retry the validation flow.
- If traces appear but logs do not, inspect `otel-collector` logs and verify the app container has `OTEL_SERVICE_NAME` and `OTEL_RESOURCE_ATTRIBUTES` configured.
- If dashboards look empty right after startup, wait one Prometheus scrape interval and refresh Grafana.
