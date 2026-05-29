# W294 — M13 Playwright: SupplyArr purchasing demand processing smoke

Builds on **W179/W194** (demand processing worker + multi-source), **W246** (procurement coordination / demand-processing operator UX), **W236/W237** (SupplyArr Playwright admin smokes).

Adds **Playwright smoke** for the Purchasing workspace `DemandProcessingPanel` with an idempotent **load-test journey seed** API fixture.

## Scope

### Backend (`apps/supplyarr-api`)

| Change | Coverage |
|--------|----------|
| `StlSupplyArrLoadTestJourneySeedCatalog` | Shared conventions for journey demand ref title, work order number, part key |
| `LoadTestJourneySeedService` | Idempotent MaintainArr demand ref + short stock + demand-processing settings enable |
| `POST /api/load-test-journey/seed` | JWT `RequireDemandProcessingSettingsManage`, audit `load_test_journey.seed` |
| `SupplyArrLoadTestJourneySeedTests` | Idempotent seed, forbidden for buyer role |

### Frontend (`apps/supplyarr-frontend`)

| Change | Coverage |
|--------|----------|
| `DemandProcessingPanel` | Test ids: summary, pending queue, retry/create-pr/view-status per row |

### Playwright (`tests/e2e-playwright`)

| Spec | Route | Coverage |
|------|-------|----------|
| `supplyarr-purchasing-demand-processing-smoke.spec.ts` | `/purchasing` | Journey seed fixture → panel visible → pending row → operator buttons → view status detail with short line |

No retry-processing or create-PR-draft clicks (read-only operator UX verification).

### `e2eApi` helpers

- `seedSupplyArrDemandProcessingJourney`
- `ensureSupplyArrDemandProcessingFixture`

### Catalog

- `StlE2ePlaywrightSpecCatalog.SupplyArrPurchasingDemandProcessingSmokeSpec` in `ProductAdminSmokeSpecs` + `All`
- `StlE2ePlaywrightSpecCatalogTests.Product_admin_smoke_specs_include_maintainarr_through_compliancecore_w230_w294`

## Prerequisites (live)

```powershell
scripts/ops/e2e-stack-up.ps1
scripts/ops/e2e-frontends-preview.ps1
cd tests/e2e-playwright
$env:E2E_LIVE='1'
npx playwright test tests/supplyarr-purchasing-demand-processing-smoke.spec.ts
```

Requires SupplyArr API (5106) and frontend (5179). Demo admin / manager role with `canCreatePr` for operator controls.

## Verification

```powershell
dotnet build STLCompliance.slnx -c Release
dotnet test tests/STLCompliance.SupplyArr.Auth.Tests/STLCompliance.SupplyArr.Auth.Tests.csproj -c Release --filter "FullyQualifiedName~LoadTestJourneySeed|FullyQualifiedName~DemandProcessing"
dotnet test tests/STLCompliance.E2E/STLCompliance.E2E.csproj -c Release --filter "FullyQualifiedName~PlaywrightSpecCatalog"
dotnet test tests/STLCompliance.Load.Tests/STLCompliance.Load.Tests.csproj -c Release --filter "FullyQualifiedName~SupplyArr_journey_seed"
cd apps/supplyarr-frontend
npm run test -- DemandProcessingPanel
cd tests/e2e-playwright
npm install
# With live stack:
# $env:E2E_LIVE='1'; npx playwright test tests/supplyarr-purchasing-demand-processing-smoke.spec.ts
```

## Out of scope

- Live retry-processing / create-PR-draft mutations in Playwright
- Demand processing worker process-batch live run
- Settings panel Playwright (covered separately from W236 cluster)

## Next recommended slice

- **M13 Playwright** — SupplyArr procurement exceptions panel smoke (W250 optional)
- **M13 Playwright** — RoutArr dispatch notification explicit webhook clear on disable (optional)
- **SupplyArr M8** — procurement exception SLA escalation worker / notifications (W250 out of scope)
