#!/usr/bin/env bash
set -euo pipefail

ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/../.." && pwd)"
HOST="${E2E_PREVIEW_HOST:-127.0.0.1}"
LOG_DIR="$ROOT/.e2e-preview-logs"
mkdir -p "$LOG_DIR"

start_preview() {
  local app="$1"
  local port="$2"
  local proxy_var="$3"
  local proxy_target="$4"
  local app_path="$ROOT/apps/$app"
  echo "Building $app..."
  (cd "$app_path" && npm ci --silent && npm run build --silent)
  echo "Starting preview $app on :$port"
  (
    cd "$app_path"
    export "${proxy_var}=${proxy_target}"
    nohup npm run preview -- --host "$HOST" --port "$port" >"$LOG_DIR/$app.log" 2>&1 &
  )
}

start_preview suite-frontend 5174 VITE_NEXARR_PROXY_TARGET http://127.0.0.1:5101
start_preview staffarr-frontend 5175 VITE_STAFFARR_PROXY_TARGET http://127.0.0.1:5102
start_preview trainarr-frontend 5176 VITE_TRAINARR_PROXY_TARGET http://127.0.0.1:5103
start_preview compliancecore-frontend 5177 VITE_COMPLIANCECORE_PROXY_TARGET http://127.0.0.1:5107
start_preview maintainarr-frontend 5178 VITE_MAINTAINARR_PROXY_TARGET http://127.0.0.1:5104
start_preview supplyarr-frontend 5179 VITE_SUPPLYARR_PROXY_TARGET http://127.0.0.1:5106
start_preview routarr-frontend 5180 VITE_ROUTARR_PROXY_TARGET http://127.0.0.1:5105

echo "Building companion-frontend..."
(
  cd "$ROOT/apps/companion-frontend"
  export VITE_NEXARR_PROXY_TARGET=http://127.0.0.1:5101
  export VITE_TRAINARR_FRONTEND_BASE="http://${HOST}:5176"
  export VITE_STAFFARR_FRONTEND_BASE="http://${HOST}:5175"
  export VITE_MAINTAINARR_FRONTEND_BASE="http://${HOST}:5178"
  export VITE_ROUTARR_FRONTEND_BASE="http://${HOST}:5180"
  export VITE_SUPPLYARR_FRONTEND_BASE="http://${HOST}:5179"
  export VITE_COMPLIANCECORE_FRONTEND_BASE="http://${HOST}:5177"
  npm ci --silent && npm run build --silent
  nohup npm run preview -- --host "$HOST" --port 5181 >"$LOG_DIR/companion-frontend.log" 2>&1 &
)

echo "Waiting for suite frontend on http://${HOST}:5174 ..."
for i in $(seq 1 30); do
  if curl -sf "http://${HOST}:5174/" >/dev/null; then
    echo "Suite frontend reachable."
    exit 0
  fi
  sleep 2
done

echo "Timed out waiting for suite frontend preview" >&2
exit 1
