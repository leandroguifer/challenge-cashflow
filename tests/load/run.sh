#!/usr/bin/env bash
set -euo pipefail

ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/../.." && pwd)"
LOAD_DIR="$ROOT/tests/load"
RESULTS_DIR="$LOAD_DIR/results"
ENV_FILE="${ENV_FILE:-$ROOT/.env}"

K6_IMAGE="${K6_IMAGE:-grafana/k6:2.4.0}"
COMPOSE_NETWORK="${COMPOSE_NETWORK:-cashflow_default}"
SEED="${SEED:-true}"

env_value() {
  grep -E "^$1=" "$ENV_FILE" | head -n 1 | cut -d= -f2-
}

native_path() {
  (cd "$1" && (pwd -W 2>/dev/null || pwd))
}

POSTGRES_USER="$(env_value POSTGRES_USER)"
KEYCLOAK_REALM="${KEYCLOAK_REALM:-$(env_value KEYCLOAK_REALM)}"
KEYCLOAK_CLIENT_ID="${KEYCLOAK_CLIENT_ID:-$(env_value KEYCLOAK_WEB_CLIENT_ID)}"

mkdir -p "$RESULTS_DIR"

cd "$ROOT"

if [ "$SEED" = "true" ]; then
  echo "Seeding the balance database"
  docker compose exec -T postgres-balance psql -v ON_ERROR_STOP=1 -U "$POSTGRES_USER" -d balance < "$LOAD_DIR/seed-balance.sql"
fi

STAMP="$(date -u +%Y%m%dT%H%M%SZ)"
SUMMARY_JSON="balance-$STAMP.json"
SUMMARY_TEXT="balance-$STAMP.txt"
SAMPLES_CSV="balance-$STAMP.csv"

echo "Running k6 ($K6_IMAGE) on network $COMPOSE_NETWORK"

set +e
MSYS_NO_PATHCONV=1 docker run --rm \
  --network "$COMPOSE_NETWORK" \
  -e BALANCE_URL="${BALANCE_URL:-http://balance:8080}" \
  -e KEYCLOAK_URL="${KEYCLOAK_URL:-http://keycloak:8080}" \
  -e KEYCLOAK_REALM="$KEYCLOAK_REALM" \
  -e KEYCLOAK_CLIENT_ID="$KEYCLOAK_CLIENT_ID" \
  -e LOAD_USER="${LOAD_USER:-proprietario}" \
  -e LOAD_PASSWORD="${LOAD_PASSWORD:-cashflow}" \
  -e RATE="${RATE:-50}" \
  -e DURATION="${DURATION:-5m}" \
  -e PRE_ALLOCATED_VUS="${PRE_ALLOCATED_VUS:-50}" \
  -e MAX_VUS="${MAX_VUS:-200}" \
  -v "$(native_path "$LOAD_DIR"):/work" \
  "$K6_IMAGE" run \
  --quiet \
  --summary-export "/work/results/$SUMMARY_JSON" \
  --out "csv=/work/results/$SAMPLES_CSV" \
  /work/balance.js 2>&1 | tee "$RESULTS_DIR/$SUMMARY_TEXT"
STATUS=${PIPESTATUS[0]}
set -e

if [ -f "$RESULTS_DIR/$SUMMARY_JSON" ]; then
  sed -i -E 's/("token": *)"[^"]*"/\1"removido"/' "$RESULTS_DIR/$SUMMARY_JSON"
fi

echo "Summary saved to tests/load/results/$SUMMARY_JSON and tests/load/results/$SUMMARY_TEXT"

exit "$STATUS"
