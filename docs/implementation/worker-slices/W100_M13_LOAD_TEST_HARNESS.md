# M13 load-test harness (k6 + SLO evaluator)

## Slice name

M13 load-test harness — k6 scenarios, engineering-default SLO catalog, summary evaluator, operator scripts, automated Load tests, nightly live k6 smoke.

## Products touched

- **STLCompliance.Shared** — `Operations/LoadTesting/*` (SLO catalog, k6 summary parser, evaluator, API endpoints)
- **tests/load-k6** — three k6 scenarios + `slo-defaults.json`
- **scripts/ops** — `load-test-run.ps1`, `load-test-run.sh`
- **tests/STLCompliance.Load.Tests** — `Category=Load` unit tests + optional live k6 (`Category=Live`)
- **CI** — Load unit step in `.github/workflows/ci.yml`; live k6 in `.github/workflows/e2e-nightly.yml`

## Shared additions

| File | Purpose |
|------|---------|
| `StlLoadTestSloTarget.cs` | SLO threshold record |
| `StlLoadTestSloCatalog.cs` | Engineering-default scenario targets (placeholder until PO SLOs) |
| `StlLoadTestApiEndpoints.cs` | Local docker-compose base URLs |
| `StlLoadTestK6Summary.cs` | Parse k6 `--summary-export` JSON |
| `StlLoadTestSloEvaluator.cs` | Evaluate summary against SLO |
| `StlLoadTestSloEvaluationResult.cs` | Evaluation outcome |

## k6 scenarios

| Key | Script | Probes |
|-----|--------|--------|
| `api-health-liveness` | `tests/load-k6/scenarios/api-health-liveness.js` | All 7 APIs `/health` |
| `api-health-ready` | `tests/load-k6/scenarios/api-health-ready.js` | All 7 APIs `/health/ready` |
| `nexarr-platform-health` | `tests/load-k6/scenarios/nexarr-platform-health.js` | NexArr `/api/platform/health` |

## Operator workflow

```powershell
docker compose up -d postgres nexarr-api staffarr-api trainarr-api maintainarr-api routarr-api supplyarr-api compliancecore-api
./scripts/ops/load-test-run.ps1
```

Short smoke:

```powershell
./scripts/ops/load-test-run.ps1 -Scenario api-health-liveness -Vus 2 -Duration 10s
```

## Verification commands

```powershell
dotnet build STLCompliance.slnx -c Release
dotnet test STLCompliance.slnx -c Release --filter "Category!=Live"
dotnet test tests/STLCompliance.Load.Tests/STLCompliance.Load.Tests.csproj -c Release --filter "Category=Load&Category!=Live"
```

Optional live k6 (docker-compose + k6 on PATH):

```powershell
docker compose up -d postgres nexarr-api staffarr-api trainarr-api maintainarr-api routarr-api supplyarr-api compliancecore-api
$env:LOAD_LIVE = "1"
dotnet test tests/STLCompliance.Load.Tests/STLCompliance.Load.Tests.csproj -c Release --filter "Category=Load&Category=Live"
```

## Gap analysis update (M13)

| Area | Status after this slice |
|------|-------------------------|
| Load / performance | **Harness ready** — k6 scenarios + SLO evaluator with engineering defaults; replace SLO values when product owners publish targets |
| DR / backup restore | Scripted drill (W99) |
| Browser E2E | Playwright scaffold (W94) |

## Next slice

Replace engineering-default SLOs with product-owner targets and extend scenarios (authenticated API flows, cross-product journeys) once SLO document is published; or **full seven-database DR nightly drill** expansion.
