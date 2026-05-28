#!/usr/bin/env bash
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
REPO_ROOT="$(cd "$SCRIPT_DIR/../.." && pwd)"

NEXARR_BASE_URL="${RENDER_STAGING_NEXARR_API_URL:-${STL_NEXARR_BASE_URL:-http://localhost:5101}}"
COMPLIANCECORE_BASE_URL="${RENDER_STAGING_COMPLIANCECORE_API_URL:-${STL_COMPLIANCECORE_BASE_URL:-http://localhost:5107}}"

EMAIL="${STL_LOAD_DEMO_EMAIL:-admin@demo.stl}"
PASSWORD="${STL_LOAD_DEMO_PASSWORD:-ChangeMe!Demo2026}"
TENANT_ID="${STL_LOAD_DEMO_TENANT_ID:-11111111-1111-1111-1111-111111111101}"

normalize_base_url() {
  local raw="$1"
  raw="${raw%/}"
  if [[ "$raw" =~ ^https?:// ]]; then
    printf '%s' "$raw"
  else
    printf 'https://%s' "$raw"
  fi
}

NEXARR_BASE_URL="$(normalize_base_url "$NEXARR_BASE_URL")"
COMPLIANCECORE_BASE_URL="$(normalize_base_url "$COMPLIANCECORE_BASE_URL")"

json_post() {
  local url="$1"
  local body="$2"
  local auth_header="${3:-}"
  if [[ -n "$auth_header" ]]; then
    curl -sf -X POST "$url" \
      -H "Content-Type: application/json" \
      -H "Authorization: Bearer $auth_header" \
      -d "$body"
  else
    curl -sf -X POST "$url" \
      -H "Content-Type: application/json" \
      -d "$body"
  fi
}

echo "Logging into NexArr at $NEXARR_BASE_URL"
login_response="$(json_post "$NEXARR_BASE_URL/api/auth/login" "{\"email\":\"$EMAIL\",\"password\":\"$PASSWORD\",\"tenantId\":\"$TENANT_ID\"}")"
nexarr_token="$(printf '%s' "$login_response" | python -c "import json,sys; print(json.load(sys.stdin)['accessToken'])")"

echo "Creating Compliance Core handoff"
handoff_response="$(json_post "$NEXARR_BASE_URL/api/launch/handoff" '{"productKey":"compliancecore","callbackUrl":null}' "$nexarr_token")"
handoff_code="$(printf '%s' "$handoff_response" | python -c "import json,sys; print(json.load(sys.stdin)['handoffCode'])")"

echo "Redeeming Compliance Core session at $COMPLIANCECORE_BASE_URL"
redeem_response="$(json_post "$COMPLIANCECORE_BASE_URL/api/auth/handoff/redeem" "{\"handoffCode\":\"$handoff_code\"}")"
compliance_token="$(printf '%s' "$redeem_response" | python -c "import json,sys; print(json.load(sys.stdin)['accessToken'])")"

echo "Seeding load-test journey fixtures"
seed_response="$(json_post "$COMPLIANCECORE_BASE_URL/api/load-test-journey/seed" "{}" "$compliance_token")"
printf '%s\n' "$seed_response"

rule_pack_id="$(printf '%s' "$seed_response" | python -c "import json,sys; print(json.load(sys.stdin)['rulePackId'])")"
export STL_LOAD_JOURNEY_RULE_PACK_ID="$rule_pack_id"
if [[ -n "${GITHUB_ENV:-}" ]]; then
  echo "STL_LOAD_JOURNEY_RULE_PACK_ID=$rule_pack_id" >> "$GITHUB_ENV"
fi
echo "Set STL_LOAD_JOURNEY_RULE_PACK_ID=$rule_pack_id"

echo "Compliance Core load-test journey seed completed."
