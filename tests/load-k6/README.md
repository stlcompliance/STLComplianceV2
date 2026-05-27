# STL Compliance k6 load-test harness

Engineering-default SLO targets for M13 ship-gate load validation. Official product-owner SLOs will replace `slo-defaults.json` and `StlLoadTestSloCatalog` when published.

## Scenarios

| Scenario | Script | Default SLO (p95 / error rate / min requests) |
|----------|--------|-----------------------------------------------|
| `api-health-liveness` | `scenarios/api-health-liveness.js` | 500ms / 1% / 50 |
| `api-health-ready` | `scenarios/api-health-ready.js` | 2000ms / 2% / 50 |
| `nexarr-platform-health` | `scenarios/nexarr-platform-health.js` | 5000ms / 5% / 20 |

## Prerequisites

- [k6](https://k6.io/docs/get-started/installation/) on PATH
- Docker-compose APIs running (all seven for ready/platform scenarios)

```powershell
docker compose up -d postgres nexarr-api staffarr-api trainarr-api maintainarr-api routarr-api supplyarr-api compliancecore-api
```

## Run locally

```powershell
# From repo root — runs all scenarios and validates summaries with Shared evaluator
./scripts/ops/load-test-run.ps1

# Single scenario
./scripts/ops/load-test-run.ps1 -Scenario api-health-liveness

# Short smoke (fewer VUs / shorter duration)
./scripts/ops/load-test-run.ps1 -Scenario api-health-liveness -Vus 2 -Duration 10s
```

Linux/macOS:

```bash
chmod +x scripts/ops/load-test-run.sh
./scripts/ops/load-test-run.sh
```

## Environment variables

| Variable | Default | Purpose |
|----------|---------|---------|
| `STL_NEXARR_BASE_URL` | `http://localhost:5101` | NexArr API base |
| `STL_STAFFARR_BASE_URL` | `http://localhost:5102` | StaffArr API base |
| `STL_TRAINARR_BASE_URL` | `http://localhost:5103` | TrainArr API base |
| `STL_MAINTAINARR_BASE_URL` | `http://localhost:5104` | MaintainArr API base |
| `STL_ROUTARR_BASE_URL` | `http://localhost:5105` | RoutArr API base |
| `STL_SUPPLYARR_BASE_URL` | `http://localhost:5106` | SupplyArr API base |
| `STL_COMPLIANCECORE_BASE_URL` | `http://localhost:5107` | Compliance Core API base |
| `STL_LOAD_VUS` | `5` (3 for platform health) | Virtual users |
| `STL_LOAD_DURATION` | `30s` | Scenario duration |
| `LOAD_LIVE` | unset | Set to `1` for optional live k6 tests in `STLCompliance.Load.Tests` |

## CI

- **Default CI** (`Category=Load`, not Live): unit tests for SLO catalog, k6 summary parsing, and evaluator logic.
- **Nightly** (`e2e-nightly.yml`): optional live k6 run against docker-compose when `LOAD_LIVE=1`.

## .NET validation

```powershell
dotnet test tests/STLCompliance.Load.Tests/STLCompliance.Load.Tests.csproj -c Release --filter "Category=Load&Category!=Live"
```

Evaluate a k6 summary export manually:

```csharp
var result = StlLoadTestSloEvaluator.EvaluateFile("api-health-liveness", "./summary.json");
```
