# W324 — M13 Playwright: StaffArr reports workspace smoke

Builds on **W106/W126/W228** (audit package export — separate admin spec W238), **W317/W319/W323** (RoutArr/SupplyArr/TrainArr reports workspace Playwright pattern), and **W238** (StaffArr admin audit export smoke).

Completes consolidated **StaffArr reports** coverage for the `/reports` workspace: one browser session verifies personnel, readiness, and incident report panels and data exports load with headings, scope filters, export/download controls, and summary or empty states (no CSV download clicks).

## Scope

### Backend (StaffArr M12 reporting)

| API group | Endpoints |
|-----------|-----------|
| Personnel reports | `GET /api/reports/personnel/summary`, `GET /api/reports/personnel/summary/export` |
| Readiness reports | `GET /api/reports/readiness/summary`, `GET /api/reports/readiness/summary/export` |
| Incident reports | `GET /api/reports/incidents/summary`, `GET /api/reports/incidents/summary/export` |
| Entity exports | `GET /api/exports/manifest`, `GET /api/exports/people`, `personnel-incidents`, `person-certifications` |

Read auth mirrors audit package read (`RequireAuditPackageRead` / supervisor+); export auth mirrors audit package export (admin/HR).

### Frontend (`ReportsSection`)

| Test id | Element |
|---------|---------|
| `staffarr-reports-workspace` | Wrapper around personnel, readiness, incident report panels and data exports panel |

Reuses panel test ids: `personnel-reports-panel`, `readiness-reports-panel`, `incident-reports-panel`, `data-exports-panel`.

`AuditPackageExportPanel` remains on `/admin`; audit export depth covered by W238.

### Playwright (`tests/e2e-playwright`)

| Spec | Route | Coverage |
|------|-------|----------|
| `staffarr-reports-workspace-smoke.spec.ts` | `/reports` | Handoff → reports workspace visible; personnel (employment filter + Export CSV), readiness (attention filter + Export CSV), incident (open-only filter + Export CSV), data exports (≥3 Download CSV controls); loading states clear; summary metrics or empty states |

No CSV download clicks (depth covered by integration + Vitest tests).

### Vitest

- `ReportsSection.test.tsx` — reports workspace test id + all four panels for authorized admin; workspace omitted for unauthorized roles

### Integration

- `StaffArrReportTests.cs` — summary aggregates, manifest listing, supervisor read vs export forbidden

### Catalog

- `StlE2ePlaywrightSpecCatalog.StaffArrReportsWorkspaceSmokeSpec` in `ProductAdminSmokeSpecs` + `All`
- `StlE2ePlaywrightSpecCatalogTests.Product_admin_smoke_specs_include_maintainarr_through_compliancecore_w230_w324`

## Prerequisites (live)

```powershell
scripts/ops/e2e-stack-up.ps1
scripts/ops/e2e-frontends-preview.ps1
cd tests/e2e-playwright
$env:E2E_LIVE='1'
npx playwright test tests/staffarr-reports-workspace-smoke.spec.ts
```

Requires StaffArr API and frontend (5175). Demo admin / platform admin with report read + export permissions.

## Verification

```powershell
dotnet build STLCompliance.slnx -c Release
dotnet test tests/STLCompliance.E2E/STLCompliance.E2E.csproj -c Release --filter "FullyQualifiedName~PlaywrightSpecCatalog"
dotnet test tests/STLCompliance.StaffArr.Auth.Tests/STLCompliance.StaffArr.Auth.Tests.csproj -c Release --filter "FullyQualifiedName~StaffArrReport"
cd apps/staffarr-frontend
npm run test -- --run ReportsSection
cd ../../tests/e2e-playwright
npm install
# With live stack:
# $env:E2E_LIVE='1'; npx playwright test tests/staffarr-reports-workspace-smoke.spec.ts
```

## Out of scope

- Per-panel CSV download depth (integration tests)
- Audit package export depth (W238 on `/admin`)
- StaffArr settings admin workspace smoke (completed W325)

## Next recommended slice

- **M13 Playwright** — RoutArr dispatch/notification depth, Compliance Core operator journeys, or next milestone backlog item per `00_SLICE_STATE.md`
