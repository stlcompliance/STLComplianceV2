# W330 — M13 Playwright: Compliance Core reports workspace smoke

Builds on **W232** (suite handoff product admin Playwright pattern), **W37/W242** (audit package export — separate admin spec), **W45** (operator dashboard), **W317/W324** (RoutArr/StaffArr reports workspace Playwright pattern), and **W329** (Compliance Core settings admin workspace smoke).

Completes consolidated **Compliance Core reports** coverage for the `/reports` workspace: one browser session verifies compliance (findings) and operator (evaluations/gates) report panels and data exports load with headings, scope filters, export/download controls, and summary or empty states (no CSV download clicks).

## Scope

### Backend (Compliance Core M12 reporting)

| API group | Endpoints |
|-----------|-----------|
| Findings reports | `GET /api/reports/findings/summary`, `GET /api/reports/findings/summary/export` |
| Operator reports | `GET /api/reports/operator/summary`, `GET /api/reports/operator/summary/export` |
| Entity exports | `GET /api/exports/manifest`, `GET /api/exports/findings`, `evaluations`, `rule-packs` |

Read auth mirrors findings read / operator dashboard read; export auth mirrors audit package export (admin/reviewer).

### Frontend (`ReportsSection`)

| Test id | Element |
|---------|---------|
| `compliancecore-reports-workspace` | Wrapper around compliance, operator report panels and data exports panel |

Reuses panel test ids: `compliance-reports-panel`, `operator-reports-panel`, `data-exports-panel`.

`AuditPackageExportPanel` remains on `/admin`; audit export depth covered by W242/W232 separate specs.

### Playwright (`tests/e2e-playwright`)

| Spec | Route | Coverage |
|------|-------|----------|
| `compliancecore-reports-workspace-smoke.spec.ts` | `/reports` | Handoff → reports workspace visible; compliance (severity filter + Export CSV), operator (attention filter + Export CSV), data exports (≥3 Download CSV controls); loading states clear; summary metrics or empty states |

No CSV download clicks (depth covered by integration + Vitest tests).

### Vitest

- `ReportsSection.test.tsx` — reports workspace test id + all three panels for authorized admin; workspace omitted for unauthorized roles

### Integration

- `ComplianceCoreReportTests.cs` — summary aggregates, manifest listing, member read vs export forbidden

### Catalog

- `StlE2ePlaywrightSpecCatalog.ComplianceCoreReportsWorkspaceSmokeSpec` in `ProductAdminSmokeSpecs` + `All`
- `StlE2ePlaywrightSpecCatalogTests.Product_admin_smoke_specs_include_maintainarr_through_compliancecore_w230_w330`

## Prerequisites (live)

```powershell
scripts/ops/e2e-stack-up.ps1
scripts/ops/e2e-frontends-preview.ps1
cd tests/e2e-playwright
$env:E2E_LIVE='1'
npx playwright test tests/compliancecore-reports-workspace-smoke.spec.ts
```

Requires Compliance Core API (5107) and frontend (5177). Demo platform admin with compliance report read + export permissions.

## Verification

```powershell
dotnet build STLCompliance.slnx -c Release
dotnet test tests/STLCompliance.E2E/STLCompliance.E2E.csproj -c Release --filter "FullyQualifiedName~PlaywrightSpecCatalog"
dotnet test tests/STLCompliance.ComplianceCore.Auth.Tests/STLCompliance.ComplianceCore.Auth.Tests.csproj -c Release --filter "FullyQualifiedName~ComplianceCoreReport"
cd apps/compliancecore-frontend
npm run test -- --run ReportsSection
cd ../../tests/e2e-playwright
npm install
# With live stack:
# $env:E2E_LIVE='1'; npx playwright test tests/compliancecore-reports-workspace-smoke.spec.ts
```

## Out of scope

- Per-panel CSV download depth (integration tests)
- Audit package export depth (W242 on `/admin`)
- Cross-product operator journey end-to-end (Compliance Core gate → RoutArr dispatch assign)
- SupplyArr procurement exception post-cancel reopen (no API)

## Next recommended slice

- **M13 Playwright** — RoutArr dispatch/notification depth, cross-product operator journey (Compliance Core gate → RoutArr dispatch assign), or next milestone backlog item per `00_SLICE_STATE.md`

## Remaining M13 gaps

- Cross-product operator journey end-to-end (Compliance Core gate → RoutArr dispatch assign) not yet Playwright-covered
- SupplyArr procurement exception post-cancel reopen blocked until API gains reopen support
