# W239 — M13 Playwright: TrainArr settings audit export smoke

Builds on **W232** / **W238** (suite handoff product admin Playwright pattern), **W165** (TrainArr audit package export), and **W167** (async audit package generation worker).

## Scope

TrainArr hosts the audit export panel on the **Settings** workspace (`/settings`), not a separate Admin route.

### Playwright (`tests/e2e-playwright`)

| Spec | Route | Coverage |
|------|-------|----------|
| `trainarr-settings-audit-export-smoke.spec.ts` | `/settings` | Suite sign-in → handoff → `trainarr-audit-package-export-panel`: manifest sections, from/to date filters, Preview JSON summary counts, sync ZIP download, Background ZIP job + `processTrainArrAuditPackageGenerationBatch` |

### e2eApi

- `processTrainArrAuditPackageGenerationBatch` — `POST /api/internal/audit-package-jobs/process-batch` with `trainarr.audit_packages.generate` service token

### UI

- `data-job-status` on `audit-package-job-status` in `AuditPackageExportPanel` (parity with StaffArr/MaintainArr)

### Catalog

- `StlE2ePlaywrightSpecCatalog.TrainArrSettingsAuditExportSmokeSpec` in `ProductAdminSmokeSpecs`
- `StlE2ePlaywrightSpecCatalogTests.Product_admin_smoke_specs_include_maintainarr_through_trainarr_w230_w239`

## Prerequisites (live)

```powershell
scripts/ops/e2e-stack-up.ps1
scripts/ops/e2e-frontends-preview.ps1
cd tests/e2e-playwright
$env:E2E_LIVE='1'
npx playwright test tests/trainarr-settings-audit-export-smoke.spec.ts
```

Requires TrainArr API (5103) and frontend (5176). Demo platform admin has `canExportAuditPackage`.

## Verification

```powershell
dotnet test tests/STLCompliance.E2E/STLCompliance.E2E.csproj -c Release --filter "FullyQualifiedName~PlaywrightSpecCatalog"
```

## Out of scope

- Rich audit-event filters / CSV export (TrainArr panel is date-range + ZIP/JSON only per W165)
- Material demand panel (W234)

## Next slice

- **Compliance Core M12** — audit delivery orchestration UI
- **Suite M13** — RoutArr Reports audit export Playwright (W227)
- **SupplyArr** — Reports CSV download E2E (optional)
