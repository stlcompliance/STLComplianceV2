# Worker 143 — M13 k6 load harness journey extensions

## Scope

Extend `tests/load-k6` beyond the original seven product-owner scenarios with four deeper authenticated journeys:

| Scenario key | Journey |
|--------------|---------|
| `staffarr-person-readiness` | Handoff → `GET /api/people/{personId}/readiness` |
| `supplyarr-procurement-pr` | Handoff → vendor/part → PR create → submit → approve |
| `maintainarr-work-order` | Handoff → asset class/type/asset → work order create → GET |
| `compliancecore-rule-evaluate` | Handoff → journey seed → `POST /api/rule-packs/{id}/evaluate` |

Also: `stl-journey.js` helpers, `stl-config` env (`STL_LOAD_JOURNEY_RULE_PACK_ID`, `STL_LOAD_DRIVER_LICENSE_FACT_KEY`), `StlLoadTestSloCatalog` + live/staging catalogs (11 scenarios), `slo-product-owner.json`, ops scripts, Load.Tests, README + PO SLO doc.

## Out of scope

- Playwright E2E additions
- New product API features solely for load testing (reuses existing endpoints)

## Verification

```bash
dotnet test tests/STLCompliance.Load.Tests/STLCompliance.Load.Tests.csproj -c Release --filter "Category=Load&Category!=Live"

# With docker-compose APIs up:
./scripts/ops/load-test-run.ps1 -Scenario staffarr-person-readiness -Vus 2 -Duration 10s

# Live (optional):
LOAD_LIVE=1 dotnet test tests/STLCompliance.Load.Tests/STLCompliance.Load.Tests.csproj -c Release --filter "Category=Load&Category=Live"
```
