# W241 — M13 Playwright: RoutArr Reports audit export smoke

Builds on **W232** / **W238** (suite handoff product admin Playwright pattern) and **W227** (RoutArr M12 audit package export on Reports workspace).

## Scope

### Playwright (`tests/e2e-playwright`)

| Spec | Route | Coverage |
|------|-------|----------|
| `routarr-reports-audit-export-smoke.spec.ts` | `/reports` | Suite sign-in → handoff → `routarr-audit-export-panel`: manifest sections, export summary counts, timeline (not loading), rich filters (`routarr-audit-filter-*`), CSV/JSON download buttons, Preview JSON, sync ZIP download, Background ZIP job + `processRoutArrAuditPackageGenerationBatch` |

### e2eApi

- `processRoutArrAuditPackageGenerationBatch` — `POST /api/internal/audit-package-jobs/process-batch` with `routarr.audit_packages.generate` service token

### Catalog

- `StlE2ePlaywrightSpecCatalog.RoutArrReportsAuditExportSmokeSpec` in `ProductAdminSmokeSpecs`
- `StlE2ePlaywrightSpecCatalogTests.Product_admin_smoke_specs_include_maintainarr_through_routarr_w230_w241`

## Prerequisites (live)

```powershell
scripts/ops/e2e-stack-up.ps1
scripts/ops/e2e-frontends-preview.ps1
cd tests/e2e-playwright
$env:E2E_LIVE='1'
npx playwright test tests/routarr-reports-audit-export-smoke.spec.ts
```

Requires RoutArr API (5105) and frontend (5180). Demo platform admin has `canExportDispatchReports`.

## Verification

```powershell
dotnet test tests/STLCompliance.E2E/STLCompliance.E2E.csproj -c Release --filter "FullyQualifiedName~PlaywrightSpecCatalog"
```

## Out of scope

- Dispatch command center panel (W235)
- Other Reports panels (dispatch/route/proof CSV exports)

## Next slice

- **Suite M13** — Compliance Core audit delivery orchestration Playwright (W240 panel)
- **SupplyArr** — Reports CSV download E2E (optional)
- **RoutArr** — dispatch exception queue Playwright (W210)
