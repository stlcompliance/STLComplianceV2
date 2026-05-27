#!/usr/bin/env bash
set -euo pipefail

ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/../.." && pwd)"
cd "$ROOT"

APIS_ONLY=0
BUILD_FRONTENDS=0
for arg in "$@"; do
  case "$arg" in
    --apis-only) APIS_ONLY=1 ;;
    --build-frontends) BUILD_FRONTENDS=1 ;;
  esac
done

COMPOSE=(docker compose -f docker-compose.yml)
API_SERVICES=(postgres nexarr-api staffarr-api trainarr-api maintainarr-api routarr-api supplyarr-api compliancecore-api)

if [[ "$APIS_ONLY" -eq 1 ]]; then
  echo "Starting API stack for browser E2E..."
  "${COMPOSE[@]}" up -d --build "${API_SERVICES[@]}"
elif [[ "$BUILD_FRONTENDS" -eq 1 ]]; then
  echo "Starting full E2E stack (APIs + dockerized Vite previews)..."
  docker compose -f docker-compose.yml -f docker-compose.e2e.yml --profile e2e up -d --build \
    "${API_SERVICES[@]}" \
    suite-frontend-e2e staffarr-frontend-e2e trainarr-frontend-e2e \
    compliancecore-frontend-e2e maintainarr-frontend-e2e supplyarr-frontend-e2e routarr-frontend-e2e
else
  echo "Starting APIs via compose; use e2e-frontends-preview.sh for host Vite previews..."
  "${COMPOSE[@]}" up -d --build "${API_SERVICES[@]}"
fi

echo "Waiting for NexArr health..."
for i in $(seq 1 60); do
  if curl -sf http://localhost:5101/health >/dev/null; then
    echo "NexArr API healthy after ${i} attempt(s)."
    exit 0
  fi
  sleep 5
done

echo "Timed out waiting for NexArr API health" >&2
exit 1
