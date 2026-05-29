# W319 — M13 Playwright: SupplyArr reports workspace consolidation

Builds on **W232** (suite handoff product admin Playwright pattern), **W181–186** (SupplyArr M12 report panels: vendor, parts/inventory, purchasing, compliance, audit history), **W237** (initial vendor + purchasing reports smoke), and **W317** (RoutArr reports workspace wrapper pattern).

Completes consolidated **SupplyArr product-admin** coverage for the `/reports` M12 workspace: one browser session verifies all five report panels load with filters, export controls (where applicable), and summary or empty states (no CSV download clicks).

## Scope

### Frontend (`ReportsSection`)

| Test id | Element |
|---------|---------|
| `supplyarr-reports-workspace` | Wrapper around all five M12 report panels |

Reuses existing panel test ids: `vendor-reports-panel`, `parts-inventory-reports-panel`, `purchasing-reports-panel`, `compliance-reports-panel`, `audit-history-panel`.

### Playwright (`tests/e2e-playwright`)

| Spec | Route | Coverage |
|------|-------|----------|
| `supplyarr-reports-workspace-smoke.spec.ts` | `/reports` | Handoff → admin workspace visible; vendor (approval filter + active-only + Export CSV); parts/inventory (below-reorder filter + Export parts CSV); purchasing (open-documents filter + Export CSV); compliance (attention-only filter + Export CSV); audit history (filters + table rows or empty copy) |

No report CSV download clicks (export control presence only).

### Vitest

- `ReportsSection.test.tsx` — reports workspace test id + all five panels for `supplyarr_admin`; empty render for unauthorized roles

### Catalog

- `StlE2ePlaywrightSpecCatalog.SupplyArrReportsWorkspaceSmokeSpec` in `ProductAdminSmokeSpecs` + `All` (registered in W237; extended in W319)
- `StlE2ePlaywrightSpecCatalogTests.Product_admin_smoke_specs_include_maintainarr_through_compliancecore_w230_w319`

## Prerequisites (live)

```powershell
scripts/ops/e2e-stack-up.ps1
scripts/ops/e2e-frontends-preview.ps1
cd tests/e2e-playwright
$env:E2E_LIVE='1'
npx playwright test tests/supplyarr-reports-workspace-smoke.spec.ts
```

Requires SupplyArr API and frontend (5179). Demo admin / platform admin with report read + export roles.

## Verification

```powershell
dotnet build STLCompliance.slnx -c Release
dotnet test tests/STLCompliance.E2E/STLCompliance.E2E.csproj -c Release --filter "FullyQualifiedName~PlaywrightSpecCatalog"
cd apps/supplyarr-frontend
npm run test -- --run ReportsSection
cd ../../tests/e2e-playwright
npm install
# With live stack:
# $env:E2E_LIVE='1'; npx playwright test tests/supplyarr-reports-workspace-smoke.spec.ts
```

## Out of scope

- CSV download assertion for any report panel
- Row click-through detail panels (vendor/part/compliance party detail)
- Audit history pagination ("Load more") depth

## Next recommended slice

- **M13 Playwright** — TrainArr/MaintainArr settings admin workspace smokes (RoutArr W316 / SupplyArr W318 pattern), RoutArr dispatch/notification depth, or next milestone backlog item per `00_SLICE_STATE.md`
- SupplyArr procurement exception post-cancel reopen only if API gains reopen support
