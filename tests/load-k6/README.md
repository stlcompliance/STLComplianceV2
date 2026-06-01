# STL Compliance k6 load-test harness

Product-owner SLO targets (V1) are the active baseline. See `docs/operations/PRODUCT_OWNER_LOAD_SLO_V1.md` and `slo-product-owner.json`. Set `STL_LOAD_SLO_PROFILE=engineering-defaults` for development fallback thresholds in `slo-defaults.json`.

## Scenarios

| Scenario | Script | PO SLO (p95 / error rate / min requests) |
|----------|--------|-------------------------------------------|
| `api-health-liveness` | `scenarios/api-health-liveness.js` | 400ms / 0.5% / 50 |
| `api-health-ready` | `scenarios/api-health-ready.js` | 1500ms / 1% / 50 |
| `nexarr-platform-health` | `scenarios/nexarr-platform-health.js` | 4000ms / 3% / 20 |
| `nexarr-auth-me` | `scenarios/nexarr-auth-me.js` | 1200ms / 1% / 30 |
| `product-auth-handoff-me` | `scenarios/product-auth-handoff-me.js` | 6000ms / 3% / 12 |
| `trainarr-qualification-check` | `scenarios/trainarr-qualification-check.js` | 10000ms / 4% / 10 |
| `routarr-dispatch-workflow-gate` | `scenarios/routarr-dispatch-workflow-gate.js` | 12000ms / 4% / 8 |
| `staffarr-person-readiness` | `scenarios/staffarr-person-readiness.js` | 8000ms / 4% / 10 |
| `supplyarr-procurement-pr` | `scenarios/supplyarr-procurement-pr.js` | 15000ms / 5% / 6 |
| `maintainarr-work-order` | `scenarios/maintainarr-work-order.js` | 18000ms / 5% / 6 |
| `compliancecore-rule-evaluate` | `scenarios/compliancecore-rule-evaluate.js` | 12000ms / 4% / 8 |

Set `STL_LOAD_JOURNEY_TRIP_ID` (from `routarr-staging-journey-seed`) to reuse the seeded dispatch trip mirror and skip per-iteration `POST /api/trips`.

Set `STL_LOAD_JOURNEY_RULE_PACK_ID` (from `compliancecore-staging-journey-seed`) to reuse a seeded rule pack and skip per-iteration journey seed in `compliancecore-rule-evaluate`.

## Prerequisites

- [k6](https://k6.io/docs/get-started/installation/) on PATH
- Docker-compose APIs running (all seven for ready/platform/journey scenarios)

```powershell
docker compose up -d postgres nexarr-api staffarr-api trainarr-api maintainarr-api routarr-api supplyarr-api compliancecore-api
```

## Run locally

```powershell
# From repo root — runs all eleven PO scenarios and validates summaries with Shared evaluator
./scripts/ops/load-test-run.ps1

# Single scenario
./scripts/ops/load-test-run.ps1 -Scenario trainarr-qualification-check

# Engineering fallback profile
$env:STL_LOAD_SLO_PROFILE = "engineering-defaults"
./scripts/ops/load-test-run.ps1 -Scenario api-health-liveness -Vus 2 -Duration 10s
```

Linux/macOS:

```bash
chmod +x scripts/ops/load-test-run.sh
./scripts/ops/load-test-run.sh trainarr-qualification-check
```

## Environment variables

| Variable | Default | Purpose |
|----------|---------|---------|
| `STL_LOAD_SLO_PROFILE` | `product-owner` | Active SLO profile (`engineering-defaults` for dev fallback) |
| `STL_NEXARR_BASE_URL` | `http://localhost:5101` | NexArr API base |
| `STL_STAFFARR_BASE_URL` | `http://localhost:5102` | StaffArr API base |
| `STL_TRAINARR_BASE_URL` | `http://localhost:5103` | TrainArr API base |
| `STL_MAINTAINARR_BASE_URL` | `http://localhost:5104` | MaintainArr API base |
| `STL_ROUTARR_BASE_URL` | `http://localhost:5105` | RoutArr API base |
| `STL_SUPPLYARR_BASE_URL` | `http://localhost:5106` | SupplyArr API base |
| `STL_COMPLIANCECORE_BASE_URL` | `http://localhost:5107` | Compliance Core API base |
| `STL_LOAD_DEMO_EMAIL` | `admin@demo.stl` | NexArr login email |
| `STL_LOAD_DEMO_PASSWORD` | `ChangeMe!Demo2026` | NexArr login password |
| `STL_LOAD_DEMO_TENANT_ID` | demo tenant GUID | NexArr login tenant |
| `STL_LOAD_SUBJECT_PERSON_ID` | demo admin user GUID | Qualification/gate journey subject |
| `STL_LOAD_QUALIFICATION_KEY` | `hazmat_endorsement` | TrainArr qualification check key |
| `STL_LOAD_RULE_PACK_KEY` | `driver_qualification` | Compliance Core rule pack key |
| `STL_LOAD_JOURNEY_RULE_PACK_ID` | unset | Reuse seeded rule pack GUID (skip per-iter seed) |
| `STL_LOAD_DRIVER_LICENSE_FACT_KEY` | `driver_license_valid` | Fact key for Compliance Core evaluate |
| `STL_LOAD_VUS` | `5` (3 for platform health) | Virtual users |
| `STL_LOAD_DURATION` | `30s` | Scenario duration |
| `LOAD_LIVE` | unset | Set to `1` for optional live k6 tests in `STLCompliance.Load.Tests` |

## CI

- **Default CI** (`Category=Load`, not Live): unit tests for SLO catalog, k6 summary parsing, and evaluator logic.
- **Nightly** (`e2e-nightly.yml`): live k6 run for all eleven product-owner scenarios when `LOAD_LIVE=1`.
- **Render staging** (`load-staging-render.yml`): manual workflow_dispatch and **weekly schedule** (Sunday 07:00 UTC) against `RENDER_STAGING_*_API_URL` secrets — see `docs/operations/RENDER_STAGING_LOAD_SOAK_V1.md`.

## Render staging soak

```powershell
$env:RENDER_STAGING_NEXARR_API_URL = "https://nexarr-api-3zlb.onrender.com"
# ... all seven RENDER_STAGING_*_API_URL values ...
./scripts/ops/render-staging-load-soak.ps1
```

## .NET validation

```powershell
dotnet test tests/STLCompliance.Load.Tests/STLCompliance.Load.Tests.csproj -c Release --filter "Category=Load&Category!=Live"
```

Evaluate a k6 summary export manually:

```csharp
var result = StlLoadTestSloEvaluator.EvaluateFile("api-health-liveness", "./summary.json");
```
