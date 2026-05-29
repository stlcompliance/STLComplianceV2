# W317 — M13 Playwright: RoutArr reports workspace smoke

Builds on **W232** (suite handoff product admin Playwright pattern), **W214/W215/W218** (dispatch/route/proof-DVIR M12 report panels), **W220** (entity data exports panel), **W237** (SupplyArr reports workspace smoke pattern), and **W241** (audit package export — out of scope here).

Completes consolidated **RoutArr product-admin** coverage for the `/reports` M12 workspace: one browser session verifies dispatch, route, proof-DVIR, and data-export panels load with scope filters, export controls, and summary or empty states (no CSV download clicks).

## Scope

### Frontend (`ReportsSection`)

| Test id | Element |
|---------|---------|
| `routarr-reports-workspace` | Wrapper around four M12 report panels (audit export panel remains outside; W241) |

Reuses existing panel test ids: `dispatch-reports-panel`, `route-reports-panel`, `proof-dvir-reports-panel`, `data-exports-panel`.

### Playwright (`tests/e2e-playwright`)

| Spec | Route | Coverage |
|------|-------|----------|
| `routarr-reports-workspace-smoke.spec.ts` | `/reports` | Handoff → admin workspace visible; dispatch/route/proof-DVIR panels with weekly scope, Export CSV enabled, summary metrics or empty copy; data exports manifest with ≥3 Download CSV buttons |

No report CSV or bulk entity download clicks (export control presence only; W241 covers audit ZIP/JSON depth).

### Vitest

- `ReportsSection.test.tsx` — reports workspace test id + four panels for `routarr_admin`; empty render for unauthorized roles

### Catalog

- `StlE2ePlaywrightSpecCatalog.RoutArrReportsWorkspaceSmokeSpec` in `ProductAdminSmokeSpecs` + `All`
- `StlE2ePlaywrightSpecCatalogTests.Product_admin_smoke_specs_include_maintainarr_through_compliancecore_w230_w317`

## Prerequisites (live)

```powershell
scripts/ops/e2e-stack-up.ps1
scripts/ops/e2e-frontends-preview.ps1
cd tests/e2e-playwright
$env:E2E_LIVE='1'
npx playwright test tests/routarr-reports-workspace-smoke.spec.ts
```

Requires RoutArr API and frontend (5180). Demo admin / platform admin with dispatch report read + export roles.

## Verification

```powershell
dotnet build STLCompliance.slnx -c Release
dotnet test tests/STLCompliance.E2E/STLCompliance.E2E.csproj -c Release --filter "FullyQualifiedName~PlaywrightSpecCatalog"
cd apps/routarr-frontend
npm run test -- --run ReportsSection
cd ../../tests/e2e-playwright
npm install
# With live stack:
# $env:E2E_LIVE='1'; npx playwright test tests/routarr-reports-workspace-smoke.spec.ts
```

## Out of scope

- Audit package export panel depth (W241)
- CSV download assertion for report rollups or entity exports
- Trip/route/proof row click-through detail panels

## Next recommended slice

- **M13 Playwright** — SupplyArr procurement exception post-cancel reopen only if API gains reopen support, RoutArr dispatch/notification depth, or next milestone backlog item per `00_SLICE_STATE.md`
