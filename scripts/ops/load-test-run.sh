#!/usr/bin/env bash
set -euo pipefail

SCENARIO="${1:-all}"
VUS="${STL_LOAD_VUS:-5}"
DURATION="${STL_LOAD_DURATION:-30s}"
OUTPUT_DIRECTORY="${STL_LOAD_OUTPUT_DIR:-}"

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
REPO_ROOT="$(cd "$SCRIPT_DIR/../.." && pwd)"
K6_DIR="$REPO_ROOT/tests/load-k6"

ALL_SCENARIOS=(
  api-health-liveness
  api-health-ready
  nexarr-platform-health
  nexarr-auth-me
  product-auth-handoff-me
  trainarr-qualification-check
  routarr-dispatch-workflow-gate
)

if ! command -v k6 >/dev/null 2>&1; then
  echo "k6 is not on PATH. Install from https://k6.io/docs/get-started/installation/" >&2
  exit 1
fi

if [[ -z "$OUTPUT_DIRECTORY" ]]; then
  OUTPUT_DIRECTORY="$(mktemp -d /tmp/stl-load-k6-XXXXXX)"
fi
mkdir -p "$OUTPUT_DIRECTORY"

run_scenario() {
  local scenario_key="$1"
  local script_path="$K6_DIR/scenarios/${scenario_key}.js"
  local summary_path="$OUTPUT_DIRECTORY/${scenario_key}-summary.json"

  if [[ ! -f "$script_path" ]]; then
    echo "Missing k6 script: $script_path" >&2
    exit 1
  fi

  echo "Running k6 scenario '$scenario_key' -> $summary_path"
  STL_LOAD_VUS="$VUS" STL_LOAD_DURATION="$DURATION" \
    k6 run "$script_path" --summary-export "$summary_path"

  LOAD_SCENARIO_KEY="$scenario_key" LOAD_SUMMARY_PATH="$summary_path" \
    dotnet test "$REPO_ROOT/tests/STLCompliance.Load.Tests/STLCompliance.Load.Tests.csproj" \
    -c Release --no-build \
    --filter "FullyQualifiedName~Evaluate_summary_file_from_environment" \
    --verbosity quiet
}

dotnet build "$REPO_ROOT/tests/STLCompliance.Load.Tests/STLCompliance.Load.Tests.csproj" -c Release -v q

if [[ "$SCENARIO" == "all" ]]; then
  for scenario_key in "${ALL_SCENARIOS[@]}"; do
    run_scenario "$scenario_key"
  done
elif [[ " ${ALL_SCENARIOS[*]} " == *" $SCENARIO "* ]]; then
  run_scenario "$SCENARIO"
else
  echo "Unknown scenario: $SCENARIO" >&2
  exit 1
fi

echo ""
echo "Load test harness completed. Summaries in $OUTPUT_DIRECTORY"
