#!/usr/bin/env bash
set -euo pipefail

SCENARIO="${1:-all}"
VUS="${STL_LOAD_VUS:-5}"
DURATION="${STL_LOAD_DURATION:-30s}"
OUTPUT_DIRECTORY="${RENDER_STAGING_LOAD_OUTPUT_DIRECTORY:-}"
SKIP_HEALTH_GATE="${RENDER_STAGING_SKIP_HEALTH_GATE:-false}"
SKIP_SLO_VALIDATION="${RENDER_STAGING_SKIP_SLO_VALIDATION:-false}"

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
LOAD_TEST_SCRIPT="$SCRIPT_DIR/load-test-run.sh"

declare -A SOURCE_ENVS=(
  [nexarr]=RENDER_STAGING_NEXARR_API_URL
  [staffarr]=RENDER_STAGING_STAFFARR_API_URL
  [trainarr]=RENDER_STAGING_TRAINARR_API_URL
  [maintainarr]=RENDER_STAGING_MAINTAINARR_API_URL
  [routarr]=RENDER_STAGING_ROUTARR_API_URL
  [supplyarr]=RENDER_STAGING_SUPPLYARR_API_URL
  [compliancecore]=RENDER_STAGING_COMPLIANCECORE_API_URL
)

declare -A TARGET_ENVS=(
  [nexarr]=STL_NEXARR_BASE_URL
  [staffarr]=STL_STAFFARR_BASE_URL
  [trainarr]=STL_TRAINARR_BASE_URL
  [maintainarr]=STL_MAINTAINARR_BASE_URL
  [routarr]=STL_ROUTARR_BASE_URL
  [supplyarr]=STL_SUPPLYARR_BASE_URL
  [compliancecore]=STL_COMPLIANCECORE_BASE_URL
)

normalize_base_url() {
  local raw="$1"
  raw="${raw%/}"
  if [[ "$raw" =~ ^https?:// ]]; then
    printf '%s' "$raw"
  else
    printf 'https://%s' "$raw"
  fi
}

resolve_staging_endpoints() {
  local missing=()
  for product in nexarr staffarr trainarr maintainarr routarr supplyarr compliancecore; do
    local source_env="${SOURCE_ENVS[$product]}"
    local target_env="${TARGET_ENVS[$product]}"
    local raw="${!source_env:-}"
    if [[ -z "$raw" ]]; then
      missing+=("$source_env")
      continue
    fi
    export "$target_env=$(normalize_base_url "$raw")"
  done

  if ((${#missing[@]} > 0)); then
    echo "Missing staging API URL environment variables: ${missing[*]}" >&2
    exit 1
  fi
}

health_gate() {
  for product in nexarr staffarr trainarr maintainarr routarr supplyarr compliancecore; do
    local target_env="${TARGET_ENVS[$product]}"
    local base_url="${!target_env}"
    local health_url="${base_url%/}/health"
    if ! curl -sf --max-time 15 "$health_url" >/dev/null; then
      echo "Staging health gate failed for $product at $health_url" >&2
      exit 1
    fi
    echo "Healthy: $health_url"
  done
}

if [[ -z "$OUTPUT_DIRECTORY" ]]; then
  OUTPUT_DIRECTORY="$(mktemp -d /tmp/stl-render-staging-load-XXXXXX)"
fi
mkdir -p "$OUTPUT_DIRECTORY"
export RENDER_STAGING_LOAD_OUTPUT_DIRECTORY="$OUTPUT_DIRECTORY"
export STL_LOAD_SLO_PROFILE=product-owner
export STL_LOAD_VUS="$VUS"
export STL_LOAD_DURATION="$DURATION"
export STL_LOAD_OUTPUT_DIR="$OUTPUT_DIRECTORY"

resolve_staging_endpoints

if [[ "$SKIP_HEALTH_GATE" != "true" ]]; then
  echo "Running Render staging API health gate..."
  health_gate
fi

echo "Starting Render staging load soak (product-owner SLO profile)..."

if [[ "$SKIP_SLO_VALIDATION" == "true" ]]; then
  echo "SLO validation skipped (RENDER_STAGING_SKIP_SLO_VALIDATION=true)"
  # load-test-run.sh always validates; invoke k6 directly when skipping SLO checks
  ALL_SCENARIOS=(
    api-health-liveness
    api-health-ready
    nexarr-platform-health
    nexarr-auth-me
    product-auth-handoff-me
    trainarr-qualification-check
    routarr-dispatch-workflow-gate
  )
  REPO_ROOT="$(cd "$SCRIPT_DIR/../.." && pwd)"
  K6_DIR="$REPO_ROOT/tests/load-k6"
  run_k6_only() {
    local scenario_key="$1"
    local script_path="$K6_DIR/scenarios/${scenario_key}.js"
    local summary_path="$OUTPUT_DIRECTORY/${scenario_key}-summary.json"
    STL_LOAD_VUS="$VUS" STL_LOAD_DURATION="$DURATION" \
      k6 run "$script_path" --summary-export "$summary_path"
  }
  if [[ "$SCENARIO" == "all" ]]; then
    for scenario_key in "${ALL_SCENARIOS[@]}"; do
      run_k6_only "$scenario_key"
    done
  else
    run_k6_only "$SCENARIO"
  fi
else
  chmod +x "$LOAD_TEST_SCRIPT"
  "$LOAD_TEST_SCRIPT" "$SCENARIO"
fi

echo ""
echo "Render staging load soak completed. Summaries in $OUTPUT_DIRECTORY"
