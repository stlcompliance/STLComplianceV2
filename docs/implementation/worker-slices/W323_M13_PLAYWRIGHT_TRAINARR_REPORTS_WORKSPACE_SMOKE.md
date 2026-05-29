# W323 — M13 Playwright: TrainArr reports workspace smoke

Builds on **W165/W167** (training audit package — separate settings spec W239), **W160/W219** (person training history), **W317/W319/W322** (RoutArr/SupplyArr/MaintainArr reports workspace Playwright pattern), and **W320** (TrainArr settings admin workspace smoke).

Completes consolidated **TrainArr reports** coverage for the `/reports` workspace: one browser session verifies assignment, qualification, compliance report panels and data exports load with headings, scope filters, export/download controls, and summary or empty states (no CSV download clicks).

## Scope

### Backend (TrainArr M12 reporting)

| API group | Endpoints |
|-----------|-----------|
| Assignment reports | `GET /api/reports/assignments/summary`, `GET /api/reports/assignments/summary/export` |
| Qualification reports | `GET /api/reports/qualifications/summary`, `GET /api/reports/qualifications/summary/export` |
| Compliance reports | `GET /api/reports/compliance/summary`, `GET /api/reports/compliance/summary/export` |
| Entity exports | `GET /api/exports/manifest`, `GET /api/exports/training-assignments`, `qualification-issues`, `training-definitions` |

Read auth mirrors audit package read (admin/trainer); export auth mirrors audit package export (admin).

### Frontend (`ReportsSection`)

| Test id | Element |
|---------|---------|
| `trainarr-reports-workspace` | Wrapper around assignment, qualification, compliance report panels and data exports panel |

Reuses panel test ids: `assignment-reports-panel`, `qualification-reports-panel`, `compliance-reports-panel`, `data-exports-panel`.

`AuditPackageExportPanel` remains on `/settings`; audit export depth covered by W239.

### Playwright (`tests/e2e-playwright`)

| Spec | Route | Coverage |
|------|-------|----------|
| `trainarr-reports-workspace-smoke.spec.ts` | `/reports` | Handoff → reports workspace visible; assignment (status + overdue filter + Export CSV), qualification (status filter + Export CSV), compliance (attention filter + Export CSV), data exports (≥3 Download CSV controls); loading states clear; summary metrics or empty states |

No CSV download clicks (depth covered by integration + Vitest tests).

### Vitest

- `ReportsSection.test.tsx` — reports workspace test id + all four panels for authorized admin; workspace omitted for unauthorized roles

### Catalog

- `StlE2ePlaywrightSpecCatalog.TrainArrReportsWorkspaceSmokeSpec` in `ProductAdminSmokeSpecs` + `All`
- `StlE2ePlaywrightSpecCatalogTests.Product_admin_smoke_specs_include_maintainarr_through_compliancecore_w230_w323`

## Prerequisites (live)

```powershell
scripts/ops/e2e-stack-up.ps1
scripts/ops/e2e-frontends-preview.ps1
cd tests/e2e-playwright
$env:E2E_LIVE='1'
npx playwright test tests/trainarr-reports-workspace-smoke.spec.ts
```

Requires TrainArr API and frontend (5176). Demo admin / platform admin with report read + export permissions.

## Verification

```powershell
dotnet build STLCompliance.slnx -c Release
dotnet test tests/STLCompliance.E2E/STLCompliance.E2E.csproj -c Release --filter "FullyQualifiedName~PlaywrightSpecCatalog"
dotnet test tests/STLCompliance.StaffArr.Auth.Tests/STLCompliance.StaffArr.Auth.Tests.csproj -c Release --filter "FullyQualifiedName~TrainArrReport"
cd apps/trainarr-frontend
npm run test -- --run ReportsSection
cd ../../tests/e2e-playwright
npm install
# With live stack:
# $env:E2E_LIVE='1'; npx playwright test tests/trainarr-reports-workspace-smoke.spec.ts
```

## Out of scope

- Per-panel CSV download depth (integration tests)
- Audit package export depth (W239 on `/settings`)
- Report panel save/mutation flows (read-only smoke)

## Next recommended slice

- **M13 Playwright** — RoutArr dispatch/notification depth, StaffArr reports workspace smoke (if applicable), or next milestone backlog item per `00_SLICE_STATE.md`
