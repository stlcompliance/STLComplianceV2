# W329 — M13 Playwright: Compliance Core settings admin workspace smoke

Builds on **W232** (suite handoff product admin Playwright pattern), **W240/W242** (audit delivery orchestration UI + smoke), **W231** (M12 analytics worker settings), **W222–225** (M12 analytics panels), **W220/W221** (source ingestion + rule change monitoring), **W35** (9-CSV import/export), **W316–W325** (other products' settings admin workspace pattern), and **W328** (operator batch workflow gate journey).

Completes consolidated **Compliance Core product-admin** coverage for the `/admin` workspace: one browser session verifies all nine tenant-admin panels load with headings, save/evaluate controls, and key history/list sections (no save mutations).

## Scope

### Frontend (`AdminSection`)

| Test id | Element |
|---------|---------|
| `compliancecore-settings-admin-workspace` | Wrapper around all nine product-admin panels |

Reuses existing panel test ids: `compliancecore-audit-delivery-orchestration-panel`, `compliancecore-m12-analytics-worker-settings-panel`, `readiness-forecast-panel`, `control-effectiveness-panel`, `missing-evidence-warnings-panel`, `risk-scoring-panel`, `rule-change-monitoring-panel`, `source-ingestion-panel`, `csv-import-export-panel`.

Added panel test ids aligned with RoutArr/SupplyArr/TrainArr workspace smokes:

- Analytics panels: `*-evaluate`, `*-list-empty`, `*-list`
- Rule change: `rule-change-events-empty`, `rule-change-events-list`
- Source ingestion: `source-ingestion-validate`, `source-ingestion-commit`, `source-ingestion-batches-empty`, `source-ingestion-batches-list`
- CSV: `csv-import-export-manifest`, `csv-import-export-download`
- Audit export: `compliancecore-audit-export-panel` (outside workspace wrapper)

`compliancecore-audit-export-panel` remains outside the admin workspace wrapper; audit export depth covered by W242/W232 separate specs.

### Playwright (`tests/e2e-playwright`)

| Spec | Route | Coverage |
|------|-------|----------|
| `compliancecore-settings-admin-workspace-smoke.spec.ts` | `/admin` | Handoff → admin workspace visible; all nine panels with headings + save/evaluate/download controls; orchestration status + analytics list + rule-change events + source batches sections loaded (empty or list) |

No settings save/evaluate mutations (depth covered by W232/W242/W231+).

### Vitest

- `AdminSection.test.tsx` — admin workspace test id + all nine panels for authorized admin; audit panel outside wrapper when `canExportAudit`; admin workspace omitted for unauthorized roles

### Catalog

- `StlE2ePlaywrightSpecCatalog.ComplianceCoreSettingsAdminWorkspaceSmokeSpec` in `ProductAdminSmokeSpecs` + `All`
- `StlE2ePlaywrightSpecCatalogTests.Product_admin_smoke_specs_include_maintainarr_through_compliancecore_w230_w329`

## Prerequisites (live)

```powershell
scripts/ops/e2e-stack-up.ps1
scripts/ops/e2e-frontends-preview.ps1
cd tests/e2e-playwright
$env:E2E_LIVE='1'
npx playwright test tests/compliancecore-settings-admin-workspace-smoke.spec.ts
```

Requires Compliance Core API (5107) and frontend (5177). Demo platform admin with compliance admin permissions.

## Verification

```powershell
dotnet build STLCompliance.slnx -c Release
dotnet test tests/STLCompliance.E2E/STLCompliance.E2E.csproj -c Release --filter "FullyQualifiedName~PlaywrightSpecCatalog"
cd apps/compliancecore-frontend
npm run test -- --run AdminSection
cd ../../tests/e2e-playwright
npm install
# With live stack:
# $env:E2E_LIVE='1'; npx playwright test tests/compliancecore-settings-admin-workspace-smoke.spec.ts
```

## Out of scope

- Per-panel save/reload/evaluate depth (W232/W242/W231/W222–225+)
- Audit package export depth (W242 separate spec)
- Operator journey smokes (W326–W328)
- SupplyArr procurement exception post-cancel reopen (no API)

## Next recommended slice

- **M13 Playwright** — Cross-product operator journey (Compliance Core gate → RoutArr dispatch assign), RoutArr dispatch/notification depth, or next milestone backlog item per `00_SLICE_STATE.md`

## Remaining M13 gaps

- Compliance Core has no dedicated `/reports` workspace Playwright smoke (unlike StaffArr W324, TrainArr W323, MaintainArr W322, SupplyArr W319, RoutArr W317)
- Cross-product operator journey end-to-end (Compliance Core gate → RoutArr dispatch assign) not yet Playwright-covered
- SupplyArr procurement exception post-cancel reopen blocked until API gains reopen support
