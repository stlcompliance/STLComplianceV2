# W322 — M13 Playwright: MaintainArr reports workspace smoke

Builds on **W203–W207** (M12 maintenance/executive/compliance report panels + entity data exports), **W230/W132** (audit package export — separate settings spec), **W317/W319** (RoutArr/SupplyArr reports workspace Playwright pattern), and **W321** (MaintainArr settings admin workspace smoke).

Completes consolidated **MaintainArr reports** coverage for the `/reports` workspace: one browser session verifies all four M12 report/export panels load with headings, scope filters, export/download controls, and summary or empty states (no CSV download clicks).

## Scope

### Frontend (`ReportsSection`)

| Test id | Element |
|---------|---------|
| `maintainarr-reports-workspace` | Wrapper around compliance, executive, maintenance report panels and data exports panel |

Reuses existing panel test ids: `compliance-reports-panel`, `executive-reports-panel`, `maintenance-reports-panel`, `data-exports-panel`.

`maintainarr-audit-export-panel` remains on `/settings` under Settings; audit export depth covered by W230.

### Playwright (`tests/e2e-playwright`)

| Spec | Route | Coverage |
|------|-------|----------|
| `maintainarr-reports-workspace-smoke.spec.ts` | `/reports` | Handoff → reports workspace visible; compliance (attention filter + Export CSV), executive summary, maintenance (lifecycle filter + Export CSV), data exports (≥3 Download CSV controls); loading states clear; summary metrics or table rows present |

No CSV download clicks (depth covered by W203–W207 API/UI unit tests).

### Vitest

- `ReportsSection.test.tsx` — reports workspace test id + all four panels for authorized admin; workspace omitted for unauthorized roles

### Catalog

- `StlE2ePlaywrightSpecCatalog.MaintainArrReportsWorkspaceSmokeSpec` in `ProductAdminSmokeSpecs` + `All`
- `StlE2ePlaywrightSpecCatalogTests.Product_admin_smoke_specs_include_maintainarr_through_compliancecore_w230_w322`

## Prerequisites (live)

```powershell
scripts/ops/e2e-stack-up.ps1
scripts/ops/e2e-frontends-preview.ps1
cd tests/e2e-playwright
$env:E2E_LIVE='1'
npx playwright test tests/maintainarr-reports-workspace-smoke.spec.ts
```

Requires MaintainArr API and frontend (5178). Demo admin / platform admin with report read + export permissions.

## Verification

```powershell
dotnet build STLCompliance.slnx -c Release
dotnet test tests/STLCompliance.E2E/STLCompliance.E2E.csproj -c Release --filter "FullyQualifiedName~PlaywrightSpecCatalog"
cd apps/maintainarr-frontend
npm run test -- --run ReportsSection
cd ../../tests/e2e-playwright
npm install
# With live stack:
# $env:E2E_LIVE='1'; npx playwright test tests/maintainarr-reports-workspace-smoke.spec.ts
```

## Out of scope

- Per-panel CSV download depth (W203–W207 unit/API tests)
- Audit package export depth (W230 on `/settings`)
- Report panel save/mutation flows (read-only smoke)

## Next recommended slice

- **M13 Playwright** — RoutArr dispatch/notification depth, StaffArr reports workspace smoke, or next milestone backlog item per `00_SLICE_STATE.md`
