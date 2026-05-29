# W325 — M13 Playwright: StaffArr settings admin workspace smoke

Builds on **W106/W119/W121/W128/W238** (audit export + M12 workers), **W324** (StaffArr reports workspace Playwright), and **W316–W321** (RoutArr/SupplyArr/TrainArr/MaintainArr settings admin workspace pattern).

Completes consolidated **StaffArr product-admin** coverage for the `/admin` workspace: one browser session verifies all six tenant-admin worker panels load with headings, save controls, and key pending/run/notification sections (no save mutations). Audit package export remains outside the wrapper (W238 depth).

## Scope

### Backend (StaffArr M12 worker admin)

| API group | Endpoints |
|-----------|-----------|
| Worker admin settings | `GET/PUT /api/worker-admin/{workerKey}/settings`, `GET /api/worker-admin/{workerKey}/pending`, `GET /api/worker-admin/{workerKey}/runs` |
| Export delivery observability | `GET /api/people/export/delivery-pending`, `GET /api/people/export/delivery-runs` |

Tenant worker settings stored in `staffarr_tenant_worker_settings`; optional run history in `staffarr_worker_runs`. Worker keys: `certification-expiration`, `readiness-rollup`, `permission-projection`, `personnel-history-rollup`, `audit-package-generation`.

Auth: `RequireWorkerAdminSettingsManage` (alias of people write — tenant/staffarr/hr admin).

### Frontend (`AdminSection`)

| Test id | Element |
|---------|---------|
| `staffarr-settings-admin-workspace` | Wrapper around all six product-admin panels |

Panel test ids: `person-export-delivery-settings-panel`, `certification-expiration-settings-panel`, `readiness-rollup-settings-panel`, `permission-projection-settings-panel`, `personnel-history-rollup-settings-panel`, `audit-package-generation-settings-panel`.

`staffarr-audit-export-panel` remains outside the admin workspace wrapper; audit export depth covered by W238.

### Playwright (`tests/e2e-playwright`)

| Spec | Route | Coverage |
|------|-------|----------|
| `staffarr-settings-admin-workspace-smoke.spec.ts` | `/admin` | Handoff → admin workspace visible; all six panels with headings + save buttons; export delivery pending/runs/notifications + worker pending/run sections loaded (empty or list) |

No settings save mutations (depth covered by W119/W46–W49/W156/W128 integration tests).

### Vitest

- `AdminSection.test.tsx` — admin workspace test id + all six panels for authorized admin; audit panel outside wrapper when `canExportAudit`; admin workspace omitted for unauthorized roles

### Integration

- `StaffArrWorkerAdminTests.cs` — settings default/upsert, pending preview, delivery runs list, supervisor forbidden

### Catalog

- `StlE2ePlaywrightSpecCatalog.StaffArrSettingsAdminWorkspaceSmokeSpec` in `ProductAdminSmokeSpecs` + `All`
- `StlE2ePlaywrightSpecCatalogTests.Product_admin_smoke_specs_include_maintainarr_through_compliancecore_w230_w325`

## Prerequisites (live)

```powershell
scripts/ops/e2e-stack-up.ps1
scripts/ops/e2e-frontends-preview.ps1
cd tests/e2e-playwright
$env:E2E_LIVE='1'
npx playwright test tests/staffarr-settings-admin-workspace-smoke.spec.ts
```

Requires StaffArr API (5102) and frontend (5175). Demo admin / platform admin with people write permissions.

## Verification

```powershell
dotnet build STLCompliance.slnx -c Release
dotnet test tests/STLCompliance.E2E/STLCompliance.E2E.csproj -c Release --filter "FullyQualifiedName~PlaywrightSpecCatalog"
dotnet test tests/STLCompliance.StaffArr.Auth.Tests/STLCompliance.StaffArr.Auth.Tests.csproj -c Release --filter "FullyQualifiedName~StaffArrWorkerAdmin"
cd apps/staffarr-frontend
npm run test -- --run AdminSection
cd ../../tests/e2e-playwright
npm install
# With live stack:
# $env:E2E_LIVE='1'; npx playwright test tests/staffarr-settings-admin-workspace-smoke.spec.ts
```

## Out of scope

- Per-panel save/reload depth (W119/W46–W49/W156/W128 integration tests)
- Audit package export depth (W238)
- Person export bundle manual export depth (HomePage `PersonExportPanel`)

## Next recommended slice

- **M13 Playwright** — RoutArr dispatch/notification depth, Compliance Core operator journeys, or next milestone backlog item per `00_SLICE_STATE.md`
