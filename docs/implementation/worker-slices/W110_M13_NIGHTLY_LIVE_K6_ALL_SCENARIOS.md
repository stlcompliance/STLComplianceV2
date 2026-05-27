# M13 nightly live k6 — all seven PO scenarios

## Slice name

M13 nightly live k6 expansion — live scenario catalog, theory-based Load live tests for all seven product-owner k6 scenarios, seven-API health gate, nightly workflow updates

## Products touched

- **STLCompliance.Shared** — `StlLoadTestLiveScenarioCatalog`
- **tests/STLCompliance.Load.Tests** — refactored `LoadTestLiveTests` (7 scenarios via `[Theory]`)
- **CI** — `.github/workflows/e2e-nightly.yml` load-test-live job
- **tests/load-k6/README.md** — nightly coverage note

## Shared additions

| File | Purpose |
|------|---------|
| `StlLoadTestLiveScenarioCatalog.cs` | Live probe VUs/duration + smoke min-request overrides |
| `ResolveLiveSloTarget()` | Product-owner p95/error with reduced min requests for short nightly runs |

## Live scenarios (nightly)

| Key | VUs | Duration | Live min requests |
|-----|-----|----------|-------------------|
| `api-health-liveness` | 2 | 10s | 20 |
| `api-health-ready` | 2 | 10s | 20 |
| `nexarr-platform-health` | 2 | 10s | 10 |
| `nexarr-auth-me` | 2 | 10s | 15 |
| `product-auth-handoff-me` | 2 | 15s | 6 |
| `trainarr-qualification-check` | 2 | 15s | 5 |
| `routarr-dispatch-workflow-gate` | 2 | 15s | 4 |

## CI changes

- `load-test-live` waits for all seven API `/health` endpoints
- Step renamed to **Live k6 load tests (all seven PO scenarios)**
- Sets `STL_LOAD_SLO_PROFILE=product-owner`
- Job timeout increased to 30 minutes

## Tests

### Backend unit (`STLCompliance.Load.Tests`)

- `Live_catalog_covers_all_product_owner_scenarios`
- `ResolveLiveSloTarget_lowers_min_request_count_for_smoke`
- `K6_scenario_meets_product_owner_live_slo` × 7 (Live category, skipped unless `LOAD_LIVE=1`)

## Verification commands

```powershell
dotnet build STLCompliance.slnx -c Release
dotnet test tests/STLCompliance.Load.Tests/STLCompliance.Load.Tests.csproj -c Release --filter "Category=Load&Category!=Live"
```

Optional local live run (docker-compose + k6):

```powershell
docker compose up -d postgres nexarr-api staffarr-api trainarr-api maintainarr-api routarr-api supplyarr-api compliancecore-api
$env:LOAD_LIVE = "1"
dotnet test tests/STLCompliance.Load.Tests/STLCompliance.Load.Tests.csproj -c Release --filter "Category=Load&Category=Live"
```

## Remaining gaps

- Render staging soak against PO SLOs not scheduled
- Journey scenarios may warn/block when Compliance Core seeds missing (HTTP 200 still expected)
- StaffArr person export bundle not started

## Next recommended slice

StaffArr person export bundle, or Render staging load soak against PO SLOs.
