#!/usr/bin/env bash
set -euo pipefail

SCENARIO="${1:-all}"
VUS="${STL_LOAD_VUS:-5}"
DURATION="${STL_LOAD_DURATION:-30s}"
OUTPUT_DIRECTORY="${STL_LOAD_OUTPUT_DIR:-}"

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
REPO_ROOT="$(cd "$SCRIPT_DIR/../.." && pwd)"
K6_DIR="$REPO_ROOT/tests/load-k6"

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

case "$SCENARIO" in
  all)
    run_scenario api-health-liveness
    run_scenario api-health-ready
    run_scenario nexarr-platform-health
    run_scenario nexarr-auth-me
    run_scenario product-auth-handoff-me
    ;;
  api-health-liveness|api-health-ready|nexarr-platform-health|nexarr-auth-me|product-auth-handoff-me)
    run_scenario "$SCENARIO"
    ;;
  *)
    echo "Unknown scenario: $SCENARIO" >&2
    exit 1
    ;;
esac

echo ""
echo "Load test harness completed. Summaries in $OUTPUT_DIRECTORY"
