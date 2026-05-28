# W238 — M13 Playwright: StaffArr admin audit export smoke

Builds on **W232** (suite handoff product admin Playwright pattern), **W228** (StaffArr M12 personnel audit export enhancements), and **W138** (platform-admin audit export smoke with internal `process-batch` helper).

## Scope

### Playwright (`tests/e2e-playwright`)

| Spec | Route | Coverage |
|------|-------|----------|
| `staffarr-admin-audit-export-smoke.spec.ts` | `/admin` | Suite sign-in → handoff → `staffarr-audit-export-panel`: manifest sections, export summary counts, timeline (not loading), rich filters (`staffarr-audit-filter-*`), CSV/JSON download buttons, Preview JSON, sync ZIP download, Background ZIP job + `processStaffArrAuditPackageGenerationBatch` |

### e2eApi

- `processStaffArrAuditPackageGenerationBatch` — `POST /api/internal/audit-package-jobs/process-batch` with `staffarr.audit_packages.generate` service token

### Catalog

- `StlE2ePlaywrightSpecCatalog.StaffArrAdminAuditExportSmokeSpec` in `ProductAdminSmokeSpecs`
- `StlE2ePlaywrightSpecCatalogTests.Product_admin_smoke_specs_include_maintainarr_through_staffarr_w230_w238`

## Prerequisites (live)

```powershell
scripts/ops/e2e-stack-up.ps1
scripts/ops/e2e-frontends-preview.ps1
cd tests/e2e-playwright
$env:E2E_LIVE='1'
npx playwright test tests/staffarr-admin-audit-export-smoke.spec.ts
```

Requires StaffArr API (5102) and frontend (5175). Demo platform admin has `canExportAuditPackage`.

## Verification

```powershell
dotnet test tests/STLCompliance.E2E/STLCompliance.E2E.csproj -c Release --filter "FullyQualifiedName~PlaywrightSpecCatalog"
```

## Out of scope

- StaffArr Settings / person export panels
- CSV-only download assertion separate from button presence (ZIP download is asserted)

## Next slice

- **Compliance Core M12** — audit delivery orchestration UI (scheduled eval + M12 batch tie-in)
- **Suite M13** — TrainArr admin audit export Playwright (W165/W167 parity)
- **RoutArr** — dispatch exception queue Playwright (W210)
